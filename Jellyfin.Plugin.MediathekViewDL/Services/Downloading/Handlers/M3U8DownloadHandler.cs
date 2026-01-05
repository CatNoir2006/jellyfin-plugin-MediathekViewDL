using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Handler for downloading M3U8 streams.
/// </summary>
public class M3U8DownloadHandler : IDownloadHandler
{
    private readonly ILogger<M3U8DownloadHandler> _logger;
    private readonly IFFmpegService _ffmpegService;

    /// <summary>
    /// Initializes a new instance of the <see cref="M3U8DownloadHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ffmpegService">The ffmpeg service.</param>
    public M3U8DownloadHandler(ILogger<M3U8DownloadHandler> logger, IFFmpegService ffmpegService)
    {
        _logger = logger;
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
        return await _ffmpegService.DownloadM3U8Async(item.SourceUrl, item.DestinationPath, cancellationToken).ConfigureAwait(false);
    }
}
