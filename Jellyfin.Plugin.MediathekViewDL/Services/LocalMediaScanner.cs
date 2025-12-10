using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service to scan local directories for existing episodes.
/// </summary>
public class LocalMediaScanner
{
    private readonly ILogger<LocalMediaScanner> _logger;
    private readonly VideoParser _videoParser;

    // Supported video extensions
    private readonly string[] _videoExtensions = { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".m4v", ".strm" };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalMediaScanner"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="videoParser">The video parser.</param>
    public LocalMediaScanner(ILogger<LocalMediaScanner> logger, VideoParser videoParser)
    {
        _logger = logger;
        _videoParser = videoParser;
    }

    /// <summary>
    /// Scans the specified directory for video files and builds a cache of existing episodes.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to scan.</param>
    /// <param name="seriesName">The name of the series (used for parsing context).</param>
    /// <returns>A <see cref="LocalEpisodeCache"/> containing the found episodes.</returns>
    public LocalEpisodeCache ScanDirectory(string directoryPath, string seriesName)
    {
        var cache = new LocalEpisodeCache();

        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            _logger.LogDebug("Directory does not exist or is invalid: {Path}", directoryPath);
            return cache;
        }

        try
        {
            _logger.LogInformation("Scanning local directory for existing episodes: {Path}", directoryPath);

            var files = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(f => _videoExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                // Parse the filename
                var videoInfo = _videoParser.ParseVideoInfo(seriesName, fileName);

                if (videoInfo != null)
                {
                    if ((videoInfo.SeasonNumber.HasValue && videoInfo.EpisodeNumber.HasValue) || videoInfo.AbsoluteEpisodeNumber.HasValue)
                    {
                        cache.Add(videoInfo.SeasonNumber, videoInfo.EpisodeNumber, videoInfo.AbsoluteEpisodeNumber, videoInfo.Language);
                        _logger.LogTrace(
                            "Found existing episode: {FileName} -> S{Season}E{Episode} (Abs: {Abs}) [{Lang}]",
                            fileName,
                            videoInfo.SeasonNumber,
                            videoInfo.EpisodeNumber,
                            videoInfo.AbsoluteEpisodeNumber,
                            videoInfo.Language);
                    }
                }
            }

            _logger.LogInformation(
                "Scan complete. Found {SECount} S/E episodes and {AbsCount} absolute numbered episodes.",
                cache.SeasonEpisodeCount,
                cache.AbsoluteEpisodeCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning directory: {Path}", directoryPath);
        }

        return cache;
    }
}
