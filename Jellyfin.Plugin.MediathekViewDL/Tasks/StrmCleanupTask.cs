using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to clean up invalid .strm files.
/// </summary>
public class StrmCleanupTask : IScheduledTask
{
    private const long MaxStrmFileSize = 4096; // 4 KB max size for .strm files to prevent accidents
    private const int MaxConcurrentValidations = 4;

    private static readonly string[] VideoExtensions =
    [
        ".mkv",
        ".mp4",
        ".avi",
        ".mov",
        ".ts",
        ".m4v",
        ".webm",
    ];

    private static readonly string[] SidecarSuffixes =
    [
        ".nfo",
        "-thumb.jpg",
        "-thumb.png",
        ".srt",
        ".vtt",
    ];

    private readonly ILogger<StrmCleanupTask> _logger;
    private readonly IUrlValidationService _validationService;
    private readonly IConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrmCleanupTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="validationService">The validation service.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    public StrmCleanupTask(ILogger<StrmCleanupTask> logger, IUrlValidationService validationService, IConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _validationService = validationService;
        _configurationProvider = configurationProvider;
    }

    /// <inheritdoc />
    public string Name => "Mediathek .strm Bereinigung";

    /// <inheritdoc />
    public string Key => "MediathekStrmCleanup";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Überprüft .strm Dateien auf Gültigkeit und löscht verwaiste Links.";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo { Type = TaskTriggerInfoType.IntervalTrigger, IntervalTicks = TimeSpan.FromHours(24).Ticks };
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            _logger.LogError(".strm cleanup task aborted because the plugin failed to initialize: {ErrorMessage}", Plugin.Instance.InitializationException.Message);
            return;
        }

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null || !config.Maintenance.EnableStrmCleanup)
        {
            _logger.LogInformation("Strm cleanup task is disabled in configuration or config is missing. Skipping.");
            return;
        }

        _logger.LogInformation("Starting .strm cleanup task.");
        progress.Report(0);

        var pathList = CollectDistinctRootPaths(config);
        if (pathList.Count == 0)
        {
            _logger.LogInformation("No download paths configured. Strm cleanup task finished.");
            progress.Report(100);
            return;
        }

        var enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
        };

        var strmFiles = new List<string>();
        foreach (var path in pathList)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogDebug("Configured path not found during .strm cleanup: {Path}", path);
                continue;
            }

            try
            {
                var found = Directory.EnumerateFiles(path, "*.strm", enumerationOptions).ToList();
                _logger.LogInformation("Found {Count} .strm files in '{Path}'.", found.Count, path);
                strmFiles.AddRange(found);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning directory '{Path}'.", path);
            }
        }

        var totalFiles = strmFiles.Count;
        if (totalFiles == 0)
        {
            _logger.LogInformation("No .strm files found across {PathCount} configured path(s).", pathList.Count);
            progress.Report(100);
            return;
        }

        var filesProcessed = 0;
        var filesDeleted = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxConcurrentValidations,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(strmFiles, parallelOptions, async (filePath, ct) =>
        {
            var deleted = await ProcessStrmFileAsync(filePath, ct).ConfigureAwait(false);
            if (deleted)
            {
                Interlocked.Increment(ref filesDeleted);
            }

            var processed = Interlocked.Increment(ref filesProcessed);
            progress.Report((double)processed / totalFiles * 100.0);
        }).ConfigureAwait(false);

        _logger.LogInformation("Strm cleanup task finished. Processed {Processed} files, deleted {Deleted} files.", filesProcessed, filesDeleted);
        progress.Report(100);
    }

    internal static List<string> CollectDistinctRootPaths(PluginConfiguration config)
    {
        var rawPaths = new[]
        {
            config.Paths.DefaultDownloadPath,
            config.Paths.DefaultSubscriptionShowPath,
            config.Paths.DefaultSubscriptionMoviePath,
            config.Paths.DefaultManualShowPath,
            config.Paths.DefaultManualMoviePath,
            config.VirtualLibrary.VirtualLibraryPath,
        }
        .Concat(config.Subscriptions.Select(s => s.Download.DownloadPath))
        .Where(p => !string.IsNullOrWhiteSpace(p))
        .Select(p => Path.GetFullPath(p.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(p => p.Length)
        .ToList();

        var rootPaths = new List<string>();
        foreach (var candidate in rawPaths)
        {
            var isNested = rootPaths.Any(parent =>
                candidate.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith(parent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

            if (!isNested)
            {
                rootPaths.Add(candidate);
            }
        }

        return rootPaths;
    }

    private async Task<bool> ProcessStrmFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return false;
            }

            if (fileInfo.Length > MaxStrmFileSize)
            {
                _logger.LogWarning("Skipping file '{Path}' because it is larger than {Max} bytes ({Size}).", filePath, MaxStrmFileSize, fileInfo.Length);
                return false;
            }

            var url = (await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false)).Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogInformation("Deleting empty .strm file: '{Path}'.", filePath);
                return TryDeleteStrmAndSidecars(filePath);
            }

            bool isValid;
            try
            {
                isValid = await _validationService.ValidateUrlAsync(url, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                // Skip non-Mediathek .strm files (e.g. trailer.strm pointing to YouTube or custom schemes).
                _logger.LogDebug("Skipping non-Mediathek .strm file '{Path}': {Reason}", filePath, ex.Message);
                return false;
            }

            if (!isValid)
            {
                _logger.LogInformation("Deleting invalid .strm file: '{Path}' (URL: {Url})", filePath, url);
                return TryDeleteStrmAndSidecars(filePath);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not validate .strm file '{Path}'; keeping file.", filePath);
        }

        return false;
    }

    private bool TryDeleteStrmAndSidecars(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            _logger.LogWarning(ex, "Failed to delete invalid .strm file '{Path}' (possibly read-only mount).", filePath);
            return false;
        }

        CleanupOrphanSidecars(filePath);
        return true;
    }

    private void CleanupOrphanSidecars(string strmPath)
    {
        var directory = Path.GetDirectoryName(strmPath);
        var stem = Path.GetFileNameWithoutExtension(strmPath);
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(stem))
        {
            return;
        }

        var basePath = Path.Combine(directory, stem);
        if (VideoExtensions.Any(ext => File.Exists(basePath + ext)))
        {
            return;
        }

        foreach (var suffix in SidecarSuffixes)
        {
            var sidecarPath = basePath + suffix;
            if (!File.Exists(sidecarPath))
            {
                continue;
            }

            try
            {
                File.Delete(sidecarPath);
                _logger.LogDebug("Deleted orphan sidecar file: '{Path}'.", sidecarPath);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                _logger.LogDebug(ex, "Could not delete sidecar file '{Path}'.", sidecarPath);
            }
        }
    }
}
