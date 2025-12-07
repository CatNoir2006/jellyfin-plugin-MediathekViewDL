using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service responsible for executing download jobs.
/// </summary>
public class DownloadManager
{
    private readonly ILogger<DownloadManager> _logger;
    private readonly FileDownloader _fileDownloader;
    private readonly FFmpegService _ffmpegService;
    private readonly IServerApplicationPaths _appPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    /// <param name="ffmpegService">The FFmpeg service.</param>
    /// <param name="appPaths">The application paths.</param>
    public DownloadManager(
        ILogger<DownloadManager> logger,
        FileDownloader fileDownloader,
        FFmpegService ffmpegService,
        IServerApplicationPaths appPaths)
    {
        _logger = logger;
        _fileDownloader = fileDownloader;
        _ffmpegService = ffmpegService;
        _appPaths = appPaths;
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
        if (File.Exists(job.DestinationPath))
        {
            _logger.LogDebug("File '{Path}' already exists. Skipping download.", job.DestinationPath);
            progress.Report(100);
            return true;
        }

        var directory = Path.GetDirectoryName(job.DestinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create directory '{Directory}'.", directory);
                return false;
            }
        }

        bool success = false;

        switch (job.JobType)
        {
            case DownloadType.StreamingUrl:
                _logger.LogInformation("Creating streaming URL file for '{Title}' at '{Path}'.", job.Title, job.DestinationPath);
                success = await _fileDownloader.GenerateStreamingUrlFileAsync(job.SourceUrl, job.DestinationPath, cancellationToken).ConfigureAwait(false);
                break;

            case DownloadType.DirectDownload:
                _logger.LogInformation("Downloading '{Title}' to '{Path}'.", job.Title, job.DestinationPath);
                success = await _fileDownloader.DownloadFileAsync(job.SourceUrl, job.DestinationPath, progress, cancellationToken).ConfigureAwait(false);
                break;

            case DownloadType.AudioExtraction:
                var tempVideoPath = Path.Combine(_appPaths.TempDirectory, $"{Guid.NewGuid()}.mp4");
                _logger.LogInformation("Downloading temporary video for '{Title}' to extract '{Language}' audio.", job.Title, job.AudioLanguage);

                // Track progress for the download part (0-80%)
                var downloadProgress = new Progress<double>(p => progress.Report(p * 0.8));

                if (await _fileDownloader.DownloadFileAsync(job.SourceUrl, tempVideoPath, downloadProgress, cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogInformation("Extracting '{Language}' audio for '{Title}' to '{Path}'.", job.AudioLanguage, job.Title, job.DestinationPath);
                    progress.Report(85);
                    success = await _ffmpegService.ExtractAudioAsync(tempVideoPath, job.DestinationPath, job.AudioLanguage ?? "und", cancellationToken).ConfigureAwait(false);

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
                _logger.LogError("Unknown download type: {Type}", job.JobType);
                success = false;
                break;
        }

        if (success)
        {
            progress.Report(100);
        }

        return success;
    }
}
