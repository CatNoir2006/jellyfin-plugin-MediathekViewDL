using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

/// <summary>
/// Represents a show in the Zapp API.
/// </summary>
public record ZappShow
{
    /// <summary>
    /// Gets or sets the title of the show.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the subtitle of the show.
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the description of the show.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the start time of the show.
    /// </summary>
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the show.
    /// </summary>
    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }
}
