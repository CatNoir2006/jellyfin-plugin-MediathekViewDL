using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// The controller for the MediathekViewDL plugin API.
/// </summary>
[ApiController]
[Route("MediathekViewDL")]
public class MediathekViewDlApiService : ControllerBase
{
    private readonly MediathekViewApiClient _apiClient;
    private readonly ILogger<MediathekViewDlApiService> _logger;
    private readonly FileDownloader _fileDownloader;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewDlApiService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiClient">The api client.</param>
    /// <param name="fileDownloader">The file downloader.</param>
    public MediathekViewDlApiService(ILogger<MediathekViewDlApiService> logger, MediathekViewApiClient apiClient, FileDownloader fileDownloader)
    {
        _logger = logger;
        _apiClient = apiClient;
        _fileDownloader = fileDownloader;
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
        var config = Plugin.Instance?.Configuration;
        if (config == null || string.IsNullOrWhiteSpace(config.DefaultDownloadPath))
        {
            _logger.LogError("Default download path is not configured. Cannot start manual download.");
            return BadRequest("Default download path is not configured.");
        }

        var videoUrl = item.UrlVideoHd ?? item.UrlVideo ?? item.UrlVideoLow;

        if (item == null || string.IsNullOrWhiteSpace(videoUrl))
        {
            return BadRequest("Invalid item provided for download (no video URL).");
        }

        _logger.LogInformation("Manual download requested for item: {Title}", item.Title);

        // Fire and forget
        Task.Run(async () =>
        {
            var sanitizedTitle = string.Join("_", item.Title.Split(Path.GetInvalidFileNameChars()));
            var manualDownloadFolder = Path.Combine(config.DefaultDownloadPath, "manual");

            // Download Video
            var videoDestinationPath = Path.Combine(manualDownloadFolder, sanitizedTitle + ".mp4");
            _logger.LogInformation("Starting manual video download of '{Title}' to '{Path}'", item.Title, videoDestinationPath);
            var videoSuccess = await _fileDownloader.DownloadFileAsync(videoUrl, videoDestinationPath, null, CancellationToken.None).ConfigureAwait(false);
            if (videoSuccess)
            {
                _logger.LogInformation("Successfully finished manual video download of '{Title}'.", item.Title);
            }
            else
            {
                _logger.LogError("Failed to manually download video for '{Title}'.", item.Title);
            }

            // Download Subtitle
            if (config.DownloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
            {
                var subtitleDestinationPath = Path.Combine(manualDownloadFolder, sanitizedTitle + ".ttml");
                _logger.LogInformation("Starting manual subtitle download of '{Title}' to '{Path}'", item.Title, subtitleDestinationPath);
                var subtitleSuccess = await _fileDownloader.DownloadFileAsync(item.UrlSubtitle, subtitleDestinationPath, null, CancellationToken.None).ConfigureAwait(false);
                if (subtitleSuccess)
                {
                    _logger.LogInformation("Successfully finished manual subtitle download of '{Title}'.", item.Title);
                }
                else
                {
                    _logger.LogError("Failed to manually download subtitle for '{Title}'.", item.Title);
                }
            }
        });

        return Ok($"Download for '{item.Title}' started in the background.");
    }
}
