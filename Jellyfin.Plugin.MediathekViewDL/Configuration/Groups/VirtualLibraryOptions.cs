using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Configuration options for the dedicated virtual streaming library (.strm &amp; .nfo).
/// </summary>
public class VirtualLibraryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether dedicated virtual library streaming is enabled.
    /// Default is false.
    /// </summary>
    public bool EnableVirtualLibrary { get; set; }

    /// <summary>
    /// Gets or sets the root destination directory for the virtual streaming library (e.g. /data/mediathek).
    /// </summary>
    public string VirtualLibraryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the synchronization interval in hours.
    /// Default is 24 hours.
    /// </summary>
    public int SyncIntervalHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets a value indicating whether all configured subscriptions should be automatically populated as virtual stream links.
    /// Default is true.
    /// </summary>
    public bool IncludeSubscriptions { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional comma-separated list of additional Mediathek topics or series to sync completely.
    /// E.g. "The Rookie, Babylon Berlin, Tatort".
    /// </summary>
    public string CustomTopics { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets minimum duration in minutes to exclude short clips or previews.
    /// Default is 10 minutes.
    /// </summary>
    public int MinDurationMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to trigger a Jellyfin library scan when new items are synced.
    /// Default is true.
    /// </summary>
    public bool AutoTriggerLibraryScan { get; set; } = true;
}
