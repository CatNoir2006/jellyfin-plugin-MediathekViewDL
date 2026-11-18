using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Service to merge media instances across local files, Seerr downloads, and Mediathek streams.
/// Prioritizes local disk copies (Prio 1) and consolidates or supersedes .strm links (Prio 2).
/// </summary>
public class MediaMergeService : IMediaMergeService
{
    private static readonly HashSet<string> PhysicalVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mkv", ".mp4", ".avi", ".m4v", ".mov", ".wmv", ".ts", ".m2ts", ".webm"
    };

    private static readonly Regex SeasonEpRegex = new(@"[sS](?<season>\d{1,2})[eE](?<episode>\d{1,3})", RegexOptions.Compiled);
    private static readonly Regex SeasonDirRegex = new(@"Season\s*(?<season>\d{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ILogger<MediaMergeService> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ILibraryManager? _libraryManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMergeService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="libraryManager">The library manager.</param>
    public MediaMergeService(
        ILogger<MediaMergeService> logger,
        IConfigurationProvider configurationProvider,
        ILibraryManager? libraryManager = null)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _libraryManager = libraryManager;
    }

    /// <inheritdoc />
    public async Task<int> MergeMediaInstancesAsync(CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null || !config.Merging.EnableInstanceMerging)
        {
            _logger.LogDebug("Media instance merging is disabled.");
            return 0;
        }

        var candidateDirs = CollectCandidateDirectories(config);
        if (candidateDirs.Count == 0)
        {
            _logger.LogInformation("No candidate media directories found for instance merging.");
            return 0;
        }

        var totalProcessed = 0;
        var index = 0;

        foreach (var dir in candidateDirs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            index++;
            progress?.Report((double)index / candidateDirs.Count * 100);

            try
            {
                var processed = await ProcessDirectoryMergingAsync(dir, cancellationToken).ConfigureAwait(false);
                totalProcessed += processed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to merge instances in directory '{Dir}'.", dir);
            }
        }

        if (totalProcessed > 0)
        {
            _logger.LogInformation("Merged/cleaned up {Count} media instances.", totalProcessed);
            _libraryManager?.QueueLibraryScan();
        }

        return totalProcessed;
    }

    /// <inheritdoc />
    public Task<int> ProcessDirectoryMergingAsync(string directoryPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return Task.FromResult(0);
        }

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return Task.FromResult(0);
        }

        var processedCount = 0;

        try
        {
            // Map physical episodes (season, episode)
            var localEpisodes = new HashSet<(int Season, int Episode)>();
            var allFiles = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories).ToList();

            foreach (var f in allFiles)
            {
                var ext = Path.GetExtension(f);
                if (!PhysicalVideoExtensions.Contains(ext))
                {
                    continue;
                }

                var fName = Path.GetFileNameWithoutExtension(f);
                var match = SeasonEpRegex.Match(fName);
                if (match.Success &&
                    int.TryParse(match.Groups["season"].Value, out var s) &&
                    int.TryParse(match.Groups["episode"].Value, out var e))
                {
                    localEpisodes.Add((s, e));
                }
            }

            // Inspect .strm files
            var strmFiles = allFiles.Where(f => string.Equals(Path.GetExtension(f), ".strm", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var strm in strmFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fName = Path.GetFileNameWithoutExtension(strm);
                var match = SeasonEpRegex.Match(fName);

                int season = 0;
                int episode = 0;
                bool matched = false;

                if (match.Success &&
                    int.TryParse(match.Groups["season"].Value, out var s) &&
                    int.TryParse(match.Groups["episode"].Value, out var e))
                {
                    season = s;
                    episode = e;
                    matched = true;
                }
                else
                {
                    // Check parent directory like "Season 01"
                    var parentDirName = Path.GetFileName(Path.GetDirectoryName(strm) ?? string.Empty);
                    var dirMatch = SeasonDirRegex.Match(parentDirName);
                    if (dirMatch.Success && int.TryParse(dirMatch.Groups["season"].Value, out var dirSeason))
                    {
                        var epMatch = Regex.Match(fName, @"\b(?<episode>\d{1,3})\b");
                        if (epMatch.Success && int.TryParse(epMatch.Groups["episode"].Value, out var ep))
                        {
                            season = dirSeason;
                            episode = ep;
                            matched = true;
                        }
                    }
                }

                if (!matched)
                {
                    continue;
                }

                // If physical video already exists for this (season, episode)
                if (localEpisodes.Contains((season, episode)))
                {
                    if (config.Merging.AutoDeleteStrmWhenLocalExists)
                    {
                        // Delete stale .strm and .nfo so physical video (Prio 1) wins cleanly
                        try
                        {
                            File.Delete(strm);
                            var nfo = Path.ChangeExtension(strm, ".nfo");
                            if (File.Exists(nfo))
                            {
                                File.Delete(nfo);
                            }

                            _logger.LogInformation("Deleted duplicate stream pointer S{Season:D2}E{Episode:D2} because physical video exists: {Path}", season, episode, strm);
                            processedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not delete duplicate stream file {Path}", strm);
                        }
                    }
                    else if (config.Merging.GroupAsMultiVersion && !fName.Contains("[Mediathek Stream]", StringComparison.OrdinalIgnoreCase))
                    {
                        // Rename to Jellyfin multi-version syntax
                        try
                        {
                            var dir = Path.GetDirectoryName(strm)!;
                            var newFileName = $"{fName} - [Mediathek Stream].strm";
                            var newPath = Path.Combine(dir, newFileName);
                            if (!File.Exists(newPath))
                            {
                                File.Move(strm, newPath);
                                var oldNfo = Path.ChangeExtension(strm, ".nfo");
                                var newNfo = Path.ChangeExtension(newPath, ".nfo");
                                if (File.Exists(oldNfo) && !File.Exists(newNfo))
                                {
                                    File.Move(oldNfo, newNfo);
                                }

                                _logger.LogInformation("Renamed duplicate stream pointer to multi-version syntax: {Path}", newPath);
                                processedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not rename multi-version stream file {Path}", strm);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error processing directory merging for {Path}", directoryPath);
        }

        return Task.FromResult(processedCount);
    }

    private List<string> CollectCandidateDirectories(PluginConfiguration config)
    {
        var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var showPath = !string.IsNullOrWhiteSpace(config.Paths.DefaultSubscriptionShowPath)
            ? config.Paths.DefaultSubscriptionShowPath
            : config.Paths.DefaultDownloadPath;

        if (!string.IsNullOrWhiteSpace(showPath) && Directory.Exists(showPath))
        {
            dirs.Add(showPath);
        }

        var moviePath = !string.IsNullOrWhiteSpace(config.Paths.DefaultSubscriptionMoviePath)
            ? config.Paths.DefaultSubscriptionMoviePath
            : config.Paths.DefaultDownloadPath;

        if (!string.IsNullOrWhiteSpace(moviePath) && Directory.Exists(moviePath))
        {
            dirs.Add(moviePath);
        }

        if (!string.IsNullOrWhiteSpace(config.VirtualLibrary.VirtualLibraryPath) && Directory.Exists(config.VirtualLibrary.VirtualLibraryPath))
        {
            dirs.Add(config.VirtualLibrary.VirtualLibraryPath);
        }

        foreach (var sub in config.Subscriptions)
        {
            if (!string.IsNullOrWhiteSpace(sub.Download.DownloadPath) && Directory.Exists(sub.Download.DownloadPath))
            {
                dirs.Add(sub.Download.DownloadPath);
            }
        }

        if (_libraryManager != null)
        {
            try
            {
                foreach (var vf in _libraryManager.GetVirtualFolders())
                {
                    if (vf.Locations != null)
                    {
                        foreach (var loc in vf.Locations)
                        {
                            if (Directory.Exists(loc))
                            {
                                dirs.Add(loc);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to inspect Jellyfin library virtual folders.");
            }
        }

        return dirs.ToList();
    }
}
