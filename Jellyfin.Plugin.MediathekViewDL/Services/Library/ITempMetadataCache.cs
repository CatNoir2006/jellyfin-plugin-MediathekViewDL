using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Provides an in-memory cache for media metadata to avoid redundant FFmpeg probing during a session.
/// </summary>
public interface ITempMetadataCache
{
    /// <summary>
    /// Gets the metadata for a media file or URL, using the cache if available and valid.
    /// </summary>
    /// <param name="urlOrPath">The path to the local file or a remote URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media info, or null if it could not be determined.</returns>
    Task<LocalMediaInfo?> GetMetadataAsync(string urlOrPath, CancellationToken cancellationToken);
}
