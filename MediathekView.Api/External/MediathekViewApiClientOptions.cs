namespace MediathekView.Api.External;

/// <summary>
/// Options for the <see cref="MediathekViewApiClient"/>.
/// </summary>
public class MediathekViewApiClientOptions
{
    /// <summary>
    /// Gets or sets the maximum number of pages requested while gathering a page of search results from the API.
    /// </summary>
    public int MaxSearchPages { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether plain HTTP video URLs should stay untouched instead of being upgraded to HTTPS.
    /// </summary>
    public bool AllowHttp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client should enrich the search results with the stream sizes of the video urls.
    /// </summary>
    public bool FetchStreamSizes { get; set; }
}
