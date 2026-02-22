namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Options for Live TV.
/// </summary>
public record LiveTvOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether subtitles should be enabled for Zapp Live TV.
    /// </summary>
    public bool EnableZappSubtitles { get; set; } = false;
}
