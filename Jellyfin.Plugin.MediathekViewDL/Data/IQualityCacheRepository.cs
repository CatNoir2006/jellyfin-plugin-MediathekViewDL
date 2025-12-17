using System;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Interface for the quality cache repository.
/// </summary>
public interface IQualityCacheRepository
{
    /// <summary>
    /// Gets a cache entry by the video URL.
    /// </summary>
    /// <param name="url">The video URL.</param>
    /// <returns>The cache entry, or null if not found.</returns>
    Task<QualityCacheEntry?> GetByUrlAsync(string url);

    /// <summary>
    /// Adds or updates a cache entry.
    /// </summary>
    /// <param name="url">The video URL.</param>
    /// <param name="width">The width of the video.</param>
    /// <param name="height">The height of the video.</param>
    /// <param name="duration">The Duration of the video.</param>
    /// <param name="size">The file size.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddOrUpdateAsync(string url, int width, int height, TimeSpan duration, long size);

    /// <summary>
    /// Removes a cache entry by the video URL.
    /// </summary>
    /// <param name="url">The video URL.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveByUrlAsync(string url);

    /// <summary>
    /// Throws away all cache entries older than the specified age.
    /// </summary>
    /// <param name="maxAge">The maxAge to keep the entries.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveByAgeAsync(TimeSpan maxAge);
}
