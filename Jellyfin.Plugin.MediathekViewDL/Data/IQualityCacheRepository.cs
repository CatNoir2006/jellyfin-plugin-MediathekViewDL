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
    /// <param name="size">The file size.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddOrUpdateAsync(string url, int width, int height, long size);

    /// <summary>
    /// Removes a cache entry by the video URL.
    /// </summary>
    /// <param name="url">The video URL.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveByUrlAsync(string url);
}
