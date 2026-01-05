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
/// Handler for downloading M3U8 streams.
/// </summary>
public class M3U8DownloadHandler : IDownloadHandler
{
    private readonly ILogger<M3U8DownloadHandler> _logger;
    private readonly IFFmpegService _ffmpegService;
    private readonly IConfigurationProvider _configProvider;
    private readonly IServerApplicationPaths _appPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="M3U8DownloadHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="appPaths">The application paths.</param>
    /// <param name="ffmpegService">The ffmpeg service.</param>
    public M3U8DownloadHandler(
        ILogger<M3U8DownloadHandler> logger,
        IConfigurationProvider configProvider,
        IServerApplicationPaths appPaths,
        IFFmpegService ffmpegService)
    {
        _logger = logger;
        _configProvider = configProvider;
        _appPaths = appPaths;
        _ffmpegService = ffmpegService;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.M3U8Download;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading M3U8 stream for '{Title}' from '{Url}' to '{Path}'.", job.Title, item.SourceUrl, item.DestinationPath);
        var tempPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mkv", _configProvider, _appPaths, _logger);
        var res = await _ffmpegService.DownloadM3U8Async(item.SourceUrl, tempPath, progress, cancellationToken).ConfigureAwait(false);
        if (res)
        {
            try
            {
                File.Move(tempPath, item.DestinationPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move temporary video file from {TempPath} to {DestinationPath}", tempPath, item.DestinationPath);
                return false;
            }
        }

        if (!File.Exists(tempPath))
        {
            return false;
        }

        try
        {
            File.Delete(tempPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temporary audio file {TempPath}", tempPath);
        }

        return false;
    }
}
