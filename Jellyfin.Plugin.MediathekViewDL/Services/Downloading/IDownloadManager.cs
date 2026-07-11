using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Interface for the DownloadManager service.
/// </summary>
public interface IDownloadManager
{
    /// <summary>
    /// Executes a single download job.
    /// </summary>
    /// <param name="job">The job to execute.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the download job with per-item details.</returns>
    Task<DownloadJobResult> ExecuteJobAsync(DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken);
}
