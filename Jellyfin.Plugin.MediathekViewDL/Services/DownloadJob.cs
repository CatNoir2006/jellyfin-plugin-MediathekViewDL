using System.Collections.Generic;

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
    /// Gets the collection of download items associated with this job.
    /// </summary>
    public HashSet<DownloadItem> DownloadItems { get; } = new();

    /// <summary>
    /// Gets or sets the language code for audio extraction (e.g., "deu", "eng").
    /// Requierd only if DownloadItems contains an audio extraction job.
    /// </summary>
    public string? AudioLanguage { get; set; }

    /// <summary>
    /// Gets or sets the title of the video/content. Used primarily for logging.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
