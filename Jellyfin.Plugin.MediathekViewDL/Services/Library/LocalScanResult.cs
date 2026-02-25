using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Represents the result of a local directory scan.
/// </summary>
public class LocalScanResult
{
    /// <summary>
    /// Gets the list of all scanned files.
    /// </summary>
    public Collection<ScannedFile> Files { get; init; } = new();

    /// <summary>
    /// Gets the episode cache for fast lookup.
    /// </summary>
    public LocalEpisodeCache EpisodeCache { get; init; } = new();
}
