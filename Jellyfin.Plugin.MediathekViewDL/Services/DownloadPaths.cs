using System.IO;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Holds the generated paths for a download item.
/// </summary>
public class DownloadPaths
{
    /// <summary>
    /// Gets or sets the target directory path.
    /// </summary>
    public string DirectoryPath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the full path for the main video or audio file.
    /// </summary>
    public string MainFilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the full path for the subtitle file.
    /// </summary>
    public string SubtitleFilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the full path for the streaming (.strm) file.
    /// </summary>
    public string StrmFilePath { get; set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the paths are valid.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(DirectoryPath) && (!string.IsNullOrWhiteSpace(MainFilePath) || !string.IsNullOrWhiteSpace(StrmFilePath));
}
