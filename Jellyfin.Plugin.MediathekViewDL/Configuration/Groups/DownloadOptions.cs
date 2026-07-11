namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Options for the download process.
/// </summary>
public record DownloadOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether subtitles should be downloaded if available.
    /// </summary>
    public bool DownloadSubtitles { get; set; } = true;

    /// <summary>
    /// Gets or sets the FFmpeg readrate multiplier for download speed limiting.
    /// 0 means unlimited (no -readrate argument is passed to FFmpeg).
    /// Values above 0 slow down the download (e.g. 0.5 = half speed).
    /// </summary>
    public int ReadRate { get; set; } = 0;

    /// <summary>
    /// Gets or sets the minimum free disk space in bytes required to start a new download.
    /// </summary>
    public long MinFreeDiskSpaceBytes { get; set; } = (long)(1.5 * 1024 * 1024 * 1024); // Default to 1.5 GiB

    /// <summary>
    /// Gets or sets a value indicating whether a library scan should be triggered after a download finishes.
    /// </summary>
    public bool ScanLibraryAfterDownload { get; set; } = true;
}
