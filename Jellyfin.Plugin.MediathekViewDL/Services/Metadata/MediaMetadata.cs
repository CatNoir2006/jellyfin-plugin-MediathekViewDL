using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

/// <summary>
/// Represents metadata that is embedded into downloaded media files (.mkv, .strm).
/// The JSON serialization of this object is stored under the key <c>MediathekViewDL</c>
/// inside the matroska container (via ffmpeg) or as a comment in .strm files.
/// </summary>
public class MediaMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier of the MediathekView result item.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the download URL that was actually used for the download.
    /// </summary>
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of all video URLs that were returned by the API for this item.
    /// Includes the selected download URL as well as any fallback qualities.
    /// </summary>
    [JsonPropertyName("videoUrls")]
    public IReadOnlyList<string> VideoUrls { get; set; } = [];

    /// <summary>
    /// Gets or sets the URL of the preferred subtitle, or <c>null</c> if no subtitle was downloaded.
    /// </summary>
    [JsonPropertyName("subtitleUrl")]
    public string? SubtitleUrl { get; set; }

    /// <summary>
    /// Gets or sets the original (unmodified) title of the item as returned by MediathekView.
    /// </summary>
    [JsonPropertyName("originalTitle")]
    public string OriginalTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original (unmodified) topic of the item as returned by the MediathekView API.
    /// This is the show / series name and is preserved even when the subscription uses a custom
    /// name for the series or the file name builder has rewritten the local topic.
    /// </summary>
    [JsonPropertyName("originalTopic")]
    public string OriginalTopic { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the season number extracted from the title, if the item is part of a series.
    /// </summary>
    [JsonPropertyName("seasonNumber")]
    public int? SeasonNumber { get; set; }

    /// <summary>
    /// Gets or sets the episode number extracted from the title, if the item is part of a series.
    /// </summary>
    [JsonPropertyName("episodeNumber")]
    public int? EpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the absolute episode number extracted from the title, if the series uses
    /// absolute (continuous) numbering instead of per-season numbering.
    /// </summary>
    [JsonPropertyName("absoluteEpisodeNumber")]
    public int? AbsoluteEpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the description / plot of the item.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
