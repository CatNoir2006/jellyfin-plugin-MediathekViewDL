using System;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

/// <summary>
/// Data Transfer Object for NFO generation.
/// </summary>
public class NfoDTO
{
    /// <summary>
    /// Gets or sets The full file path where the NFO will be saved.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets The title of the video or episode.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets The description or plot summary of the video.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets The name of the show for episodes.
    /// </summary>
    public string Show { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets The season number for episodes.
    /// </summary>
    public int? Season { get; set; }

    /// <summary>
    /// Gets or sets The episode number for episodes.
    /// </summary>
    public int? Episode { get; set; }

    /// <summary>
    /// Gets a value indicating whether this NFO represents an episode.
    /// </summary>
    public bool IsEpisode => Season.HasValue; // Extras with Season number are also episodes

    /// <summary>
    /// Gets or sets The studio or production company. In our case, the broadcaster. (e.g., ARD, ZDF).
    /// </summary>
    public string Studio { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets The unique identifier for the video from MediathekView.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets the source of the unique identifier. In this case, it's always "MediathekView".
    /// </summary>
    public string IdSource => "MediathekView";

    /// <summary>
    /// Gets or sets The air date of the video or episode.
    /// </summary>
    public DateTime? AirDate { get; set; }

    /// <summary>
    /// Gets or sets The runtime/duration of the video.
    /// </summary>
    public TimeSpan? RunTime { get; set; }

    /// <summary>
    /// Gets or sets The set name for grouping related videos.
    /// Only used for movies. Currently unused.
    /// </summary>
    public string Set { get; set; } = string.Empty;
}
