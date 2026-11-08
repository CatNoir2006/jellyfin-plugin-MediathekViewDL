using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Mode of syncing missing episodes: direct download or streaming link (.strm).
/// </summary>
public enum MissingEpisodeSyncMode
{
    /// <summary>
    /// Download the actual video file via the download manager.
    /// </summary>
    Download = 0,

    /// <summary>
    /// Create a lightweight .strm stream link pointing to Mediathek CDN.
    /// </summary>
    StreamLink = 1
}

/// <summary>
/// Configuration options for automatically syncing missing episodes.
/// </summary>
public class MissingEpisodesOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic missing episodes sync is enabled.
    /// Default is false.
    /// </summary>
    public bool EnableMissingEpisodesSync { get; set; }

    /// <summary>
    /// Gets or sets the sync mode (Download or StreamLink).
    /// Default is Download.
    /// </summary>
    public MissingEpisodeSyncMode SyncMode { get; set; } = MissingEpisodeSyncMode.Download;

    /// <summary>
    /// Gets or sets the sync interval in hours.
    /// Default is 24 hours.
    /// </summary>
    public int SyncIntervalHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets an optional comma-separated list of series titles to monitor.
    /// If empty, all local series are eligible for missing episode detection.
    /// </summary>
    public string SeriesWhitelist { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether dual-audio episodes (German + Original English) should be preferred.
    /// Default is false.
    /// </summary>
    public bool PreferDualAudio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to trigger Jellyfin library scan after creating .strm or downloads.
    /// Default is true.
    /// </summary>
    public bool AutoTriggerLibraryScan { get; set; } = true;
}
