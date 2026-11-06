namespace MediathekView.Api.External;

/// <summary>
/// Options for the <see cref="MediathekViewApiClient"/>.
/// </summary>
public class MediathekViewApiClientOptions
{
    /// <summary>
    /// The default base URL of the MediathekViewWeb server.
    /// </summary>
    public const string DefaultApiBaseUrl = "https://mediathekviewweb.de";

    /// <summary>
    /// The response the MediathekViewWeb server sends on its stats health endpoint when it is up and running.
    /// </summary>
    public const string ExpectedStatsResponse = "Server is up and running";

    /// <summary>
    /// Gets or sets the base URL of the MediathekViewWeb server.
    /// The <c>/api</c> path is added automatically when building API endpoint URLs.
    /// Can be changed to point at a local test server instance, e.g. http://localhost:8000.
    /// </summary>
    public string ApiBaseUrl { get; set; } = DefaultApiBaseUrl;

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
