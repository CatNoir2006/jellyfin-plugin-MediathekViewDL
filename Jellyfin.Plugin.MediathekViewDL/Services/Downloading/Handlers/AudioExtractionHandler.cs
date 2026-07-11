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
/// Handler for extracting audio directly from a video URL via FFmpeg.
/// </summary>
public class AudioExtractionHandler : IDownloadHandler
{
    private readonly ILogger<AudioExtractionHandler> _logger;
    private readonly IFFmpegService _ffmpegService;
    private readonly IConfigurationProvider _configProvider;
    private readonly IServerApplicationPaths _appPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioExtractionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ffmpegService">The ffmpeg service.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="appPaths">The application paths.</param>
    public AudioExtractionHandler(
        ILogger<AudioExtractionHandler> logger,
        IFFmpegService ffmpegService,
        IConfigurationProvider configProvider,
        IServerApplicationPaths appPaths)
    {
        _logger = logger;
        _ffmpegService = ffmpegService;
        _configProvider = configProvider;
        _appPaths = appPaths;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.AudioExtraction;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Extracting audio for '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
        var tempPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mka", _configProvider, _appPaths, _logger);
        try
        {
            var itemInfo = job.ItemInfo;
            var res = await _ffmpegService.ExtractAudioFromWebAsync(
                item.SourceUrl,
                tempPath,
                itemInfo.Language,
                itemInfo.Language != "deu",
                itemInfo.HasAudiodescription,
                progress,
                cancellationToken).ConfigureAwait(false);

            if (!res)
            {
                return false;
            }

            File.Move(tempPath, item.DestinationPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract or move audio file for {DestinationPath}", item.DestinationPath);
            return false;
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
                    _logger.LogWarning(ex, "Failed to delete temporary audio file {TempPath}", tempPath);
                }
            }
        }
    }
}
