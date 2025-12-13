namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

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
    /// Upgrade the quality of an existing file.
    /// </summary>
    QualityUpgrade
}
