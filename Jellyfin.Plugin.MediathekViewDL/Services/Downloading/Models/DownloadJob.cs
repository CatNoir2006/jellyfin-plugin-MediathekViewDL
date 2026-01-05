using System.Collections.Generic;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Represents a unit of work for the download manager.
/// </summary>
public class DownloadJob
{
    private readonly VideoInfo _itemInfo = null!;

    /// <summary>
    /// Gets the unique identifier of the MediathekView result item.
    /// Used for tracking processed items.
    /// </summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the collection of download items associated with this job.
    /// </summary>
    public HashSet<DownloadItem> DownloadItems { get; } = new();

    /// <summary>
    /// Gets the Item Info of the Mainvideo for the Download Job.
    /// </summary>
    public required VideoInfo ItemInfo { get => _itemInfo; init => _itemInfo = value with { }; }

    /// <summary>
    /// Gets or sets the title of the video/content. Used primarily for logging.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the NFO metadata to be created for this item, if applicable.
    /// </summary>
    public NfoDTO? NfoMetadata { get; set; }
}
