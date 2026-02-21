using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Handler for creating streaming URL files (.strm).
/// </summary>
public class StreamingUrlHandler : IDownloadHandler
{
    private readonly ILogger<StreamingUrlHandler> _logger;
    private readonly IFileDownloader _fileDownloader;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamingUrlHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    public StreamingUrlHandler(ILogger<StreamingUrlHandler> logger, IFileDownloader fileDownloader)
    {
        _logger = logger;
        _fileDownloader = fileDownloader;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.StreamingUrl;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating streaming URL file for '{Title}' at '{Path}'.", job.Title, item.DestinationPath);
        return await _fileDownloader.GenerateStreamingUrlFileAsync(item.SourceUrl, item.DestinationPath, cancellationToken).ConfigureAwait(false);
    }
}
