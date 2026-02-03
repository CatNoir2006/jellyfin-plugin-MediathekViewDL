namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

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
    /// Gets or sets a value indicating whether direct audio extraction from URL is enabled.
    /// </summary>
    public bool EnableDirectAudioExtraction { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum download bandwidth in MBit/s.
    /// 0 means unlimited.
    /// </summary>
    public int MaxBandwidthMBits { get; set; } = 0;

    /// <summary>
    /// Gets or sets the minimum free disk space in bytes required to start a new download.
    /// </summary>
    public long MinFreeDiskSpaceBytes { get; set; } = (long)(1.5 * 1024 * 1024 * 1024); // Default to 1.5 GiB

    /// <summary>
    /// Gets or sets a value indicating whether a library scan should be triggered after a download finishes.
    /// </summary>
    public bool ScanLibraryAfterDownload { get; set; } = true;
}
