using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediathekView.Api.Converters;
using MediathekView.Api.Exceptions;
using MediathekView.Api.External;
using MediathekView.Api.External.Models;
using MediathekView.Api.Models;
using MediathekView.Api.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;

namespace MediathekView.Api.External;

/// <summary>
/// A client for the MediathekViewWeb API.
/// </summary>
public class MediathekViewApiClient : IMediathekViewApiClient
{
    private static readonly SemaphoreSlim _channelsSemaphore = new(1, 1);
    private static readonly SemaphoreSlim _topicsSemaphore = new(1, 1);

    private static CacheEntry? _channelsCache;
    private static CacheEntry? _topicsCache;

    private readonly HttpClient _httpClient;
    private readonly ILogger<MediathekViewApiClient> _logger;
    private readonly IOptionsMonitor<MediathekViewApiClientOptions> _optionsMonitor;
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
    /// <param name="optionsMonitor">The options monitor supplying the client options.</param>
    public MediathekViewApiClient(HttpClient httpClient, ILogger<MediathekViewApiClient> logger, IOptionsMonitor<MediathekViewApiClientOptions> optionsMonitor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _optionsMonitor = optionsMonitor;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
    }

    /// <summary>
    /// Gets the search endpoint, derived from the configured API base URL.
    /// </summary>
    private string SearchEndpoint => BuildApiEndpoint("/query");

    /// <summary>
    /// Gets the stream size endpoint, derived from the configured API base URL.
    /// </summary>
    private string StreamSizeEndpoint => BuildApiEndpoint("/content-length?url=");

    /// <summary>
    /// Gets the channels endpoint, derived from the configured API base URL.
    /// </summary>
    private string ChannelsEndpoint => BuildApiEndpoint("/channels");

    /// <summary>
    /// Gets the topics endpoint, derived from the configured API base URL.
    /// </summary>
    private string TopicsEndpoint => BuildApiEndpoint("/topics");

    /// <summary>
    /// Gets the Zapp channels endpoint, derived from the configured Zapp API base URL.
    /// </summary>
    private string ZappChannelsEndpoint => GetZappBaseUrl(_optionsMonitor.CurrentValue.ZappApiBaseUrl) + "/v1/channelInfoList";

    /// <summary>
    /// Gets the Zapp shows endpoint, derived from the configured Zapp API base URL.
    /// </summary>
    private string ZappShowsEndpoint => GetZappBaseUrl(_optionsMonitor.CurrentValue.ZappApiBaseUrl) + "/v1/shows/";

    /// <summary>
    /// Builds a MediathekViewWeb API endpoint URL from the configured base URL.
    /// The <c>/api</c> path is added between the base URL and the endpoint path.
    /// Falls back to the default URL when no valid base URL is configured.
    /// </summary>
    /// <param name="relativeEndpoint">The endpoint path, starting with a forward slash.</param>
    /// <returns>The absolute endpoint URL.</returns>
    /// <exception cref="MediathekApiException">Thrown when the configured base URL is not a valid absolute http(s) URL.</exception>
    private string BuildApiEndpoint(string relativeEndpoint)
    {
        return GetApiBaseUrl(_optionsMonitor.CurrentValue.ApiBaseUrl) + "/api" + relativeEndpoint;
    }

    /// <summary>
    /// Gets the normalized base URL of a MediathekViewWeb server.
    /// Falls back to the default URL when no base URL is given.
    /// </summary>
    /// <param name="configuredUrl">The base URL to normalize. May be null or empty, in which case the default URL is used.</param>
    /// <returns>The absolute server root URL without a trailing slash.</returns>
    /// <exception cref="MediathekApiException">Thrown when the given base URL is not a valid absolute http(s) URL.</exception>
    private string GetApiBaseUrl(string? configuredUrl)
    {
        var baseUrl = string.IsNullOrWhiteSpace(configuredUrl)
            ? MediathekViewApiClientOptions.DefaultApiBaseUrl
            : configuredUrl.Trim().TrimEnd('/');

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new MediathekApiException($"The configured API base URL '{configuredUrl}' is not a valid absolute http(s) URL.");
        }

        return baseUrl;
    }

    /// <summary>
    /// Gets the normalized base URL of a Zapp API server.
    /// Falls back to the default URL when no base URL is given.
    /// </summary>
    /// <param name="configuredUrl">The base URL to normalize. May be null or empty, in which case the default URL is used.</param>
    /// <returns>The absolute server root URL without a trailing slash.</returns>
    /// <exception cref="MediathekApiException">Thrown when the given base URL is not a valid absolute http(s) URL.</exception>
    private string GetZappBaseUrl(string? configuredUrl)
    {
        var baseUrl = string.IsNullOrWhiteSpace(configuredUrl)
            ? MediathekViewApiClientOptions.DefaultZappApiBaseUrl
            : configuredUrl.Trim().TrimEnd('/');

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new MediathekApiException($"The configured Zapp API base URL '{configuredUrl}' is not a valid absolute http(s) URL.");
        }

        return baseUrl;
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
        var pageSize = apiQueryDto.Size;
        var maxPages = _optionsMonitor.CurrentValue.MaxSearchPages;
        var allResults = new List<ResultItemDto>();
        QueryInfoDto? lastQueryInfo = null;
        var currentOffset = apiQueryDto.Offset;
        var page = 0;

        while (allResults.Count < pageSize && page < maxPages)
        {
            var currentQuery = apiQueryDto with { Size = pageSize, Offset = currentOffset };
            var res = await PerformSearchInternalAsync(currentQuery, cancellationToken).ConfigureAwait(false);
            allResults.AddRange(res.Results);
            lastQueryInfo = res.QueryInfo;

            if (currentOffset + pageSize >= res.QueryInfo.TotalResults || allResults.Count >= pageSize)
            {
                break;
            }

            currentOffset += pageSize;
            page++;
        }

        return new QueryResultDto
        {
            QueryInfo = lastQueryInfo ?? new QueryInfoDto(),
            Results = allResults.Take(pageSize).ToList()
        };
    }

    private async Task<QueryResultDto> PerformSearchInternalAsync(
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

            var upgradeToHttps = !_optionsMonitor.CurrentValue.AllowHttp;
            var dto = apiResult.Result.ToDto(apiQueryDto, upgradeToHttps);

            // Filter results locally based on exclusion criteria
            dto = FilterResults(dto, apiQueryDto);

            if (_optionsMonitor.CurrentValue.FetchStreamSizes)
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

    /// <summary>
    /// Validates the given or configured MediathekViewWeb server URL by calling its stats health endpoint.
    /// The server must respond with status code 200 and the body "Server is up and running".
    /// </summary>
    /// <param name="baseUrlOverride">Optional base URL to validate. When null or empty, the configured base URL is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous check.</returns>
    /// <exception cref="MediathekConnectionException">Thrown when the server is not reachable.</exception>
    /// <exception cref="MediathekApiException">Thrown when the server responds with an unexpected status code or body, or when the configured base URL is invalid.</exception>
    public async Task CheckConnectionAsync(string? baseUrlOverride, CancellationToken cancellationToken)
    {
        var baseUrl = GetApiBaseUrl(string.IsNullOrWhiteSpace(baseUrlOverride)
            ? _optionsMonitor.CurrentValue.ApiBaseUrl
            : baseUrlOverride);
        var statsUrl = baseUrl + "/stats";
        _logger.LogDebug("Checking MediathekViewWeb server availability at: {Url}", statsUrl);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_optionsMonitor.CurrentValue.ConnectionCheckTimeoutSeconds));

        try
        {
            using var response = await _httpClient.GetAsync(statsUrl, timeoutCts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Stats health check of the MediathekViewWeb server failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"The MediathekViewWeb server at '{baseUrl}' responded with status code {response.StatusCode}.", response.StatusCode);
            }

            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token).ConfigureAwait(false);
            var normalizedBody = body.Trim().TrimEnd('.', ' ');
            if (!string.Equals(normalizedBody, MediathekViewApiClientOptions.ExpectedStatsResponse, StringComparison.Ordinal))
            {
                _logger.LogError("Stats health check of the MediathekViewWeb server returned an unexpected body: '{Body}'", body);
                throw new MediathekApiException($"The MediathekViewWeb server at '{baseUrl}' did not return the expected response. Expected '{MediathekViewApiClientOptions.ExpectedStatsResponse}' but got '{body.Trim()}'");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "The MediathekViewWeb server at {BaseUrl} is not reachable", baseUrl);
            throw new MediathekConnectionException($"The MediathekViewWeb server at '{baseUrl}' is not reachable.", ex);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("The connection check of the MediathekViewWeb server at {BaseUrl} timed out", baseUrl);
            throw new MediathekConnectionException($"The MediathekViewWeb server at '{baseUrl}' did not respond within {_optionsMonitor.CurrentValue.ConnectionCheckTimeoutSeconds} seconds.");
        }
    }

    /// <summary>
    /// Validates the given or configured Zapp API server URL by calling its channelInfoList endpoint.
    /// The server must respond with a status code 200 and a parsable JSON body (a dictionary of channel info entries).
    /// </summary>
    /// <param name="baseUrlOverride">Optional base URL to validate. When null or empty, the configured Zapp base URL is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous check.</returns>
    /// <exception cref="MediathekConnectionException">Thrown when the server is not reachable.</exception>
    /// <exception cref="MediathekApiException">Thrown when the server responds with an unexpected status code or a non-parsable body, or when the configured base URL is invalid.</exception>
    public async Task CheckZappConnectionAsync(string? baseUrlOverride, CancellationToken cancellationToken)
    {
        var baseUrl = GetZappBaseUrl(string.IsNullOrWhiteSpace(baseUrlOverride)
            ? _optionsMonitor.CurrentValue.ZappApiBaseUrl
            : baseUrlOverride);
        var channelsUrl = baseUrl + "/v1/channelInfoList";
        _logger.LogDebug("Checking Zapp API server availability at: {Url}", channelsUrl);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_optionsMonitor.CurrentValue.ConnectionCheckTimeoutSeconds));

        try
        {
            using var response = await _httpClient.GetAsync(channelsUrl, timeoutCts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Zapp connection check failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"The Zapp API server at '{baseUrl}' responded with status code {response.StatusCode}.", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(timeoutCts.Token).ConfigureAwait(false);
            Dictionary<string, ZappChannelInfo> channels;
            try
            {
                channels = await JsonSerializer.DeserializeAsync<Dictionary<string, ZappChannelInfo>>(responseStream, _jsonSerializerOptions, timeoutCts.Token).ConfigureAwait(false)
                    ?? throw new MediathekApiException($"The Zapp API server at '{baseUrl}' returned a non-parsable response body.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "The Zapp API server at {BaseUrl} returned a non-parsable response body", baseUrl);
                throw new MediathekApiException($"The Zapp API server at '{baseUrl}' returned a non-parsable response body.", ex);
            }

            _logger.LogDebug("Zapp connection check succeeded with {Count} channels", channels.Count);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "The Zapp API server at {BaseUrl} is not reachable", baseUrl);
            throw new MediathekConnectionException($"The Zapp API server at '{baseUrl}' is not reachable.", ex);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("The connection check of the Zapp API server at {BaseUrl} timed out", baseUrl);
            throw new MediathekConnectionException($"The Zapp API server at '{baseUrl}' did not respond within {_optionsMonitor.CurrentValue.ConnectionCheckTimeoutSeconds} seconds.");
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
    public async Task<IReadOnlyCollection<string>> GetChannelsAsync(CancellationToken cancellationToken)
    {
        var cache = _channelsCache;
        if (cache != null && DateTimeOffset.UtcNow < cache.Expiry)
        {
            return cache.Items;
        }

        await _channelsSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cache = _channelsCache;
            if (cache != null && DateTimeOffset.UtcNow < cache.Expiry)
            {
                return cache.Items;
            }

            _logger.LogDebug("Retrieving channels from: {Url}", ChannelsEndpoint);

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.GetAsync(ChannelsEndpoint, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Channels API request failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"Channels API request failed with status code {response.StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiChannelsResponse>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            if (apiResponse?.Channels == null)
            {
                return Array.Empty<string>();
            }

            var items = apiResponse.Channels.AsReadOnly();
            _channelsCache = new CacheEntry(items, DateTimeOffset.UtcNow.AddHours(24));
            return items;
        }
        catch (Exception ex) when (ex is not MediathekException)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling the channels API");
            throw new MediathekApiException("An unexpected error occurred while calling the channels API", ex);
        }
        finally
        {
            _channelsSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string>> GetTopicsAsync(CancellationToken cancellationToken)
    {
        var cache = _topicsCache;
        if (cache != null && DateTimeOffset.UtcNow < cache.Expiry)
        {
            return cache.Items;
        }

        await _topicsSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cache = _topicsCache;
            if (cache != null && DateTimeOffset.UtcNow < cache.Expiry)
            {
                return cache.Items;
            }

            _logger.LogDebug("Retrieving topics from: {Url}", TopicsEndpoint);

            var response = await _resiliencePolicy.ExecuteAsync(
                async ct => await _httpClient.GetAsync(TopicsEndpoint, ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Topics API request failed with status code {StatusCode}", response.StatusCode);
                throw new MediathekApiException($"Topics API request failed with status code {response.StatusCode}", response.StatusCode);
            }

            var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiTopicsResponse>(responseStream, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            if (apiResponse?.Topics == null)
            {
                return Array.Empty<string>();
            }

            var items = apiResponse.Topics.AsReadOnly();
            _topicsCache = new CacheEntry(items, DateTimeOffset.UtcNow.AddHours(24));
            return items;
        }
        catch (Exception ex) when (ex is not MediathekException)
        {
            _logger.LogError(ex, "An unexpected error occurred while calling the topics API");
            throw new MediathekApiException("An unexpected error occurred while calling the topics API", ex);
        }
        finally
        {
            _topicsSemaphore.Release();
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

    private sealed record CacheEntry(IReadOnlyCollection<string> Items, DateTimeOffset Expiry);
}
