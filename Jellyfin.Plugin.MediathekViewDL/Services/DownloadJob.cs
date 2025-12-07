namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Represents a unit of work for the download manager.
/// </summary>
public class DownloadJob
{
    /// <summary>
    /// Gets or sets the unique identifier of the MediathekView result item.
    /// Used for tracking processed items.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source URL (Video URL, Subtitle URL, etc.).
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full local path where the result should be saved.
    /// </summary>
    public string DestinationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of operation to perform.
    /// </summary>
    public DownloadType JobType { get; set; }

    /// <summary>
    /// Gets or sets the language code for audio extraction (e.g., "deu", "eng").
    /// Required only if <see cref="JobType"/> is <see cref="DownloadType.AudioExtraction"/>.
    /// </summary>
    public string? AudioLanguage { get; set; }

    /// <summary>
    /// Gets or sets the title of the video/content. Used primarily for logging.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
