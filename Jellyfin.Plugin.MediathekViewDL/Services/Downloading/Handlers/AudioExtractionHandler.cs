using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Helpers;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Handler for audio extraction.
/// </summary>
public class AudioExtractionHandler : IDownloadHandler
{
    private readonly ILogger<AudioExtractionHandler> _logger;
    private readonly IFFmpegService _ffmpegService;
    private readonly IConfigurationProvider _configProvider;
    private readonly IServerApplicationPaths _appPaths;
    private readonly IFileDownloader _fileDownloader;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioExtractionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ffmpegService">The ffmpeg service.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="appPaths">The application paths.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    public AudioExtractionHandler(
        ILogger<AudioExtractionHandler> logger,
        IFFmpegService ffmpegService,
        IConfigurationProvider configProvider,
        IServerApplicationPaths appPaths,
        IFileDownloader fileDownloader)
    {
        _logger = logger;
        _ffmpegService = ffmpegService;
        _configProvider = configProvider;
        _appPaths = appPaths;
        _fileDownloader = fileDownloader;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.AudioExtraction;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
        var config = _configProvider.Configuration;

        if (config.EnableDirectAudioExtraction)
        {
            return await DoAudioExtractNew(item, job.ItemInfo, progress, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return await DoAudioExtractOld(item, job.ItemInfo.Language, progress, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> DoAudioExtractNew(DownloadItem item, VideoInfo itemInfo, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var tempPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mka", _configProvider, _appPaths, _logger);
        var res = await _ffmpegService.ExtractAudioFromWebAsync(item.SourceUrl, tempPath, itemInfo.Language, itemInfo.Language != "deu", itemInfo.HasAudiodescription, progress, cancellationToken).ConfigureAwait(false);
        if (res)
        {
            try
            {
                File.Move(tempPath, item.DestinationPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move temporary audio file from {TempPath} to {DestinationPath}", tempPath, item.DestinationPath);
                return false;
            }
        }

        if (File.Exists(tempPath))
        {
            try
            {
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary audio file {TempPath}", tempPath);
            }
        }

        return false;
    }

    private async Task<bool> DoAudioExtractOld(DownloadItem item, string language, IProgress<double> progress, CancellationToken cancellationToken)
    {
        bool success = true;
        var tempVideoPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mkv", _configProvider, _appPaths, _logger);

        // Track progress for the download part (0-80%)
        var downloadProgress = new Progress<double>(p => progress.Report(p * 0.8));

        if (await _fileDownloader.DownloadFileAsync(item.SourceUrl, tempVideoPath, downloadProgress, cancellationToken).ConfigureAwait(false))
        {
            progress.Report(85);
            // Track progress for the extraction part (85-100%)
            var extractionProgress = new Progress<double>(p => progress.Report(85 + (p * 0.15)));

            success &= await _ffmpegService.ExtractAudioAsync(tempVideoPath, item.DestinationPath, language, extractionProgress, cancellationToken).ConfigureAwait(false);

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
                success = false;
            }
        }
        else
        {
            _logger.LogError("Failed to download temporary video for '{Title}'.", item.DestinationPath);
            success = false;
        }

        return success;
    }
}
