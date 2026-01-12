using System;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Data transfer object for query information.
/// </summary>
public record QueryInfoDto
{
    /// <summary>
    /// Gets the timestamp of the film list used.
    /// </summary>
    public long MovieListTimestamp { get; init; }

    /// <summary>
    /// Gets the time taken by the search engine.
    /// </summary>
    public TimeSpan SearchEngineTime { get; init; }

    /// <summary>
    /// Gets the number of results returned in this response.
    /// </summary>
    public int ResultCount { get; init; }

    /// <summary>
    /// Gets the total number of results found for the query.
    /// </summary>
    public int TotalResults { get; init; }

    /// <summary>
    /// Gets the relation of the total results.
    /// </summary>
    public TotalRelation TotalRelation { get; init; }

    /// <summary>
    /// Gets the total number of entries in the database.
    /// </summary>
    public long TotalEntries { get; init; }
}
