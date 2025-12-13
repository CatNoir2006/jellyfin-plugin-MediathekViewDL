using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Service responsible for executing download jobs.
/// </summary>
public class DownloadManager : IDownloadManager
{
    private readonly ILogger<DownloadManager> _logger;
    private readonly IFileDownloader _fileDownloader;
    private readonly IFFmpegService _ffmpegService;
    private readonly IServerApplicationPaths _appPaths;
    private readonly INfoService _nfoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    /// <param name="ffmpegService">The FFmpeg service.</param>
    /// <param name="appPaths">The application paths.</param>
    /// <param name="nfoService">The NFO service.</param>
    public DownloadManager(
        ILogger<DownloadManager> logger,
        IFileDownloader fileDownloader,
        IFFmpegService ffmpegService,
        IServerApplicationPaths appPaths,
        INfoService nfoService)
    {
        _logger = logger;
        _fileDownloader = fileDownloader;
        _ffmpegService = ffmpegService;
        _appPaths = appPaths;
        _nfoService = nfoService;
    }

    /// <summary>
    /// Executes a single download job.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the job was successful (or file already existed), otherwise false.</returns>
    public async Task<bool> ExecuteJobAsync(DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting download job for '{Title}'.", job.Title);
        var success = true;

        foreach (var item in job.DownloadItems)
        {
            _logger.LogInformation("Processing download item: {Type} -> {Path}", item.JobType, item.DestinationPath);
            if (File.Exists(item.DestinationPath) && item.JobType != DownloadType.QualityUpgrade)
            {
                _logger.LogDebug("File '{Path}' already exists. Skipping download.", item.DestinationPath);
                // Even if file exists, we might want to check for NFO if configured, but typically we assume "done is done".
                // However, user might have just enabled NFO.
                // For now, we only generate NFO on successful download action or if we decide to support retroactive NFO gen.
                // Let's stick to "new downloads" for now to avoid side effects on existing files unless explicitly requested.
                // But wait, if the file exists, the loop continues. We should probably flag this item as "successful".
            }
            else
            {
                var directory = Path.GetDirectoryName(item.DestinationPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create directory '{Directory}'.", directory);
                        success = false;
                        continue; // Skip this item
                    }
                }

                switch (item.JobType)
                {
                    case DownloadType.StreamingUrl:
                        _logger.LogInformation("Creating streaming URL file for '{Title}' at '{Path}'.", job.Title, item.DestinationPath);
                        success &= await _fileDownloader.GenerateStreamingUrlFileAsync(item.SourceUrl, item.DestinationPath, cancellationToken).ConfigureAwait(false);
                        break;

                    case DownloadType.DirectDownload:
                        _logger.LogInformation("Downloading '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
                        success &= await _fileDownloader.DownloadFileAsync(item.SourceUrl, item.DestinationPath, progress, cancellationToken).ConfigureAwait(false);
                        break;

                    case DownloadType.QualityUpgrade:
                        _logger.LogInformation("Starting quality upgrade download for '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
                        success &= await DoQualityUpgrade(item, progress, cancellationToken).ConfigureAwait(false);
                        break;
                    case DownloadType.AudioExtraction:
                        var tempVideoPath = GetTempFilePath(".mkv");
                        _logger.LogInformation("Downloading temporary video for '{Title}' to extract '{Language}' audio.", job.Title, job.AudioLanguage);

                        // Track progress for the download part (0-80%)
                        var downloadProgress = new Progress<double>(p => progress.Report(p * 0.8));

                        if (await _fileDownloader.DownloadFileAsync(item.SourceUrl, tempVideoPath, downloadProgress, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogInformation("Extracting '{Language}' audio for '{Title}' to '{Path}'.", job.AudioLanguage, job.Title, item.DestinationPath);
                            progress.Report(85);
                            success &= await _ffmpegService.ExtractAudioAsync(tempVideoPath, item.DestinationPath, job.AudioLanguage ?? "und", cancellationToken).ConfigureAwait(false);

                            if (success)
                            {
                                _logger.LogInformation("Successfully extracted '{Language}' audio for '{Title}'.", job.AudioLanguage, job.Title);
                            }
                            else
                            {
                                _logger.LogError("Failed to extract audio for '{Title}'.", job.Title);
                            }

                            // Clean up
                            try
                            {
                                if (File.Exists(tempVideoPath))
                                {
                                    File.Delete(tempVideoPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete temporary file '{TempPath}'.", tempVideoPath);
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to download temporary video for '{Title}'.", job.Title);
                            success = false;
                        }

                        break;

                    default:
                        _logger.LogError("Unknown download type: {Type}", item.JobType);
                        success = false;
                        break;
                }
            }
        }

        if (success)
        {
            progress.Report(100);

            if (job.NfoMetadata is not null && !File.Exists(job.NfoMetadata.FilePath))
            {
                _nfoService.CreateNfo(job.NfoMetadata);
            }
        }

        return success;
    }

    /// <summary>
    /// Tries to perform a quality upgrade download.
    /// </summary>
    /// <param name="item">The Download item to process.</param>
    /// <param name="progress">Progress Reporting.</param>
    /// <param name="cancellationToken">The cancellation Token.</param>
    /// <returns>Return true if the upgrade was successful, false otherwise.</returns>
    private async Task<bool> DoQualityUpgrade(DownloadItem item, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var tempPath = GetTempFilePath(".mkv");
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

    /// <summary>
    /// Gets a temporary file path with an optional extension.
    /// </summary>
    /// <param name="extension">The Extension for the Temp-path.</param>
    /// <returns>The Temp-path.</returns>
    private string GetTempFilePath(string? extension = null)
    {
        var tempFileName = $"{Guid.NewGuid()}{extension}";
        return Path.Combine(_appPaths.TempDirectory, tempFileName);
    }
}
