namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// The File Type of the Download Main Download Path.
/// </summary>
public enum FileType
{
    /// <summary>
    /// It's a Video File.
    /// </summary>
    Video,

    /// <summary>
    /// It's a Audio File.
    /// </summary>
    Audio,

    /// <summary>
    /// It's a Strm File.
    /// </summary>
    Strm,

    /// <summary>
    /// It's a Subtitle File.
    /// </summary>
    Subtitle,

    /// <summary>
    /// It's an Info/Metadata File (e.g., .txt or .nfo).
    /// </summary>
    Info
}
