namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Defines the type of download job.
/// </summary>
public enum DownloadType
{
    /// <summary>
    /// Direct file download (e.g. video, subtitle).
    /// </summary>
    DirectDownload,

    /// <summary>
    /// Extract audio from video.
    /// </summary>
    AudioExtraction,

    /// <summary>
    /// Create a streaming URL file (.strm).
    /// </summary>
    StreamingUrl,

    /// <summary>
    /// Download an M3U8 stream.
    /// </summary>
    M3U8Download,

    /// <summary>
    /// Upgrade the quality of an existing file.
    /// </summary>
    QualityUpgrade
}
