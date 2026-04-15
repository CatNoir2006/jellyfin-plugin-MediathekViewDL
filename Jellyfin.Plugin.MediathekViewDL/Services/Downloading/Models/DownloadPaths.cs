using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Holds the generated paths for a download item.
/// </summary>
public record DownloadPaths
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
    /// Gets or sets the full path for the NFO metadata file.
    /// </summary>
    public string NfoFilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the File Type of the Main File Path.
    /// </summary>
    public FileType MainType { get; set; } = FileType.Video;

    /// <summary>
    /// Gets a value indicating whether the paths are valid.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(DirectoryPath) && !string.IsNullOrWhiteSpace(MainFilePath);
}
