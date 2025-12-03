using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL;

/// <summary>
/// The controller for the MediathekViewDL plugin API.
/// </summary>
[ApiController]
[Route("MediathekViewDL")]
public class MediathekViewDlApiService : ControllerBase
{
    private readonly MediathekViewApiClient _apiClient;
    private readonly ILogger<MediathekViewDlApiService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewDlApiService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiClient">The api client.</param>
    public MediathekViewDlApiService(ILogger<MediathekViewDlApiService> logger, MediathekViewApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Searches for media.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="minDuration">Optional minimum duration in seconds.</param>
    /// <param name="maxDuration">Optional maximum duration in seconds.</param>
    /// <returns>A list of search results.</returns>
    [HttpGet("Search")]
    public async Task<ActionResult<List<ResultItem>>> Search(
        [FromQuery] string query,
        [FromQuery] int? minDuration,
        [FromQuery] int? maxDuration)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query cannot be empty.");
        }

        var results = await _apiClient.SearchAsync(query, minDuration, maxDuration, default).ConfigureAwait(false);
        if (results == null)
        {
            return StatusCode(500, "An error occurred while searching.");
        }

        return Ok(results);
    }

    /// <summary>
    /// Triggers a download for a single item.
    /// </summary>
    /// <param name="item">The item to download.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("Download")]
    public IActionResult Download([FromBody] ResultItem item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.UrlVideo))
        {
            return BadRequest("Invalid item provided for download.");
        }

        _logger.LogInformation("Manual download requested for item: {Title}", item.Title);

        // TODO: Implement actual download logic by adding to a download queue/manager.
        // For now, we just acknowledge the request.

        return Ok($"Download request for '{item.Title}' received.");
    }
}
