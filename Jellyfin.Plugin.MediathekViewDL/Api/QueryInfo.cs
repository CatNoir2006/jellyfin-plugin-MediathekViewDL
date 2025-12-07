using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

#nullable disable

/// <summary>
/// Contains metadata about the search query results.
/// </summary>
public class QueryInfo
{
    /// <summary>
    /// Gets or sets the timestamp of the film list used.
    /// </summary>
    [JsonPropertyName("filmlisteTimestamp")]
    public long FilmlisteTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the time taken by the search engine.
    /// </summary>
    [JsonPropertyName("searchEngineTime")]
    public string SearchEngineTime { get; set; }

    /// <summary>
    /// Gets or sets the number of results returned in this response.
    /// </summary>
    [JsonPropertyName("resultCount")]
    public int ResultCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of results found for the query.
    /// </summary>
    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    /// <summary>
    /// Gets or sets the relation of the total results (e.g. "eq").
    /// </summary>
    [JsonPropertyName("totalRelation")]
    public string TotalRelation { get; set; }

    /// <summary>
    /// Gets or sets the total number of entries in the database.
    /// </summary>
    [JsonPropertyName("totalEntries")]
    public long TotalEntries { get; set; }
}
