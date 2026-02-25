using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;
using Jellyfin.Plugin.MediathekViewDL.Services.Adoption;
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
    private readonly IFileAdoptionService _fileAdoptionService;

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
    /// <param name="fileAdoptionService">The file adoption service.</param>
    public MediathekViewDlApiService(
        ILogger<MediathekViewDlApiService> logger,
        IMediathekViewApiClient apiClient,
        IFileDownloader fileDownloader,
        IFileNameBuilderService fileNameBuilder,
        ISubscriptionProcessor subscriptionProcessor,
        IDownloadHistoryRepository downloadHistoryRepository,
        IDownloadQueueManager downloadQueueManager,
        IConfigurationProvider configurationProvider,
        IVideoParser videoParser,
        IFileAdoptionService fileAdoptionService)
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
        _fileAdoptionService = fileAdoptionService;
    }

    /// <summary>
    /// Gets the initialization error, if any.
    /// </summary>
    /// <returns>The error message or null.</returns>
    [HttpGet("InitializationError")]
    public ActionResult<string?> GetInitializationError()
    {
        if (Plugin.Instance?.InitializationException is null)
        {
            return Ok(null);
        }

        string msg = Plugin.Instance.InitializationException.Message;
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = "Ein unbekannter Fehler während der Initialisierung ist aufgetreten.";
        }

        return Ok(msg);
    }

    /// <summary>
    /// Gets the currently active downloads.
    /// </summary>
    /// <returns>A list of active downloads.</returns>
    [HttpGet("Downloads/Active")]
    public ActionResult<IEnumerable<ActiveDownload>> GetActiveDownloads()
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

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
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

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
            return Ok($"Download '{id}' Abbruch angefordert.");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiErrorDto(ApiErrorId.NotFound, $"Download mit ID '{id}' wurde nicht gefunden."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidOperation, ex.Message));
        }
    }

    /// <summary>
    /// Tests a subscription to see what items would be downloaded.
    /// </summary>
    /// <param name="subscription">The subscription configuration to test.</param>
    /// <returns>A list of items that would be downloaded.</returns>
    [HttpPost("TestSubscription")]
    public async Task<ActionResult<List<ResultItemDto>>> TestSubscription([FromBody] Subscription? subscription)
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

        if (subscription == null)
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidSubscription, "Abonnement-Konfiguration ist erforderlich."));
        }

        _logger.LogInformation("Testing subscription '{Name}' with {QueryCount} queries.", subscription.Name, subscription.Search.Criteria.Count);

        var results = new List<ResultItemDto>();
        await foreach (var item in _subscriptionProcessor.TestSubscriptionAsync(subscription, CancellationToken.None).ConfigureAwait(false))
        {
            results.Add(item);
        }

        return Ok(results);
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
    /// <returns>A list of search results.</returns>
    [HttpGet("Search")]
    public async Task<ActionResult<IEnumerable<ResultItemDto>>> Search(
        [FromQuery] string? title,
        [FromQuery] string? topic,
        [FromQuery] string? channel,
        [FromQuery] string? combinedSearch,
        [FromQuery] int? minDuration,
        [FromQuery] int? maxDuration,
        [FromQuery] DateTimeOffset? minBroadcastDate,
        [FromQuery] DateTimeOffset? maxBroadcastDate)
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

        if (string.IsNullOrWhiteSpace(title) &&
            string.IsNullOrWhiteSpace(topic) &&
            string.IsNullOrWhiteSpace(channel) &&
            string.IsNullOrWhiteSpace(combinedSearch))
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidSearch, "Mindestens ein Suchparameter (Titel, Thema, Sender oder kombinierte Suche) muss angegeben werden."));
        }

        try
        {
            var results = await _apiClient.SearchAsync(title, topic, channel, combinedSearch, minDuration, maxDuration, minBroadcastDate, maxBroadcastDate, CancellationToken.None).ConfigureAwait(false);
            return Ok(results);
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
    /// Parses a search item into video information.
    /// </summary>
    /// <param name="item">The search result item to parse.</param>
    /// <returns>The parsed video info.</returns>
    [HttpPost("Items/Parse")]
    public ActionResult<VideoInfo> ParseSearchItem([FromBody] ResultItemDto item)
    {
        try
        {
            var parsed = _videoParser.ParseVideoInfo(item.Topic, item.Title);
            if (parsed == null)
            {
                _logger.LogError("Could not parse the Item: {Item}", item);
                return BadRequest(new ApiErrorDto(ApiErrorId.ParseError, "Das Element konnte nicht analysiert werden."));
            }

            return Ok(parsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not parse the Item: {Item}", item);
            return BadRequest(new ApiErrorDto(ApiErrorId.ParseError, "Das Element konnte nicht analysiert werden."));
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
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidPath, "Empfohlene Pfade konnten nicht erstellt werden."));
        }
    }

    /// <summary>
    /// Triggers a download for a single item.
    /// </summary>
    /// <param name="item">The item to download.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("Download")]
    public IActionResult Download([FromBody] ResultItemDto? item)
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            _logger.LogError("Plugin configuration is not available. Cannot start manual download.");
            return StatusCode(500, new ApiErrorDto(ApiErrorId.ConfigurationNotAvailable, "Plugin-Konfiguration ist nicht verfügbar."));
        }

        var videoUrl = item?.GetVideoByQuality()?.Url;

        if (item == null || string.IsNullOrWhiteSpace(videoUrl))
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidItem, "Ungültiges Element für den Download bereitgestellt (keine Video-URL)."));
        }

        var videoInfo = _videoParser.ParseVideoInfo(item.Topic, item.Title);
        if (videoInfo == null)
        {
            _logger.LogError("Could not parse video info for item: {Title}", item.Title);
            return BadRequest(new ApiErrorDto(ApiErrorId.ParseError, "Video-Informationen konnten nicht analysiert werden."));
        }

        var defaultSub = new Subscription() { Name = item.Topic };
        var paths = _fileNameBuilder.GenerateDownloadPaths(videoInfo, defaultSub, DownloadContext.Manual, FileType.Video);

        if (!paths.IsValid)
        {
            _logger.LogError("Could not generate download paths for item: {Title}", item.Title);
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidPath, "Download-Pfade konnten nicht generiert werden."));
        }

        if (FileDownloader.GetDiskSpace(paths.DirectoryPath) < config.Download.MinFreeDiskSpaceBytes)
        {
            _logger.LogError("Not enough free disk space to start download for item: {Title} at {Path}", item.Title, paths.DirectoryPath);
            return BadRequest(new ApiErrorDto(ApiErrorId.InsufficientDiskSpace, "Nicht genügend freier Speicherplatz, um den Download zu starten."));
        }

        _logger.LogInformation("Manual download requested for item: {Title}", item.Title);

        var job = new DownloadJob { ItemId = item.Id, Title = item.Title, ItemInfo = videoInfo };

        job.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = videoUrl.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ? DownloadType.M3U8Download : DownloadType.DirectDownload });

        var subtitle = item.GetSubtitle();
        if (config.Download.DownloadSubtitles && !string.IsNullOrWhiteSpace(subtitle?.Url))
        {
            job.DownloadItems.Add(new DownloadItem { SourceUrl = subtitle.Url, DestinationPath = paths.SubtitleFilePath, JobType = DownloadType.DirectDownload });
        }

        _downloadQueueManager.QueueJob(job);
        return Ok($"Download für '{item.Title}' in Warteschlange.");
    }

    /// <summary>
    /// Triggers an advanced download for a single item with custom options.
    /// </summary>
    /// <param name="options">The advanced download options.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("AdvancedDownload")]
    public IActionResult AdvancedDownload([FromBody] AdvancedDownloadOptions? options)
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            _logger.LogError("Plugin configuration is not available. Cannot start advanced download.");
            return StatusCode(500, new ApiErrorDto(ApiErrorId.ConfigurationNotAvailable, "Plugin-Konfiguration ist nicht verfügbar."));
        }

        if (options == null)
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidOptions, "Erweiterte Download-Optionen sind erforderlich."));
        }

        var item = options.Item;
        var videoUrl = item.GetVideoByQuality()?.Url;

        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidItem, "Ungültiges Element für den Download bereitgestellt (keine Video-URL)."));
        }

        if (string.IsNullOrWhiteSpace(options.DownloadPath) || string.IsNullOrWhiteSpace(options.FileName))
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidOptions, "Download-Pfad und Dateiname sind für den erweiterten Download erforderlich."));
        }

        // Security check: Validate path traversal
        if (!_fileNameBuilder.IsPathSafe(options.DownloadPath))
        {
            _logger.LogWarning("Blocked advanced download request to unsafe path: {Path}", options.DownloadPath);
            return BadRequest(new ApiErrorDto(ApiErrorId.UnsafePath, "Der angegebene Download-Pfad ist nicht zulässig. Bitte verwenden Sie einen Pfad innerhalb Ihrer Bibliothek oder der konfigurierten Download-Verzeichnisse."));
        }

        // Validate using project-specific sanitization logic
        if (_fileNameBuilder.SanitizeFileName(options.FileName) != options.FileName)
        {
            return BadRequest(new ApiErrorDto(ApiErrorId.InvalidFilename, "Der Dateiname enthält ungültige Zeichen."));
        }

        var videoInfo = _videoParser.ParseVideoInfo(item.Topic, item.Title);
        if (videoInfo == null)
        {
            _logger.LogError("Could not parse video info for item: {Title}", item.Title);
            return BadRequest(new ApiErrorDto(ApiErrorId.ParseError, "Video-Informationen konnten nicht analysiert werden."));
        }

#pragma warning disable CA3003 // Path is validated via manual check and directory creation rules
        if (FileDownloader.GetDiskSpace(options.DownloadPath) < config.Download.MinFreeDiskSpaceBytes)
#pragma warning restore CA3003
        {
            _logger.LogError("Not enough free disk space to start advanced download for item: {Title} at {Path}", item.Title, options.DownloadPath);
            return BadRequest(new ApiErrorDto(ApiErrorId.InsufficientDiskSpace, "Nicht genügend freier Speicherplatz, um den Download zu starten."));
        }

        _logger.LogInformation("Advanced download requested for item: {Title} to path: {Path} with filename: {FileName}", item.Title, options.DownloadPath, options.FileName);

        var videoDestinationPath = Path.Combine(options.DownloadPath, _fileNameBuilder.SanitizeFileName(options.FileName));
        var job = new DownloadJob { ItemId = item.Id, Title = item.Title, ItemInfo = videoInfo };

        job.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = videoDestinationPath, JobType = videoUrl.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ? DownloadType.M3U8Download : DownloadType.DirectDownload });

        var subtitle = item.GetSubtitle();
        if (options.DownloadSubtitles && !string.IsNullOrWhiteSpace(subtitle?.Url))
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
            job.DownloadItems.Add(new DownloadItem { SourceUrl = subtitle.Url, DestinationPath = subtitleDestinationPath, JobType = DownloadType.DirectDownload });
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
        var validationResult = GetSubscriptionOrError(subscriptionId, out var subscription, out var config);
        if (validationResult != null)
        {
            return validationResult;
        }

        await _downloadHistoryRepository.RemoveBySubscriptionIdAsync(subscriptionId).ConfigureAwait(false);
        subscription!.LastDownloadedTimestamp = null; // Also reset the timestamp for consistency
        _configurationProvider.TryUpdate(config!);

        _logger.LogInformation("Processed items list reset for subscription '{SubscriptionName}' (ID: {SubscriptionId}).", subscription.Name, subscriptionId);
        return Ok($"Liste der verarbeiteten Elemente für Abonnement '{subscription.Name}' wurde zurückgesetzt.");
    }

    /// <summary>
    /// Gets the adoption candidates for a specific subscription.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription.</param>
    /// <returns>Adoption information containing candidates and API results.</returns>
    [HttpGet("Adoption/Candidates/{subscriptionId}")]
    public async Task<ActionResult<AdoptionInfo>> GetAdoptionCandidates([FromRoute] Guid subscriptionId)
    {
        var validationResult = GetSubscriptionOrError(subscriptionId, out _, out _);
        if (validationResult != null)
        {
            return validationResult;
        }

        var info = await _fileAdoptionService.GetAdoptionCandidatesAsync(subscriptionId, CancellationToken.None).ConfigureAwait(false);
        return Ok(info);
    }

    /// <summary>
    /// Sets an adoption match for a specific local file group.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription.</param>
    /// <param name="mapping">The adoption mapping.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("Adoption/Match")]
    public async Task<ActionResult> SetAdoptionMatch([FromQuery] Guid subscriptionId, [FromBody] FileAdoptionMapping mapping)
    {
        var validationResult = GetSubscriptionOrError(subscriptionId, out _, out _);
        if (validationResult != null)
        {
            return validationResult;
        }

        await _fileAdoptionService.SetApiIdAsync(subscriptionId, mapping.CandidateId, mapping.ApiId, mapping.VideoUrl, CancellationToken.None).ConfigureAwait(false);
        return Ok();
    }

    /// <summary>
    /// Sets multiple adoption mappings for a subscription.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription.</param>
    /// <param name="mappings">The adoption mappings.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("Adoption/Mappings")]
    public async Task<ActionResult> SetAdoptionMappings([FromQuery] Guid subscriptionId, [FromBody] IReadOnlyList<FileAdoptionMapping> mappings)
    {
        var validationResult = GetSubscriptionOrError(subscriptionId, out _, out _);
        if (validationResult != null)
        {
            return validationResult;
        }

        await _fileAdoptionService.SetMappingsAsync(subscriptionId, mappings, CancellationToken.None).ConfigureAwait(false);
        return Ok();
    }

    private ActionResult? GetSubscriptionOrError(Guid subscriptionId, out Subscription? subscription, out PluginConfiguration? config)
    {
        subscription = null;
        config = null;

        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

        config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return StatusCode(500, new ApiErrorDto(ApiErrorId.ConfigurationNotAvailable, "Plugin-Konfiguration ist nicht verfügbar."));
        }

        subscription = config.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        if (subscription == null)
        {
            return NotFound(new ApiErrorDto(ApiErrorId.NotFound, $"Abonnement mit der ID '{subscriptionId}' wurde nicht gefunden."));
        }

        return null;
    }
}
