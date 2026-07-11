namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Defines the type of download job.
/// </summary>
public enum DownloadType
{
    /// <summary>
    /// Video download via FFmpeg (supports HTTP URLs and M3U8/HLS streams).
    /// </summary>
    FFmpegDownload,

    /// <summary>
    /// Subtitle download via HTTP (no bandwidth limiting).
    /// </summary>
    SubtitleDownload,

    /// <summary>
    /// Extract audio from a video URL via FFmpeg.
    /// </summary>
    AudioExtraction,

    /// <summary>
    /// Create a streaming URL file (.strm).
    /// </summary>
    StreamingUrl,
}
