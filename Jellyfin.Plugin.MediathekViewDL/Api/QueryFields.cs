using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// Defines a filter for the search.
/// </summary>
public class QueryFields
{
    /// <summary>
    /// Gets the fields to search in.
    /// </summary>
    [JsonPropertyName("fields")]
    public Collection<string> Fields { get; init; } = new() { };

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    [JsonPropertyName("query")]
    public required string Query { get; set; }
}
