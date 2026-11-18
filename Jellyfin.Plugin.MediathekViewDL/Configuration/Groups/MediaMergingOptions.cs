using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Configuration options for merging multi-source media instances and preventing duplicates.
/// </summary>
public class MediaMergingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether multi-source instance merging is enabled.
    /// Default is false.
    /// </summary>
    public bool EnableInstanceMerging { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether .strm stream links should be automatically deleted when a physical local video file exists for the same episode.
    /// Default is false.
    /// </summary>
    public bool AutoDeleteStrmWhenLocalExists { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multi-source versions should be named with standard Jellyfin multi-version tags (e.g. [Mediathek Stream]) to avoid duplicate cards.
    /// Default is true.
    /// </summary>
    public bool GroupAsMultiVersion { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether local Jellyfin storage is prioritized as Priority 1 and streams as Priority 2.
    /// Default is true.
    /// </summary>
    public bool PrioritizeLocalOverStream { get; set; } = true;
}
