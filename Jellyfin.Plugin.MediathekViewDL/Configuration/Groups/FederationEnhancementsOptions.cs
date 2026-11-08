using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Additional federation and ecosystem enhancement options (inspired by Jellyfin.Plugin.Emby).
/// </summary>
public class FederationEnhancementsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether dynamic MediaSource pre-population is enabled for instant direct play of .strm links without FFmpeg probing delays.
    /// Default is false.
    /// </summary>
    public bool EnableFastMediaSourceResolution { get; set; }

    /// <summary>
    /// Gets or sets the optional Seerr / Jellyseerr base URL (e.g. http://seerr:5055).
    /// </summary>
    public string SeerrUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional Seerr / Jellyseerr API key.
    /// </summary>
    public string SeerrApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically notify Seerr / Jellyseerr to run recently-added scan after new downloads or stream links are synced.
    /// Default is false.
    /// </summary>
    public bool AutoTriggerSeerrScan { get; set; }
}
