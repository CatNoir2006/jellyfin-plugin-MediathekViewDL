using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Interface for the download history repository.
/// </summary>
public interface IDownloadHistoryRepository
{
    /// <summary>
    /// Adds a new entry to the download history.
    /// </summary>
    /// <param name="entry">The history entry to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(DownloadHistoryEntry entry);

    /// <summary>
    /// Checks if a video URL has already been downloaded.
    /// </summary>
    /// <param name="videoUrl">The video URL.</param>
    /// <returns>True if the video exists in history, otherwise false.</returns>
    Task<bool> ExistsAsync(string videoUrl);

    /// <summary>
    /// Checks if a video URL hash has already been downloaded.
    /// </summary>
    /// <param name="videoUrlHash">The hash of the video URL.</param>
    /// <returns>True if the video exists in history, otherwise false.</returns>
    Task<bool> ExistsByHashAsync(string videoUrlHash);

    /// <summary>
    /// Gets a download history entry by the video URL.
    /// </summary>
    /// <param name="videoUrl">The video URL.</param>
    /// <returns>The history entry, or null if not found.</returns>
    Task<DownloadHistoryEntry?> GetByVideoUrlAsync(string videoUrl);

    /// <summary>
    /// Gets all download history entries for a specific subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <returns>A collection of download history entries.</returns>
    Task<IEnumerable<DownloadHistoryEntry>> GetBySubscriptionIdAsync(Guid subscriptionId);
}
