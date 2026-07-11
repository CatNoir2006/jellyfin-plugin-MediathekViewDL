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
/// Handler for downloading video files via FFmpeg.
/// Supports both regular HTTP URLs and M3U8/HLS streams.
/// </summary>
public class FFmpegDownloadHandler : IDownloadHandler
{
    private readonly ILogger<FFmpegDownloadHandler> _logger;
    private readonly IFFmpegService _ffmpegService;
    private readonly IConfigurationProvider _configProvider;
    private readonly IServerApplicationPaths _appPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegDownloadHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ffmpegService">The ffmpeg service.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="appPaths">The application paths.</param>
    public FFmpegDownloadHandler(
        ILogger<FFmpegDownloadHandler> logger,
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
        return downloadType == DownloadType.FFmpegDownload;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading video for '{Title}' from '{Url}' to '{Path}'.", job.Title, item.SourceUrl, item.DestinationPath);
        var tempPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mkv", _configProvider, _appPaths, _logger);
        try
        {
            var readRate = _configProvider.Configuration.Download.ReadRate;
            var res = await _ffmpegService.DownloadFileAsync(item.SourceUrl, tempPath, readRate, progress, cancellationToken).ConfigureAwait(false);
            if (!res)
            {
                return false;
            }

            File.Move(tempPath, item.DestinationPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download or move video file for {DestinationPath}", item.DestinationPath);
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
                    _logger.LogWarning(ex, "Failed to delete temporary video file {TempPath}", tempPath);
                }
            }
        }
    }
}
