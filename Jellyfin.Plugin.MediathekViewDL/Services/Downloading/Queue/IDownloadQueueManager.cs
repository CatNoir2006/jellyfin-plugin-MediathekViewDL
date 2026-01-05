using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;

/// <summary>
/// Interface for the download queue manager.
/// </summary>
public interface IDownloadQueueManager
{
    /// <summary>
    /// Queues a download job.
    /// </summary>
    /// <param name="job">The job to queue.</param>
    /// <param name="subscriptionId">The optional subscription ID if triggered by an automation.</param>
    void QueueJob(DownloadJob job, Guid? subscriptionId = null);

    /// <summary>
    /// Cancels a specific download job.
    /// </summary>
    /// <param name="id">The active download ID.</param>
    void CancelJob(Guid id);

    /// <summary>
    /// Gets all active downloads (queued, running, processing, failed).
    /// </summary>
    /// <returns>A list of active downloads.</returns>
    IEnumerable<ActiveDownload> GetActiveDownloads();
}
