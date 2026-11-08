using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Service to identify and fill missing episodes in existing series libraries using ARD/ZDF Mediathek.
/// </summary>
public interface IMissingEpisodeService
{
    /// <summary>
    /// Scans libraries, detects missing episodes for existing series, and synchronizes them via download or stream link.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>The number of missing episodes synced or enqueued.</returns>
    Task<int> SyncMissingEpisodesAsync(CancellationToken cancellationToken, IProgress<double>? progress = null);

    /// <summary>
    /// Checks and synchronizes missing episodes for a single specific series.
    /// </summary>
    /// <param name="seriesName">The series name.</param>
    /// <param name="seriesPath">The series directory path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of missing episodes synced or enqueued.</returns>
    Task<int> SyncSeriesMissingEpisodesAsync(string seriesName, string seriesPath, CancellationToken cancellationToken);
}
