using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// Defines the request body for the MediathekViewWeb API.
/// </summary>
public class ApiQuery
{
    /// <summary>
    /// Gets the list of queries to filter the search.
    /// </summary>
    [JsonPropertyName("queries")]
    public Collection<QueryFields> Queries { get; init; } = new();

    /// <summary>
    /// Gets or sets the field to sort by.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public string SortBy { get; set; } = "timestamp";

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public string SortOrder { get; set; } = "desc";

    /// <summary>
    /// Gets or sets a value indicating whether to include future broadcasts.
    /// </summary>
    [JsonPropertyName("future")]
    public bool Future { get; set; }

    /// <summary>
    /// Gets or sets the offset for pagination.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    /// <summary>
    /// Gets or sets the number of results to return.
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; } = 25;

    /// <summary>
    /// Gets or sets the minimum duration in seconds for search results.
    /// </summary>
    [JsonPropertyName("duration_min")]
    public int? MinDuration { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration in seconds for search results.
    /// </summary>
    [JsonPropertyName("duration_max")]
    public int? MaxDuration { get; set; }
}
