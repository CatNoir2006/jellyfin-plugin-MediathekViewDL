using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Handler for downloading subtitle files via HTTP.
/// No bandwidth limiting is applied since subtitles are small files.
/// </summary>
public class SubtitleDownloadHandler : IDownloadHandler
{
    private readonly ILogger<SubtitleDownloadHandler> _logger;
    private readonly IFileDownloader _fileDownloader;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubtitleDownloadHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileDownloader">The file downloader service.</param>
    public SubtitleDownloadHandler(ILogger<SubtitleDownloadHandler> logger, IFileDownloader fileDownloader)
    {
        _logger = logger;
        _fileDownloader = fileDownloader;
    }

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return downloadType == DownloadType.SubtitleDownload;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading subtitle for '{Title}' to '{Path}'.", job.Title, item.DestinationPath);
        try
        {
            var directory = Path.GetDirectoryName(item.DestinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return await _fileDownloader.DownloadFileAsync(item.SourceUrl, item.DestinationPath, progress, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download subtitle for {DestinationPath}", item.DestinationPath);
            return false;
        }
    }
}
