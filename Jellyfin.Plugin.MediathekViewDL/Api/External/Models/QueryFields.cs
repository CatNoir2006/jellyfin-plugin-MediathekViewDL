using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

/// <summary>
/// Defines a filter for the search.
/// </summary>
public class QueryFields
{
    /// <summary>
    /// Gets the fields to search in. Possible values are "title", "topic", "description", and "channel".
    /// </summary>
    [JsonPropertyName("fields")]
    public Collection<string> Fields { get; init; } = new() { };

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    [JsonPropertyName("query")]
    public required string Query { get; set; }
}
