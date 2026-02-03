using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Deletes temporary files created by the plugin.
/// </summary>
public class TempFileCleanup : IScheduledTask
{
    private readonly ILogger<TempFileCleanup> _logger;
    private readonly IServerApplicationPaths _appPaths;
    private readonly IConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TempFileCleanup"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="appPaths">The application paths.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    public TempFileCleanup(
        ILogger<TempFileCleanup> logger,
        IServerApplicationPaths appPaths,
        IConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _appPaths = appPaths;
        _configurationProvider = configurationProvider;
    }

    /// <inheritdoc />
    public string Name => "Temporäre Dateien Bereinigen";

    /// <inheritdoc />
    public string Key => "MediathekTempFileCleanup";

    /// <inheritdoc />
    public string Description => "Löscht vom Plugin erstellte Temporäre Dateien (*.mvdl-tmp) bei Plugin start.";

    /// <inheritdoc />
    public string Category => Constants.SchedTaskCat;

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo() { Type = TaskTriggerInfoType.StartupTrigger };
    }

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting temporary file cleanup.");
        progress.Report(0);

        var config = _configurationProvider.ConfigurationOrNull;
        var directoriesToScan = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. System/Jellyfin Temp Directory (Fallback location)
        if (!string.IsNullOrWhiteSpace(_appPaths.TempDirectory))
        {
            directoriesToScan.Add(_appPaths.TempDirectory);
        }

        if (config != null)
        {
            // 2. Configured Plugin Temp Directory
            if (!string.IsNullOrWhiteSpace(config.Paths.TempDownloadPath))
            {
                directoriesToScan.Add(config.Paths.TempDownloadPath);
            }

            // 3. Default Download Directory (if no temp dir configured, temp files might be here)
            if (!string.IsNullOrWhiteSpace(config.Paths.DefaultDownloadPath))
            {
                directoriesToScan.Add(config.Paths.DefaultDownloadPath);
            }

            // 4. Subscription Download Directories
            foreach (var subscription in config.Subscriptions)
            {
                if (!string.IsNullOrWhiteSpace(subscription.Download.DownloadPath))
                {
                    directoriesToScan.Add(subscription.Download.DownloadPath);
                }
            }
        }

        var totalDirs = directoriesToScan.Count;
        var processedDirs = 0;
        var deletedCount = 0;

        foreach (var dir in directoriesToScan)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                if (Directory.Exists(dir))
                {
                    var files = Directory.GetFiles(dir, "*.mvdl-tmp", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            File.Delete(file);
                            deletedCount++;
                            _logger.LogInformation("Deleted temporary file: {File}", file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temporary file: {File}", file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning directory for temporary files: {Dir}", dir);
            }

            processedDirs++;
            progress.Report((double)processedDirs / totalDirs * 100);
        }

        progress.Report(100);
        _logger.LogInformation("Temporary file cleanup finished. Deleted {Count} files.", deletedCount);
        return Task.CompletedTask;
    }
}
