namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Options for searching the Mediathek.
/// </summary>
public record SearchOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to fetch the stream size for search results.
    /// This requires an additional HTTP request per result and may slow down the search.
    /// </summary>
    public bool FetchStreamSizes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to search in future broadcasts when performing searches.
    /// </summary>
    public bool SearchInFutureBroadcasts { get; set; } = true;
}
