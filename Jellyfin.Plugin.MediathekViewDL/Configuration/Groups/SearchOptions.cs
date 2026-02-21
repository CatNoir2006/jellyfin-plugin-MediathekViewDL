namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Options for searching the Mediathek.
/// </summary>
public record SearchOptions
{
    /// <summary>
    /// Gets a value indicating whether to fetch the stream size for search results.
    /// This requires an additional HTTP request per result and may slow down the search.
    /// </summary>
    public bool FetchStreamSizes { get; init; }

    /// <summary>
    /// Gets a value indicating whether to search in future broadcasts when performing searches.
    /// </summary>
    public bool SearchInFutureBroadcasts { get; init; } = true;
}
