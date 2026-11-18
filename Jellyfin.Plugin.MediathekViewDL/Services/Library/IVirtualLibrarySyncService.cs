using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Service to provision and synchronize ARD / ZDF Mediathek content as lightweight .strm and .nfo pointers.
/// </summary>
public interface IVirtualLibrarySyncService
{
    /// <summary>
    /// Synchronizes configured topics and subscriptions into the virtual library directory.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>The number of items written or updated.</returns>
    Task<int> SyncVirtualLibraryAsync(CancellationToken cancellationToken, IProgress<double>? progress = null);

    /// <summary>
    /// Synchronizes all available episodes for a specific topic into the target directory.
    /// </summary>
    /// <param name="topic">The topic / series name.</param>
    /// <param name="rootPath">The root virtual library folder.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of items written or updated.</returns>
    Task<int> SyncTopicAsync(string topic, string rootPath, CancellationToken cancellationToken);
}
