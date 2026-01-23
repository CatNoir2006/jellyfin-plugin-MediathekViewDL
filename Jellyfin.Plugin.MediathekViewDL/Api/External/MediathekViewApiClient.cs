using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Converters;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
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
    private const string BaseApiUrl = "https://mediathekviewweb.de/api";
    private const string SearchEndpoint = BaseApiUrl + "/query";
    private const string StreamSizeEndpoint = BaseApiUrl + "/content-length?url=";
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

    /// <inheritdoc/>
    /// <exception cref="MediathekException">Thrown when an error occurs while calling the API.</exception>
    public async Task<IReadOnlyCollection<ResultItemDto>> SearchAsync(
        string? title,
        string? topic,
        string? channel,
        string? combinedSearch,
        int? minDuration,
        int? maxDuration,
        DateTimeOffset? minBroadcastDate,
        DateTimeOffset? maxBroadcastDate,
        CancellationToken cancellationToken)
    {
        var allResults = new List<ResultItemDto>();
        const int pageSize = 50;
        const int maxPages = 5;
        var currentOffset = 0;
        var page = 0;

        while (allResults.Count < pageSize && page < maxPages)
        {
            var apiQuery = new ApiQueryDto
            {
                Size = pageSize,
                Offset = currentOffset,
                MinDuration = minDuration,
                MaxDuration = maxDuration,
                MinBroadcastDate = minBroadcastDate,
                MaxBroadcastDate = maxBroadcastDate,
            };

            PopulateQueries(apiQuery, title, topic, channel, combinedSearch);

            if (apiQuery.Queries.Count == 0)
            {
                return Array.Empty<ResultItemDto>();
            }

            var res = await SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
            allResults.AddRange(res.Results);

            if (currentOffset + pageSize >= res.QueryInfo.TotalResults)
            {
                break;
            }

            currentOffset += pageSize;
            page++;
        }

        return allResults;
    }

    private void PopulateQueries(ApiQueryDto apiQuery, string? title, string? topic, string? channel, string? combinedSearch)
    {
        var titles = SplitAndClean(title);
        foreach (var titleItem in titles)
        {
            var query = new QueryFieldsDto { Query = titleItem };
            query.Fields.Add(QueryFieldType.Title);
            apiQuery.Queries.Add(query);
        }

        var topics = SplitAndClean(topic);
        foreach (var topicItem in topics)
        {
            var query = new QueryFieldsDto { Query = topicItem };
            query.Fields.Add(QueryFieldType.Topic);
            apiQuery.Queries.Add(query);
        }

        var channels = SplitAndClean(channel);
        foreach (var channelItem in channels)
        {
            var query = new QueryFieldsDto { Query = channelItem };
            query.Fields.Add(QueryFieldType.Channel);
            apiQuery.Queries.Add(query);
        }

        if (!string.IsNullOrWhiteSpace(combinedSearch))
        {
            var combinedQueries = SplitAndClean(combinedSearch);
            foreach (var combinedQueryItem in combinedQueries)
            {
                var query = new QueryFieldsDto { Query = combinedQueryItem };
                query.Fields.Add(QueryFieldType.Title);
                query.Fields.Add(QueryFieldType.Topic);
                apiQuery.Queries.Add(query);
            }
        }
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
    /// <param name="apiQueryDto">The api query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result.</returns>
    /// <exception cref="MediathekException">Thrown when an error occurs while calling the API.</exception>
    public async Task<QueryResultDto> SearchAsync(
        ApiQueryDto apiQueryDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiQuery = apiQueryDto.ToModel();
            var json = JsonSerializer.Serialize(apiQuery);
            _logger.LogDebug("Performing API search with payload: {Json}", json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.PostAsync(SearchEndpoint, content, ct).ConfigureAwait(false),
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

            var upgradeToHttps = !(_configurationProvider.ConfigurationOrNull?.AllowHttp ?? false);
            var dto = apiResult.Result.ToDto(apiQueryDto, upgradeToHttps);

            if (_configurationProvider.ConfigurationOrNull?.FetchStreamSizes == true)
            {
                var newResults = new List<ResultItemDto>();
                foreach (var item in dto.Results)
                {
                    var videoUrlTasks = item.VideoUrls.Select(async v =>
                    {
                        try
                        {
                            var size = await GetStreamSizeAsync(v.Url, cancellationToken).ConfigureAwait(false);
                            return v with { Size = size };
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to retrieve size for video URL: {Url}", v.Url);
                            return v;
                        }
                    });

                    var newVideoUrls = await Task.WhenAll(videoUrlTasks).ConfigureAwait(false);
                    newResults.Add(item with { VideoUrls = newVideoUrls.ToList() });
                }

                return dto with { Results = newResults };
            }

            return dto;
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

    /// <inheritdoc />
    public async Task<long> GetStreamSizeAsync(string streamUrl, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving stream size for URL: {StreamUrl}", streamUrl);
            var url = StreamSizeEndpoint + Uri.EscapeDataString(streamUrl);

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.GetAsync(url, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API request failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"API request failed with status code {response.StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!long.TryParse(responseStream, out var fileSize))
            {
                _logger.LogError("Failed to parse stream size from response: {Response}", responseStream);
                throw new MediathekParsingException("Failed to parse stream size from API response.");
            }

            return fileSize;
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
