namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Specifies the type of subtitles.
/// </summary>
public enum SubtitleType
{
    /// <summary>
    /// Unknown subtitle type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// TTML / EBU-TT (Timed Text Markup Language / European Broadcasting Union Timed Text) subtitles.
    /// </summary>
    // ReSharper disable once InconsistentNaming, Subtitle format name
    TTML = 1,

    /// <summary>
    /// WebVTT (Web Video Text Tracks) subtitles.
    /// </summary>
    // ReSharper disable once InconsistentNaming, Subtitle format name
    WEBVTT = 2,
}
