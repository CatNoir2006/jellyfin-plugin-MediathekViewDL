namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Represents a single download item.
/// </summary>
public class DownloadItem
{
    /// <summary>
    /// Gets or sets the source URL (Video URL, Subtitle URL, etc.).
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full local path where the result should be saved.
    /// </summary>
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of an existing file that this download is intended to replace (e.g., during a quality upgrade).
    /// </summary>
    public string? ReplaceFilePath { get; set; }

    /// <summary>
    /// Gets or sets the type of operation to perform.
    /// </summary>
    public DownloadType JobType { get; set; }
}
