using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// A client for the MediathekViewWeb API.
/// </summary>
public class MediathekViewApiClient
{
    private const string ApiUrl = "https://mediathekviewweb.de/api/query";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MediathekViewApiClient> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewApiClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    /// <param name="logger">The logger.</param>
    public MediathekViewApiClient(IHttpClientFactory httpClientFactory, ILogger<MediathekViewApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    /// <summary>
    /// Searches for media on the MediathekViewWeb API.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="minDuration">Optional minimum duration in seconds.</param>
    /// <param name="maxDuration">Optional maximum duration in seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of result items, or null if an error occurred.</returns>
    public async Task<Collection<ResultItem>?> SearchAsync(
        string searchQuery,
        int? minDuration,
        int? maxDuration,
        CancellationToken cancellationToken)
    {
        var apiQuery = new ApiQuery
        {
            Queries = new Collection<QueryFields>
            {
                new() { Query = searchQuery }
            },
            Size = 50, // Get a decent number of results
            MinDuration = minDuration,
            MaxDuration = maxDuration
        };
        var res = await SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
        return res?.Results;
    }

    /// <summary>
    /// Searches for media on the MediathekViewWeb API using a specified query.
    /// </summary>
    /// <param name="apiQuery">The api query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result, or null if an error occurred.</returns>
    public async Task<ResultChannels?> SearchAsync(
        ApiQuery apiQuery,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(apiQuery);
            _logger.LogDebug("Performing API search with payload: {Json}", json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(ApiUrl, content, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API request failed with status code {StatusCode}", response.StatusCode);
                return null;
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var apiResult = await JsonSerializer.DeserializeAsync<ApiResult>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("API search returned {Count} results", apiResult?.Result.Results.Count);
            return apiResult?.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling the MediathekViewWeb API");
            return null;
        }
    }
}
