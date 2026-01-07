using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External;

/// <summary>
/// A client for the MediathekViewWeb API.
/// </summary>
public class MediathekViewApiClient : IMediathekViewApiClient
{
    private const string ApiUrl = "https://mediathekviewweb.de/api/query";
    private readonly HttpClient _httpClient;
    private readonly ILogger<MediathekViewApiClient> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private static readonly AsyncPolicy<HttpResponseMessage> _resiliencePolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
        .WrapAsync(Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The http client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    public MediathekViewApiClient(HttpClient httpClient, ILogger<MediathekViewApiClient> logger, IConfigurationProvider configurationProvider)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configurationProvider = configurationProvider;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
    }

    /// <summary>
    /// Searches for media on the MediathekViewWeb API.
    /// </summary>
    /// <param name="title">The title query.</param>
    /// <param name="topic">The topic filter.</param>
    /// <param name="channel">The channel filter.</param>
    /// <param name="combinedSearch">The combined search query (Title, Topic).</param>
    /// <param name="minDuration">Optional minimum duration in seconds.</param>
    /// <param name="maxDuration">Optional maximum duration in seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of result items.</returns>
    /// <exception cref="MediathekException">Thrown when an error occurs while calling the API.</exception>
    public async Task<Collection<ResultItem>> SearchAsync(
        string? title,
        string? topic,
        string? channel,
        string? combinedSearch,
        int? minDuration,
        int? maxDuration,
        CancellationToken cancellationToken)
    {
        var apiQuery = new ApiQuery
        {
            Size = 50, // Get a decent number of results
            MinDuration = minDuration,
            MaxDuration = maxDuration,
            Queries = new Collection<QueryFields>()
        };

        var titles = SplitAndClean(title);
        foreach (var titleItem in titles)
        {
            apiQuery.Queries.Add(new QueryFields { Query = titleItem, Fields = ["title"] });
        }

        var topics = SplitAndClean(topic);
        foreach (var topicItem in topics)
        {
            apiQuery.Queries.Add(new QueryFields { Query = topicItem, Fields = ["topic"] });
        }

        var channels = SplitAndClean(channel);
        foreach (var channelItem in channels)
        {
            apiQuery.Queries.Add(new QueryFields { Query = channelItem, Fields = ["channel"] });
        }

        if (!string.IsNullOrWhiteSpace(combinedSearch))
        {
            var fields = new Collection<string> { "title", "topic" };

            var combinedQueries = SplitAndClean(combinedSearch);
            foreach (var combinedQueryItem in combinedQueries)
            {
                apiQuery.Queries.Add(new QueryFields { Query = combinedQueryItem, Fields = fields });
            }
        }

        if (apiQuery.Queries.Count == 0)
        {
            return new Collection<ResultItem>();
        }

        var res = await SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
        return res.Results;
    }

    private static List<string> SplitAndClean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
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
            var json = JsonSerializer.Serialize(apiQuery);
            _logger.LogDebug("Performing API search with payload: {Json}", json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.PostAsync(ApiUrl, content, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

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
            ChannelUrlHttpsUpgrade(apiResult.Result);
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

    private void ChannelUrlHttpsUpgrade(ResultChannels? channels)
    {
        if (channels?.Results == null || channels.Results.Count == 0 || _configurationProvider.ConfigurationOrNull?.AllowHttp == true)
        {
            return;
        }

        foreach (var channel in channels.Results)
        {
            channel.UrlSubtitle = UrlHttpsUpgrade(channel.UrlSubtitle);
            channel.UrlVideo = UrlHttpsUpgrade(channel.UrlVideo);
            channel.UrlVideoHd = UrlHttpsUpgrade(channel.UrlVideoHd);
            channel.UrlVideoLow = UrlHttpsUpgrade(channel.UrlVideoLow);
        }
    }

    private string UrlHttpsUpgrade(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var uriRes) && uriRes.Scheme == Uri.UriSchemeHttp)
        {
            var builder = new UriBuilder(uriRes) { Scheme = Uri.UriSchemeHttps };
            return builder.ToString();
        }

        return uri;
    }
}
