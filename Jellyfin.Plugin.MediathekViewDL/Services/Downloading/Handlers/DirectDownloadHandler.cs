using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Handler for direct file downloads.
/// </summary>
public class DirectDownloadHandler : IDownloadHandler
{
    private readonly ILogger<DirectDownloadHandler> _logger;
    private readonly IFileDownloader _fileDownloader;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectDownloadHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    public DirectDownloadHandler(ILogger<DirectDownloadHandler> logger, IFileDownloader fileDownloader)
    {
        _logger = logger;
        _fileDownloader = fileDownloader;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.DirectDownload;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
        return await _fileDownloader.DownloadFileAsync(item.SourceUrl, item.DestinationPath, progress, cancellationToken).ConfigureAwait(false);
    }
}
