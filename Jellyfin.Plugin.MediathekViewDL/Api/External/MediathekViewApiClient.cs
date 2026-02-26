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

    private const string ZappBaseApiUrl = "https://api.zapp.mediathekview.de/v1";
    private const string ZappChannelsEndpoint = ZappBaseApiUrl + "/channelInfoList";
    private const string ZappShowsEndpoint = ZappBaseApiUrl + "/shows/";

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
        var pageSize = _configurationProvider.Configuration.Search.PageSize;
        var maxPages = _configurationProvider.Configuration.Search.MaxPages;
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
                Future = _configurationProvider.Configuration.Search.SearchInFutureBroadcasts
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

    private static void PopulateQueries(ApiQueryDto apiQuery, string? title, string? topic, string? channel, string? combinedSearch)
    {
        AddQuery(title, [QueryFieldType.Title]);
        AddQuery(topic, [QueryFieldType.Topic]);
        AddQuery(channel, [QueryFieldType.Channel]);
        AddQuery(combinedSearch, [QueryFieldType.Title, QueryFieldType.Topic]);
        return;

        void AddQuery(string? input, Collection<QueryFieldType> fields)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            var queries = GenerateQueryFromCsv(input, fields);
            foreach (var item in queries)
            {
                apiQuery.Queries.Add(item);
            }
        }
    }

    private static List<QueryFieldsDto> GenerateQueryFromCsv(string? input, Collection<QueryFieldType> fields)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(q =>
            new QueryFieldsDto()
            {
                Fields = fields,
                Query = q.StartsWith('!') ? q[1..] : q,
                IsExclude = q.StartsWith('!')
            }).ToList();
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

            var upgradeToHttps = !(_configurationProvider.ConfigurationOrNull?.Network.AllowHttp ?? false);
            var dto = apiResult.Result.ToDto(apiQueryDto, upgradeToHttps);

            // Filter results locally based on exclusion criteria
            dto = FilterResults(dto, apiQueryDto);

            if (_configurationProvider.ConfigurationOrNull?.Search.FetchStreamSizes == true)
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

    private static QueryResultDto FilterResults(QueryResultDto dto, ApiQueryDto apiQueryDto)
    {
        var excludes = apiQueryDto.Queries.Where(q => q.IsExclude).ToList();
        if (excludes.Count == 0)
        {
            return dto;
        }

        var filteredResults = dto.Results.Where(item =>
        {
            return !excludes.Any(exclude => MatchesExclude(item, exclude));
        }).ToList();

        return dto with { Results = filteredResults };
    }

    private static bool MatchesExclude(ResultItemDto item, QueryFieldsDto exclude)
    {
        if (string.IsNullOrWhiteSpace(exclude.Query))
        {
            return false;
        }

        foreach (var field in exclude.Fields)
        {
            var valueToSearch = field switch
            {
                QueryFieldType.Title => item.Title,
                QueryFieldType.Topic => item.Topic,
                QueryFieldType.Description => item.Description,
                QueryFieldType.Channel => item.Channel,
                _ => string.Empty
            };

            if (valueToSearch.Contains(exclude.Query, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ZappChannelDto>> GetZappChannelsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving Zapp channels from: {Url}", ZappChannelsEndpoint);

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.GetAsync(ZappChannelsEndpoint, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Zapp API request failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"Zapp API request failed with status code {response.StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var channels = await JsonSerializer.DeserializeAsync<Dictionary<string, ZappChannelInfo>>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            if (channels == null)
            {
                return Array.Empty<ZappChannelDto>();
            }

            return channels.Select(kvp => new ZappChannelDto
            {
                Id = kvp.Key,
                Name = kvp.Value.Name ?? kvp.Key,
                StreamUrl = kvp.Value.StreamUrl ?? string.Empty
            }).ToList();
        }
        catch (Exception ex) when (ex is not MediathekException)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling the Zapp API for channels");
            throw new MediathekApiException("An unexpected error occurred while calling the Zapp API for channels", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ZappShowDto>> GetCurrentZappShowAsync(string channelId, CancellationToken cancellationToken)
    {
        try
        {
            var url = ZappShowsEndpoint + channelId;
            _logger.LogDebug("Retrieving current Zapp show for channel {ChannelId} from: {Url}", channelId, url);

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.GetAsync(url, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Zapp API request failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"Zapp API request failed with status code {response.StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var showResponse = await JsonSerializer.DeserializeAsync<ZappShowResponse>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            if (showResponse?.Shows == null || showResponse.Shows.Count == 0)
            {
                return [];
            }

            return showResponse.Shows.Select(s => new ZappShowDto
            {
                Title = s.Title ?? "Unknown",
                Subtitle = s.Subtitle,
                Description = s.Description,
                StartTime = TryParseDateTimeOffset(s.StartTime),
                EndTime = TryParseDateTimeOffset(s.EndTime)
            }).ToList();
        }
        catch (Exception ex) when (ex is not MediathekException)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling the Zapp API for show info");
            throw new MediathekApiException("An unexpected error occurred while calling the Zapp API for show info", ex);
        }
    }

    private static DateTimeOffset? TryParseDateTimeOffset(string? input)
    {
        if (DateTimeOffset.TryParse(input, out var result))
        {
            return result;
        }

        return null;
    }
}
