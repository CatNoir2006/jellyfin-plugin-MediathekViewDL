using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Controller;

/// <summary>
/// The Controller to search on MediathekView.
/// </summary>
[ApiController]
[Route("MediathekViewDL/[controller]")]
[Authorize(Policy = Policies.RequiresElevation)]
public class SearchController : ControllerBase
{
    private readonly IMediathekViewApiClient _apiClient;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IQueryParser _queryParser;
    private readonly ILogger<SearchController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class.
    /// </summary>
    /// <param name="apiClient">The MediathekView API client.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="queryParser">The query parser.</param>
    /// <param name="logger">The logger.</param>
    public SearchController(IMediathekViewApiClient apiClient, IConfigurationProvider configurationProvider, IQueryParser queryParser, ILogger<SearchController> logger)
    {
        _apiClient = apiClient;
        _configurationProvider = configurationProvider;
        _queryParser = queryParser;
        _logger = logger;
    }

    /// <summary>
    /// Searches for media.
    /// </summary>
    /// <param name="title">The title query.</param>
    /// <param name="topic">The topic filter.</param>
    /// <param name="channel">The channel filter.</param>
    /// <param name="combinedSearch">The combined search query (Title, Topic).</param>
    /// <param name="minDuration">Optional minimum duration in seconds.</param>
    /// <param name="maxDuration">Optional maximum duration in seconds.</param>
    /// <param name="minBroadcastDate">The minimum Date of the Item.</param>
    /// <param name="maxBroadcastDate">The max Age of the Item. (e.g. In Future).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of search results.</returns>
    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<ResultItemDto>>> Search(
        [FromQuery] string? title,
        [FromQuery] string? topic,
        [FromQuery] string? channel,
        [FromQuery] string? combinedSearch,
        [FromQuery] int? minDuration,
        [FromQuery] int? maxDuration,
        [FromQuery] DateTimeOffset? minBroadcastDate,
        [FromQuery] DateTimeOffset? maxBroadcastDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title) &&
            string.IsNullOrWhiteSpace(topic) &&
            string.IsNullOrWhiteSpace(channel) &&
            string.IsNullOrWhiteSpace(combinedSearch))
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidSearch, "Mindestens ein Suchparameter (Titel, Thema, Sender oder kombinierte Suche) muss angegeben werden."));
        }

        try
        {
            var queries = _queryParser.Parse(title, topic, channel, combinedSearch);
            var apiQuery = new ApiQueryDto
            {
                Queries = queries,
                MinDuration = minDuration,
                MaxDuration = maxDuration,
                MinBroadcastDate = minBroadcastDate,
                MaxBroadcastDate = maxBroadcastDate,
                Future = _configurationProvider.Configuration.Search.SearchInFutureBroadcasts,
                Size = _configurationProvider.Configuration.Search.PageSize,
            };

            var results = await _apiClient.SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
            return Ok(results.Results);
        }
        catch (MediathekConnectionException ex)
        {
            _logger.LogError(ex, "Connection error while searching.");
            return StatusCode(503, new ApiErrorDto(ApiErrorId.MediathekUnavailable, "Die MediathekView API ist derzeit nicht erreichbar. Bitte versuchen Sie es später erneut."));
        }
        catch (MediathekParsingException ex)
        {
            _logger.LogError(ex, "Parsing error while searching.");
            return StatusCode(502, new ApiErrorDto(ApiErrorId.MediathekInvalidResponse, "Ungültige Antwort von der MediathekView API erhalten."));
        }
        catch (MediathekApiException ex)
        {
            _logger.LogError(ex, "API error while searching. Status code: {StatusCode}", ex.StatusCode);
            var statusCode = (int)ex.StatusCode >= 500 ? 502 : 500;
            return StatusCode(statusCode, new ApiErrorDto(ApiErrorId.MediathekApiError, $"Die MediathekView API hat einen Fehler zurückgegeben ({ex.StatusCode})."));
        }
        catch (MediathekException ex)
        {
            _logger.LogError(ex, "An error occurred while searching.");
            return StatusCode(500, new ApiErrorDto(ApiErrorId.MediathekError, "Ein unerwarteter Fehler ist beim Aufruf der MediathekView API aufgetreten."));
        }
    }

    /// <summary>
    /// Converts search parameters into subscription criteria.
    /// </summary>
    /// <param name="title">The title query.</param>
    /// <param name="topic">The topic filter.</param>
    /// <param name="channel">The channel filter.</param>
    /// <param name="combinedSearch">The combined search query (Title, Topic).</param>
    /// <returns>A list of query fields.</returns>
    [HttpGet("Criteria")]
    public ActionResult<IEnumerable<QueryFieldsDto>> GetCriteriaForSearch(
        [FromQuery] string? title,
        [FromQuery] string? topic,
        [FromQuery] string? channel,
        [FromQuery] string? combinedSearch)
    {
        return Ok(_queryParser.Parse(title, topic, channel, combinedSearch));
    }
}
