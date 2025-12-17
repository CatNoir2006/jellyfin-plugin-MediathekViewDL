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
    /// <param name="videoUrl">The Video Url.</param>
    /// <param name="itemId">The MediathekView Id.</param>
    /// <param name="subscriptionId">The SubId.</param>
    /// <param name="downloadPath">The Download Path.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(string videoUrl, string itemId, Guid subscriptionId, string downloadPath);

    /// <summary>
    /// Checks if a video URL has already been downloaded.
    /// </summary>
    /// <param name="videoUrl">The video URL.</param>
    /// <returns>True if the video exists in history, otherwise false.</returns>
    Task<bool> ExistsByUrlAsync(string videoUrl);

    /// <summary>
    /// Checks if an item with the given item ID exists in the download history.
    /// </summary>
    /// <param name="itemId">The id of the item in MediathekView.</param>
    /// <returns>True if the video exists in history, otherwise false.</returns>
    Task<bool> ExistsByItemIdAsync(string itemId);

    /// <summary>
    /// Gets whether a download history entry exists for the specified video URL and subscription ID.
    /// </summary>
    /// <param name="videoUrl">The url of the video.</param>
    /// <param name="subscriptionId">The Id of the Sub.</param>
    /// <returns>True if the video exists in history, otherwise false.</returns>
    Task<bool> ExistsByUrlAndSubscriptionIdAsync(string videoUrl, Guid subscriptionId);

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
    /// Gets a download history entry by the item ID and subscription ID.
    /// </summary>
    /// <param name="itemId">The mediathekView item id.</param>
    /// <param name="subscriptionId">The sub Id.</param>
    /// <returns>The history entry, or null if not found.</returns>
    Task<DownloadHistoryEntry?> GetByItemIdAndSubscriptionIdAsync(string itemId, Guid subscriptionId);

    /// <summary>
    /// Gets a download history entry by the item ID.
    /// </summary>
    /// <param name="itemId">The mediathekView item id.</param>
    /// <returns>The history entry, or null if not found.</returns>
    Task<DownloadHistoryEntry?> GetByItemIdAsync(string itemId);

    /// <summary>
    /// Gets a download history entry by the video URL and subscription ID.
    /// </summary>
    /// <param name="videoUrl">The video Url.</param>
    /// <param name="subscriptionId">The sub Id.</param>
    /// <returns>The history entry, or null if not found.</returns>
    Task<DownloadHistoryEntry?> GetByUrlAndSubscriptionIdAsync(string videoUrl, Guid subscriptionId);

    /// <summary>
    /// Gets all download history entries for a specific subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <returns>A collection of download history entries.</returns>
    Task<IEnumerable<DownloadHistoryEntry>> GetBySubscriptionIdAsync(Guid subscriptionId);
}
