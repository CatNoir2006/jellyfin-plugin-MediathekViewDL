namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Options for maintenance and system behavior.
/// </summary>
public record MaintenanceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to enable the automated cleanup of invalid .strm files.
    /// </summary>
    public bool EnableStrmCleanup { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether downloads should be allowed if the available disk space cannot be determined.
    /// This can happen with network shares or non-standard file systems.
    /// </summary>
    public bool AllowDownloadOnUnknownDiskSpace { get; set; }
}
