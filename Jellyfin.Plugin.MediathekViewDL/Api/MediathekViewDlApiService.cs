using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
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
[Authorize(Policy = Policies.RequiresElevation)]
public class MediathekViewDlApiService : ControllerBase
{
    private readonly IMediathekViewApiClient _apiClient;
    private readonly ILogger<MediathekViewDlApiService> _logger;
    private readonly IFileDownloader _fileDownloader;
    private readonly IFileNameBuilderService _fileNameBuilder;
    private readonly ISubscriptionProcessor _subscriptionProcessor;
    private readonly IDownloadHistoryRepository _downloadHistoryRepository;
    private readonly IDownloadQueueManager _downloadQueueManager;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IVideoParser _videoParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewDlApiService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiClient">The api client.</param>
    /// <param name="fileDownloader">The file downloader.</param>
    /// <param name="fileNameBuilder">The file name builder.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    /// <param name="downloadHistoryRepository">The Download History Repo.</param>
    /// <param name="downloadQueueManager">The download queue manager.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="videoParser">The video parser.</param>
    public MediathekViewDlApiService(
        ILogger<MediathekViewDlApiService> logger,
        IMediathekViewApiClient apiClient,
        IFileDownloader fileDownloader,
        IFileNameBuilderService fileNameBuilder,
        ISubscriptionProcessor subscriptionProcessor,
        IDownloadHistoryRepository downloadHistoryRepository,
        IDownloadQueueManager downloadQueueManager,
        IConfigurationProvider configurationProvider,
        IVideoParser videoParser)
    {
        _logger = logger;
        _apiClient = apiClient;
        _fileDownloader = fileDownloader;
        _fileNameBuilder = fileNameBuilder;
        _subscriptionProcessor = subscriptionProcessor;
        _downloadHistoryRepository = downloadHistoryRepository;
        _downloadQueueManager = downloadQueueManager;
        _configurationProvider = configurationProvider;
        _videoParser = videoParser;
    }

    /// <summary>
    /// Gets the currently active downloads.
    /// </summary>
    /// <returns>A list of active downloads.</returns>
    [HttpGet("Downloads/Active")]
    public ActionResult<IEnumerable<ActiveDownload>> GetActiveDownloads()
    {
        return Ok(_downloadQueueManager.GetActiveDownloads());
    }

    /// <summary>
    /// Gets the download history.
    /// </summary>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <returns>A list of download history entries.</returns>
    [HttpGet("Downloads/History")]
    public async Task<ActionResult<IEnumerable<DownloadHistoryEntry>>> GetDownloadHistory([FromQuery] int limit = 50)
    {
        var history = await _downloadHistoryRepository.GetRecentHistoryAsync(limit).ConfigureAwait(false);
        return Ok(history);
    }

    /// <summary>
    /// Cancels a specific download.
    /// </summary>
    /// <param name="id">The active download identifier.</param>
    /// <returns>An OK result.</returns>
    [HttpDelete("Downloads/{id}")]
    [Authorize(Policy = Policies.RequiresElevation)]
    public IActionResult CancelDownload([FromRoute] Guid id)
    {
        try
        {
            _downloadQueueManager.CancelJob(id);
            return Ok($"Download '{id}' cancellation requested.");
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Download with ID '{id}' not found.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Tests a subscription to see what items would be downloaded.
    /// </summary>
    /// <param name="subscription">The subscription configuration to test.</param>
    /// <returns>A list of items that would be downloaded.</returns>
    [HttpPost("TestSubscription")]
    public async Task<ActionResult<List<ResultItem>>> TestSubscription([FromBody] Subscription? subscription)
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
    public async Task<ActionResult<List<ResultItem>>> Search(
        [FromQuery] string query,
        [FromQuery] int? minDuration,
        [FromQuery] int? maxDuration)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query cannot be empty.");
        }

        try
        {
            var results = await _apiClient.SearchAsync(query, minDuration, maxDuration, CancellationToken.None).ConfigureAwait(false);
            return Ok(results);
        }
        catch (MediathekConnectionException ex)
        {
            _logger.LogError(ex, "Connection error while searching.");
            return StatusCode(503, "The MediathekView API is currently unreachable. Please try again later.");
        }
        catch (MediathekParsingException ex)
        {
            _logger.LogError(ex, "Parsing error while searching.");
            return StatusCode(502, "Received an invalid response from the MediathekView API.");
        }
        catch (MediathekApiException ex)
        {
            _logger.LogError(ex, "API error while searching. Status code: {StatusCode}", ex.StatusCode);
            var statusCode = (int)ex.StatusCode >= 500 ? 502 : 500;
            return StatusCode(statusCode, $"The MediathekView API returned an error ({ex.StatusCode}).");
        }
        catch (MediathekException ex)
        {
            _logger.LogError(ex, "An error occurred while searching.");
            return StatusCode(500, "An unexpected error occurred while calling the MediathekView API.");
        }
    }

    /// <summary>
    /// Parses a search item into video information.
    /// </summary>
    /// <param name="item">The search result item to parse.</param>
    /// <returns>The parsed video info.</returns>
    [HttpPost("Items/Parse")]
    public ActionResult<VideoInfo> ParseSearchItem([FromBody] ResultItem item)
    {
        try
        {
            var parsed = _videoParser.ParseVideoInfo(item.Topic, item.Title);
            if (parsed == null)
            {
                _logger.LogError("Could not parse the Item: {Item}", item);
                return BadRequest("Could not parse the Item");
            }

            return Ok(parsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse the Item: {Item}", item);
            return BadRequest("Could not parse the Item");
        }
    }

    /// <summary>
    /// Gets the recommended download path for a given video info.
    /// </summary>
    /// <param name="videoInfo">The video info to generate a path for.</param>
    /// <returns>The recommended path.</returns>
    [HttpPost("Items/RecommendedPath")]
    public ActionResult<RecommendedPath> GetRecommendedPath([FromBody] VideoInfo videoInfo)
    {
        try
        {
            var defaultSub = new Subscription() { Name = videoInfo.Topic };
            var dlPaths = _fileNameBuilder.GenerateDownloadPaths(videoInfo, defaultSub, DownloadContext.Manual, FileType.Video);
            var genPaths = new RecommendedPath()
            {
                FileName = Path.GetFileName(dlPaths.MainFilePath),
                SubtitleName = Path.GetFileName(dlPaths.SubtitleFilePath),
                Path = Path.GetDirectoryName(dlPaths.MainFilePath)!,
            };

            return Ok(genPaths);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create RecommendedPaths for: {VideoInfo}", videoInfo);
            return BadRequest("Could not create RecommendedPaths");
        }
    }

    /// <summary>
    /// Triggers a download for a single item.
    /// </summary>
    /// <param name="item">The item to download.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("Download")]
    public IActionResult Download([FromBody] ResultItem item)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            _logger.LogError("Plugin configuration is not available. Cannot start manual download.");
            return StatusCode(500, "Plugin configuration is not available.");
        }

        var videoUrl = item.UrlVideoHd ?? item.UrlVideo ?? item.UrlVideoLow;

        if (item == null || string.IsNullOrWhiteSpace(videoUrl))
        {
            return BadRequest("Invalid item provided for download (no video URL).");
        }

        var videoInfo = _videoParser.ParseVideoInfo(item.Topic, item.Title);
        if (videoInfo == null)
        {
            _logger.LogError("Could not parse video info for item: {Title}", item.Title);
            return BadRequest("Could not parse video info.");
        }

        var defaultSub = new Subscription() { Name = item.Topic };
        var paths = _fileNameBuilder.GenerateDownloadPaths(videoInfo, defaultSub, DownloadContext.Manual, FileType.Video);

        if (!paths.IsValid)
        {
            _logger.LogError("Could not generate download paths for item: {Title}", item.Title);
            return BadRequest("Could not generate download paths.");
        }

        if (FileDownloader.GetDiskSpace(paths.DirectoryPath) < config.MinFreeDiskSpaceBytes)
        {
            _logger.LogError("Not enough free disk space to start download for item: {Title} at {Path}", item.Title, paths.DirectoryPath);
            return BadRequest("Not enough free disk space to start download.");
        }

        _logger.LogInformation("Manual download requested for item: {Title}", item.Title);

        var job = new DownloadJob { ItemId = item.Id, Title = item.Title, ItemInfo = videoInfo };

        job.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = videoUrl.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ? DownloadType.M3U8Download : DownloadType.DirectDownload });

        if (config.DownloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
        {
            job.DownloadItems.Add(new DownloadItem { SourceUrl = item.UrlSubtitle, DestinationPath = paths.SubtitleFilePath, JobType = DownloadType.DirectDownload });
        }

        _downloadQueueManager.QueueJob(job);
        return Ok($"Download for '{item.Title}' queued.");
    }

    /// <summary>
    /// Triggers an advanced download for a single item with custom options.
    /// </summary>
    /// <param name="options">The advanced download options.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("AdvancedDownload")]
    public IActionResult AdvancedDownload([FromBody] AdvancedDownloadOptions options)
    {
        var config = _configurationProvider.ConfigurationOrNull;
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

        if (string.IsNullOrWhiteSpace(options.DownloadPath) || string.IsNullOrWhiteSpace(options.FileName))
        {
            return BadRequest("DownloadPath and FileName are required for advanced download.");
        }

        // Validate using project-specific sanitization logic
        if (_fileNameBuilder.SanitizeFileName(options.FileName) != options.FileName)
        {
            return BadRequest("FileName contains invalid characters.");
        }

        var videoInfo = _videoParser.ParseVideoInfo(item.Topic, item.Title);
        if (videoInfo == null)
        {
            _logger.LogError("Could not parse video info for item: {Title}", item.Title);
            return BadRequest("Could not parse video info.");
        }

#pragma warning disable CA3003 // Path is validated via manual check and directory creation rules
        if (!Directory.Exists(options.DownloadPath))
        {
            try
            {
                Directory.CreateDirectory(options.DownloadPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create directory: {Path}", options.DownloadPath);
                return BadRequest("Could not create target directory.");
            }
        }

        if (FileDownloader.GetDiskSpace(options.DownloadPath) < config.MinFreeDiskSpaceBytes)
#pragma warning restore CA3003
        {
            _logger.LogError("Not enough free disk space to start advanced download for item: {Title} at {Path}", item.Title, options.DownloadPath);
            return BadRequest("Not enough free disk space to start download.");
        }

        _logger.LogInformation("Advanced download requested for item: {Title} to path: {Path} with filename: {FileName}", item.Title, options.DownloadPath, options.FileName);

        var videoDestinationPath = Path.Combine(options.DownloadPath, _fileNameBuilder.SanitizeFileName(options.FileName));
        var job = new DownloadJob { ItemId = item.Id, Title = item.Title, ItemInfo = videoInfo };

        job.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = videoDestinationPath, JobType = videoUrl.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ? DownloadType.M3U8Download : DownloadType.DirectDownload });

        if (options.DownloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
        {
            string subtitleFileName;
            if (!string.IsNullOrWhiteSpace(options.SubtitleName))
            {
                subtitleFileName = _fileNameBuilder.SanitizeFileName(options.SubtitleName);
            }
            else
            {
                var defaultSub = new Subscription() { Name = item.Topic };
                var genPaths = _fileNameBuilder.GenerateDownloadPaths(videoInfo, defaultSub, DownloadContext.Manual, FileType.Video);
                subtitleFileName = Path.GetFileName(genPaths.SubtitleFilePath);
            }

            var subtitleDestinationPath = Path.Combine(options.DownloadPath, subtitleFileName);
            job.DownloadItems.Add(new DownloadItem { SourceUrl = item.UrlSubtitle, DestinationPath = subtitleDestinationPath, JobType = DownloadType.DirectDownload });
        }

        _downloadQueueManager.QueueJob(job);
        return Ok($"Advanced download for '{item.Title}' queued.");
    }

    /// <summary>
    /// Resets the list of processed item IDs for a specific subscription.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to reset.</param>
    /// <returns>An OK result if successful, or BadRequest/NotFound if an error occurs.</returns>
    [HttpPost("ResetProcessedItems")]
    public async Task<ActionResult> ResetProcessedItems([FromQuery] Guid subscriptionId)
    {
        var config = _configurationProvider.ConfigurationOrNull;
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

        await _downloadHistoryRepository.RemoveBySubscriptionIdAsync(subscriptionId).ConfigureAwait(false);
        subscription.LastDownloadedTimestamp = null; // Also reset the timestamp for consistency
        _configurationProvider.TryUpdate(config);

        _logger.LogInformation("Processed items list reset for subscription '{SubscriptionName}' (ID: {SubscriptionId}).", subscription.Name, subscriptionId);
        return Ok($"Processed items list reset for subscription '{subscription.Name}'.");
    }
}
