using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// A client for the MediathekViewWeb API.
/// </summary>
public class MediathekViewApiClient : IMediathekViewApiClient
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
    /// <returns>A collection of result items.</returns>
    /// <exception cref="MediathekException">Thrown when an error occurs while calling the API.</exception>
    public async Task<Collection<ResultItem>> SearchAsync(
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
        return res.Results;
    }

    /// <summary>
    /// Searches for media on the MediathekViewWeb API using a specified query.
    /// </summary>
    /// <param name="apiQuery">The api query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result.</returns>
    /// <exception cref="MediathekException">Thrown when an error occurs while calling the API.</exception>
    public async Task<ResultChannels> SearchAsync(
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
                throw new MediathekApiException($"API request failed with status code {response.StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var apiResult = await JsonSerializer.DeserializeAsync<ApiResult>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            if (apiResult?.Result == null)
            {
                throw new MediathekParsingException("Failed to deserialize API result or result was null.");
            }

            _logger.LogInformation("API search returned {Count} results", apiResult.Result.Results.Count);
            return apiResult.Result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "A network error occurred while calling the MediathekViewWeb API");
            throw new MediathekConnectionException("A network error occurred while calling the MediathekViewWeb API", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "A parsing error occurred while deserializing the MediathekViewWeb API response");
            throw new MediathekParsingException("A parsing error occurred while deserializing the MediathekViewWeb API response", ex);
        }
        catch (Exception ex) when (ex is not MediathekException)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling the MediathekViewWeb API");
            throw new MediathekApiException("An unexpected error occurred while calling the MediathekViewWeb API", ex);
        }
    }
}
