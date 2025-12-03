using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

#nullable disable

/// <summary>
/// Represents a single item in the API result set.
/// </summary>
public class ResultItem
{
    /// <summary>
    /// Gets or sets the unique identifier of the result item.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the video.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the topic of the video.
    /// </summary>
    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    /// <summary>
    /// Gets or sets the channel where the video was published.
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    /// <summary>
    /// Gets or sets the description of the video.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the video in Unix epoch format.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the duration of the video in seconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets the size of the video file in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the URL for the standard quality video.
    /// </summary>
    [JsonPropertyName("url_video")]
    public string UrlVideo { get; set; }

    /// <summary>
    /// Gets or sets the URL for the low quality video.
    /// </summary>
    [JsonPropertyName("url_video_low")]
    public string UrlVideoLow { get; set; }

    /// <summary>
    /// Gets or sets the URL for the high quality video.
    /// </summary>
    [JsonPropertyName("url_video_hd")]
    public string UrlVideoHd { get; set; }

    /// <summary>
    /// Gets or sets the URL for the subtitle file.
    /// </summary>
    [JsonPropertyName("url_subtitle")]
    public string UrlSubtitle { get; set; }
}

#nullable enable
