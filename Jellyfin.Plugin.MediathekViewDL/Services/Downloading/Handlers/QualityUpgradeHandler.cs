using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Helpers;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Handler for quality upgrades.
/// </summary>
public class QualityUpgradeHandler : IDownloadHandler
{
    private readonly ILogger<QualityUpgradeHandler> _logger;
    private readonly IFileDownloader _fileDownloader;
    private readonly IConfigurationProvider _configProvider;
    private readonly IServerApplicationPaths _appPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="QualityUpgradeHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="appPaths">The application paths.</param>
    public QualityUpgradeHandler(
        ILogger<QualityUpgradeHandler> logger,
        IFileDownloader fileDownloader,
        IConfigurationProvider configProvider,
        IServerApplicationPaths appPaths)
    {
        _logger = logger;
        _fileDownloader = fileDownloader;
        _configProvider = configProvider;
        _appPaths = appPaths;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.QualityUpgrade;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting quality upgrade download for '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
        return await DoQualityUpgrade(item, progress, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> DoQualityUpgrade(DownloadItem item, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var tempPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mkv", _configProvider, _appPaths, _logger);
        try
        {
            if (!await _fileDownloader.DownloadFileAsync(item.SourceUrl, tempPath, progress, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            // Determine the actual file that needs to be replaced on disk.
            // This could be different from DestinationPath if the filename/extension has changed.
            string fileToReplace = item.ReplaceFilePath ?? item.DestinationPath;

            if (!File.Exists(tempPath) || !File.Exists(fileToReplace))
            {
                _logger.LogError("Either temporary file '{TempPath}' or the file to replace '{FileToReplace}' does not exist for quality upgrade.", tempPath, fileToReplace);
                return false;
            }

            var tempFile = new FileInfo(tempPath);
            var existingFile = new FileInfo(fileToReplace);

            if (tempFile.Length <= existingFile.Length)
            {
                _logger.LogInformation("Downloaded file '{TempPath}' is not larger than existing file '{ExistingFile}'. Skipping upgrade.", tempPath, fileToReplace);
                return false;
            }

            _logger.LogInformation(
                "Upgrading file '{FileToReplace}' to '{DestinationPath}' with higher quality download from '{TempPath}'.",
                fileToReplace,
                item.DestinationPath,
                tempPath);

            if (Path.GetFullPath(fileToReplace).Equals(Path.GetFullPath(item.DestinationPath), StringComparison.OrdinalIgnoreCase))
            {
                // Destination and file to replace are the same (e.g., same name, same extension)
                var backupPath = $"{item.DestinationPath}.{Guid.NewGuid()}.bak";
                File.Move(item.DestinationPath, backupPath); // item.DestinationPath is safe because it's same as fileToReplace
                File.Move(tempPath, item.DestinationPath);
                File.Delete(backupPath);
            }
            else
            {
                // Destination and file to replace are different (e.g., different extension, name changed)
                // Ensure the target directory for the new file exists
                var destinationDirectory = Path.GetDirectoryName(item.DestinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Move the new file FIRST
                File.Move(tempPath, item.DestinationPath);

                // Then delete the old file
                try
                {
                    if (File.Exists(fileToReplace))
                    {
                        File.Delete(fileToReplace);
                        _logger.LogInformation("Deleted old file '{FileToReplace}' after successful upgrade.", fileToReplace);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old file '{FileToReplace}'. The new file is already in place at '{DestinationPath}'.", fileToReplace, item.DestinationPath);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quality upgrade download for '{DestPath}'.", item.DestinationPath);
            throw new InvalidOperationException("Error during quality upgrade download.", ex);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary file '{TempPath}'.", tempPath);
                }
            }
        }
    }
}
