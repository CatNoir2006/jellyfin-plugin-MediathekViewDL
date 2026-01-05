namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Represents the status of a download job.
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// The job is queued and waiting to start.
    /// </summary>
    Queued,

    /// <summary>
    /// The job is currently downloading files.
    /// </summary>
    Downloading,

    /// <summary>
    /// The job is processing files (e.g., ffmpeg muxing/extracting).
    /// </summary>
    Processing,

    /// <summary>
    /// The job finished successfully.
    /// </summary>
    Finished,

    /// <summary>
    /// The job failed with an error.
    /// </summary>
    Failed,

    /// <summary>
    /// The job was cancelled by the user.
    /// </summary>
    Cancelled
}
