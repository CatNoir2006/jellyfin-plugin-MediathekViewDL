using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services;
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
    private readonly ILogger<StrmCleanupTask> _logger;
    private readonly IStrmValidationService _validationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrmCleanupTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="validationService">The validation service.</param>
    public StrmCleanupTask(ILogger<StrmCleanupTask> logger, IStrmValidationService validationService)
    {
        _logger = logger;
        _validationService = validationService;
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
        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.EnableStrmCleanup)
        {
            _logger.LogInformation("Strm cleanup task is disabled in configuration or config is missing. Skipping.");
            return;
        }

        _logger.LogInformation("Starting .strm cleanup task.");
        progress.Report(0);

        var subscriptions = config.Subscriptions.Where(s => s.IsEnabled).ToList();
        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("No active subscriptions found. Task finished.");
            return;
        }

        // Collect all distinct download paths
        var paths = new HashSet<string>();

        // Add default download path
        if (!string.IsNullOrWhiteSpace(config.DefaultDownloadPath))
        {
            paths.Add(config.DefaultDownloadPath);
        }

        // Add subscription specific paths
        foreach (var sub in subscriptions)
        {
            if (!string.IsNullOrWhiteSpace(sub.DownloadPath))
            {
                paths.Add(sub.DownloadPath);
            }
        }

        var pathList = paths.ToList();
        var totalPaths = pathList.Count;
        var filesProcessed = 0;
        var filesDeleted = 0;

        for (int i = 0; i < totalPaths; i++)
        {
            var path = pathList[i];
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Directory not found: {Path}", path);
                continue;
            }

            try
            {
                var strmFiles = Directory.GetFiles(path, "*.strm", SearchOption.AllDirectories);
                var totalFiles = strmFiles.Length;

                _logger.LogInformation("Found {Count} .strm files in '{Path}'.", totalFiles, path);

                for (int j = 0; j < totalFiles; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var filePath = strmFiles[j];
                    var fileInfo = new FileInfo(filePath);

                    // Safety check: File size
                    if (fileInfo.Length > MaxStrmFileSize)
                    {
                        _logger.LogWarning("Skipping file '{Path}' because it is larger than {Max} bytes ({Size}).", filePath, MaxStrmFileSize, fileInfo.Length);
                        continue;
                    }

                    try
                    {
                        var url = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
                        url = url.Trim();

                        var isValid = await _validationService.ValidateUrlAsync(url, cancellationToken).ConfigureAwait(false);

                        if (!isValid)
                        {
                            _logger.LogInformation("Deleting invalid .strm file: '{Path}' (URL: {Url})", filePath, url);
                            File.Delete(filePath);
                            filesDeleted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file '{Path}'.", filePath);
                    }

                    filesProcessed++;

                    // Calculate progress based on paths processed + files processed within current path
                    // This is a rough estimation
                    double pathProgress = (double)i / totalPaths * 100;
                    double fileProgress = (double)(j + 1) / totalFiles * (100.0 / totalPaths);
                    progress.Report(pathProgress + fileProgress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning directory '{Path}'.", path);
            }
        }

        _logger.LogInformation("Strm cleanup task finished. Processed {Processed} files, deleted {Deleted} files.", filesProcessed, filesDeleted);
        progress.Report(100);
    }
}
