using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Service to merge media instances from multiple sources (Jellyfin local disk, Seerr downloads, Mediathek streams).
/// Prioritizes local physical media (Prio 1) over stream links (Prio 2) to eliminate duplicates.
/// </summary>
public interface IMediaMergeService
{
    /// <summary>
    /// Scans libraries and cleans up or consolidates duplicate .strm links where physical video files already exist.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>The number of items merged or cleaned up.</returns>
    Task<int> MergeMediaInstancesAsync(CancellationToken cancellationToken, IProgress<double>? progress = null);

    /// <summary>
    /// Processes merging logic for a specific season or series directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of duplicate or stale stream items merged or removed.</returns>
    Task<int> ProcessDirectoryMergingAsync(string directoryPath, CancellationToken cancellationToken);
}
