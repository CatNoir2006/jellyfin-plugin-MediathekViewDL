using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

/// <summary>
/// Represents a channel in the Zapp API.
/// </summary>
public record ZappChannelInfo
{
    /// <summary>
    /// Gets or sets the name of the channel.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the stream URL.
    /// </summary>
    [JsonPropertyName("streamUrl")]
    public string? StreamUrl { get; set; }
}
