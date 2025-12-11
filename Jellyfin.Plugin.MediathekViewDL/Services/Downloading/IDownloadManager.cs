using System;
using System.Threading;
using System.Threading.Tasks;

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
    /// <returns>True if the job was successful (or file already existed), otherwise false.</returns>
    Task<bool> ExecuteJobAsync(DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken);
}
