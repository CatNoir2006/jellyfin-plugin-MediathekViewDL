using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

/// <summary>
/// Response model for the topics API.
/// </summary>
public class ApiTopicsResponse
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// Gets the list of topics.
    /// </summary>
    [JsonPropertyName("topics")]
    public Collection<string>? Topics { get; init; }
}
