using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

/// <summary>
/// Represents the response for a show query in the Zapp API.
/// </summary>
public record ZappShowResponse
{
    /// <summary>
    /// Gets or sets the list of shows.
    /// </summary>
    [JsonPropertyName("shows")]
    public IReadOnlyList<ZappShow>? Shows { get; set; }
}
