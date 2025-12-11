using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
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
    private readonly IMediathekViewApiClient _apiClient;
    private readonly ILogger<MediathekViewDlApiService> _logger;
    private readonly IFileDownloader _fileDownloader;
    private readonly IFileNameBuilderService _fileNameBuilder;
    private readonly ISubscriptionProcessor _subscriptionProcessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewDlApiService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiClient">The api client.</param>
    /// <param name="fileDownloader">The file downloader.</param>
    /// <param name="fileNameBuilder">The file name builder.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    public MediathekViewDlApiService(
        ILogger<MediathekViewDlApiService> logger,
        IMediathekViewApiClient apiClient,
        IFileDownloader fileDownloader,
        IFileNameBuilderService fileNameBuilder,
        ISubscriptionProcessor subscriptionProcessor)
    {
        _logger = logger;
        _apiClient = apiClient;
        _fileDownloader = fileDownloader;
        _fileNameBuilder = fileNameBuilder;
        _subscriptionProcessor = subscriptionProcessor;
    }

    /// <summary>
    /// Gets the plugin configuration.
    /// </summary>
    protected virtual PluginConfiguration? Configuration => Plugin.Instance?.Configuration;

    /// <summary>
    /// Tests a subscription to see what items would be downloaded.
    /// </summary>
    /// <param name="subscription">The subscription configuration to test.</param>
    /// <returns>A list of items that would be downloaded.</returns>
    [HttpPost("TestSubscription")]
    [Authorize(Policy = Policies.RequiresElevation)]
    public async Task<ActionResult<List<ResultItem>>> TestSubscription([FromBody] Subscription subscription)
    {
        if (subscription == null)
        {
            return BadRequest("Subscription configuration is required.");
        }

        _logger.LogInformation("Testing subscription '{Name}' with {QueryCount} queries.", subscription.Name, subscription.Queries.Count);

        var results = new List<ResultItem>();
        await foreach (var item in _subscriptionProcessor.TestSubscriptionAsync(subscription, CancellationToken.None).ConfigureAwait(false))
        {
            results.Add(item);
        }

        return Ok(results);
    }

    /// <summary>
    /// Searches for media.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="minDuration">Optional minimum duration in seconds.</param>
    /// <param name="maxDuration">Optional maximum duration in seconds.</param>
    /// <returns>A list of search results.</returns>
    [HttpGet("Search")]
    [Authorize(Policy = Policies.RequiresElevation)]
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
    [Authorize(Policy = Policies.RequiresElevation)]
    public IActionResult Download([FromBody] ResultItem item)
    {
        var config = Configuration;
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

        if (FileDownloader.GetDiskSpace(config.DefaultDownloadPath) < config.MinFreeDiskSpaceBytes)
        {
            _logger.LogError("Not enough free disk space to start download for item: {Title}", item.Title);
            return BadRequest("Not enough free disk space to start download.");
        }

        _logger.LogInformation("Manual download requested for item: {Title}", item.Title);

        // Fire and forget
        Task.Run(async () =>
        {
            var sanitizedTitle = _fileNameBuilder.SanitizeFileName(item.Title);
            var manualDownloadFolder = Path.Combine(config.DefaultDownloadPath, "manual");
            Directory.CreateDirectory(manualDownloadFolder);

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

    /// <summary>
    /// Triggers an advanced download for a single item with custom options.
    /// </summary>
    /// <param name="options">The advanced download options.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("AdvancedDownload")]
    [Authorize(Policy = Policies.RequiresElevation)]
    public IActionResult AdvancedDownload([FromBody] AdvancedDownloadOptions options)
    {
        var config = Configuration;
        if (config == null)
        {
            _logger.LogError("Plugin configuration is not available. Cannot start advanced download.");
            return StatusCode(500, "Plugin configuration is not available.");
        }

        var item = options.Item;
        var videoUrl = item.UrlVideoHd ?? item.UrlVideo ?? item.UrlVideoLow;

        if (item == null || string.IsNullOrWhiteSpace(videoUrl))
        {
            return BadRequest("Invalid item provided for download (no video URL).");
        }

        var targetDownloadPath = string.IsNullOrWhiteSpace(options.DownloadPath)
            ? config.DefaultDownloadPath
            : options.DownloadPath;

        if (string.IsNullOrWhiteSpace(targetDownloadPath))
        {
            _logger.LogError("Download path is not configured. Cannot start advanced download.");
            return BadRequest("Download path is not configured.");
        }

        Directory.CreateDirectory(targetDownloadPath);

        if (FileDownloader.GetDiskSpace(targetDownloadPath) < config.MinFreeDiskSpaceBytes)
        {
            _logger.LogError("Not enough free disk space to start advanced download for item: {Title}", item.Title);
            return BadRequest("Not enough free disk space to start download.");
        }

        _logger.LogInformation("Advanced download requested for item: {Title} to path: {Path} with filename: {FileName}", item.Title, targetDownloadPath, options.FileName);

        // Fire and forget
        Task.Run(async () =>
        {
            // Sanitize filename provided by the user, or use a default one.
            var fileName = string.IsNullOrWhiteSpace(options.FileName)
                ? _fileNameBuilder.SanitizeFileName(item.Title) + ".mp4"
                : _fileNameBuilder.SanitizeFileName(options.FileName);

            var videoDestinationPath = Path.Combine(targetDownloadPath, fileName);
            _logger.LogInformation("Starting advanced video download of '{Title}' to '{Path}'", item.Title, videoDestinationPath);
            var videoSuccess = await _fileDownloader.DownloadFileAsync(videoUrl, videoDestinationPath, null, CancellationToken.None).ConfigureAwait(false);
            if (videoSuccess)
            {
                _logger.LogInformation("Successfully finished advanced video download of '{Title}'.", item.Title);
            }
            else
            {
                _logger.LogError("Failed to advanced download video for '{Title}'.", item.Title);
            }

            // Download Subtitle
            if (options.DownloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
            {
                var subtitleFileName = Path.GetFileNameWithoutExtension(fileName) + ".ttml";
                var subtitleDestinationPath = Path.Combine(targetDownloadPath, subtitleFileName);
                _logger.LogInformation("Starting advanced subtitle download of '{Title}' to '{Path}'", item.Title, subtitleDestinationPath);
                var subtitleSuccess = await _fileDownloader.DownloadFileAsync(item.UrlSubtitle, subtitleDestinationPath, null, CancellationToken.None).ConfigureAwait(false);
                if (subtitleSuccess)
                {
                    _logger.LogInformation("Successfully finished advanced subtitle download of '{Title}'.", item.Title);
                }
                else
                {
                    _logger.LogError("Failed to advanced download subtitle for '{Title}'.", item.Title);
                }
            }
        });

        return Ok($"Advanced download for '{item.Title}' started in the background.");
    }

    /// <summary>
    /// Resets the list of processed item IDs for a specific subscription.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to reset.</param>
    /// <returns>An OK result if successful, or BadRequest/NotFound if an error occurs.</returns>
    [HttpPost("ResetProcessedItems")]
    [Authorize(Policy = Policies.RequiresElevation)]
    public ActionResult ResetProcessedItems([FromQuery] Guid subscriptionId)
    {
        var config = Configuration;
        if (config == null)
        {
            _logger.LogError("Plugin configuration is not available. Cannot reset processed items.");
            return StatusCode(500, "Plugin configuration is not available.");
        }

        var subscription = config.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription with ID '{SubscriptionId}' not found. Cannot reset processed items.", subscriptionId);
            return NotFound($"Subscription with ID '{subscriptionId}' not found.");
        }

        subscription.ProcessedItemIds.Clear();
        subscription.LastDownloadedTimestamp = null; // Also reset the timestamp for consistency
        Plugin.Instance?.UpdateConfiguration(config);

        _logger.LogInformation("Processed items list reset for subscription '{SubscriptionName}' (ID: {SubscriptionId}).", subscription.Name, subscriptionId);
        return Ok($"Processed items list reset for subscription '{subscription.Name}'.");
    }
}
