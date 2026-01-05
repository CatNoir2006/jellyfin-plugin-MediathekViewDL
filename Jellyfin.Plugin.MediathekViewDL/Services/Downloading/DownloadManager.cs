using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Service responsible for executing download jobs.
/// </summary>
public class DownloadManager : IDownloadManager
{
    private readonly ILogger<DownloadManager> _logger;
    private readonly INfoService _nfoService;
    private readonly IEnumerable<IDownloadHandler> _downloadHandlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadManager"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="nfoService">The NFO service.</param>
    /// <param name="downloadHandlers">The download handlers.</param>
    public DownloadManager(
        ILogger<DownloadManager> logger,
        INfoService nfoService,
        IEnumerable<IDownloadHandler> downloadHandlers)
    {
        _logger = logger;
        _nfoService = nfoService;
        _downloadHandlers = downloadHandlers;
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
                // Still continue execution so NFO and other files continue downloading.
                continue;
            }

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

            var handler = _downloadHandlers.FirstOrDefault(h => h.CanHandle(item.JobType));
            if (handler != null)
            {
                success &= await handler.ExecuteAsync(item, job, progress, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogError("No handler found for download type: {Type}", item.JobType);
                success = false;
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
}
