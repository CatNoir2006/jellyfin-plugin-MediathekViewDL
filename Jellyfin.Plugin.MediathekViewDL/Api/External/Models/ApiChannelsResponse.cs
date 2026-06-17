using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

/// <summary>
/// Response model for the channels API.
/// </summary>
public class ApiChannelsResponse
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// Gets the list of channels.
    /// </summary>
    [JsonPropertyName("channels")]
    public Collection<string>? Channels { get; init; }
}
