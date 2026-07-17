using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Controller;

/// <summary>
/// The controller for managing downloads.
/// </summary>
[ApiController]
[Route("MediathekViewDL/[controller]")]
[Authorize(Policy = Policies.RequiresElevation)]
public class DownloadsController : ControllerBase
{
    private readonly IDownloadQueueManager _downloadQueueManager;
    private readonly IDownloadHistoryRepository _downloadHistoryRepository;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IVideoParser _videoParser;
    private readonly IFileNameBuilderService _fileNameBuilder;
    private readonly ILogger<DownloadsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadsController"/> class.
    /// </summary>
    /// <param name="downloadQueueManager">The download queue manager.</param>
    /// <param name="downloadHistoryRepository">The download history repository.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="fileNameBuilder">The file name builder service.</param>
    /// <param name="logger">The logger.</param>
    public DownloadsController(
        IDownloadQueueManager downloadQueueManager,
        IDownloadHistoryRepository downloadHistoryRepository,
        IConfigurationProvider configurationProvider,
        IVideoParser videoParser,
        IFileNameBuilderService fileNameBuilder,
        ILogger<DownloadsController> logger)
    {
        _downloadQueueManager = downloadQueueManager;
        _downloadHistoryRepository = downloadHistoryRepository;
        _configurationProvider = configurationProvider;
        _videoParser = videoParser;
        _fileNameBuilder = fileNameBuilder;
        _logger = logger;
    }

    /// <summary>
    /// Gets the currently active downloads.
    /// </summary>
    /// <returns>A list of active downloads.</returns>
    [HttpGet("Active")]
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
    [HttpGet("History")]
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
    /// Gets the grouped download history.
    /// </summary>
    /// <param name="limit">The maximum number of raw entries to fetch before grouping.</param>
    /// <returns>A list of grouped download history entries.</returns>
    [HttpGet("History/Grouped")]
    public async Task<ActionResult<IEnumerable<GroupedDownloadHistoryDto>>> GetGroupedDownloadHistory([FromQuery] int limit = 100)
    {
        if (Plugin.Instance?.InitializationException is not null)
        {
            return StatusCode(503, new ApiErrorDto(ApiErrorId.InitializationError, Plugin.Instance.InitializationException.Message));
        }

        var history = await _downloadHistoryRepository.GetRecentHistoryAsync(limit).ConfigureAwait(false);
        var groups = new List<GroupedDownloadHistoryDto>();

        foreach (var entry in history)
        {
            var entrySubId = entry.SubscriptionId;
            var entryItemId = entry.ItemId;
            var entryTitle = entry.Title;
            var entryFileName = !string.IsNullOrEmpty(entry.DownloadPath) ? System.IO.Path.GetFileName(entry.DownloadPath) : string.Empty;
            var entryDisplayName = !string.IsNullOrWhiteSpace(entryTitle) ? entryTitle : entryFileName;

            var group = groups.Find(g =>
            {
                if (g.SubscriptionId != entrySubId)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(entryItemId) && !string.IsNullOrEmpty(g.ItemId) && entryItemId == g.ItemId)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(entryTitle) && !string.IsNullOrEmpty(g.Title) && entryTitle == g.Title)
                {
                    return true;
                }

                return !string.IsNullOrEmpty(entryDisplayName) && !string.IsNullOrEmpty(g.DisplayName) && entryDisplayName == g.DisplayName;
            });

            if (group == null)
            {
                group = new GroupedDownloadHistoryDto
                {
                    SubscriptionId = entrySubId,
                    Title = entryTitle,
                    DisplayName = entryDisplayName,
                    ItemId = entryItemId,
                    LatestTimestamp = entry.Timestamp
                };
                groups.Add(group);
            }

            group.Entries.Add(entry);

            if (!string.IsNullOrEmpty(entryDisplayName) && (string.IsNullOrEmpty(group.DisplayName) || entryDisplayName.Length < group.DisplayName.Length))
            {
                group.DisplayName = entryDisplayName;
            }

            if (entry.Timestamp > group.LatestTimestamp)
            {
                group.LatestTimestamp = entry.Timestamp;
            }
        }

        return Ok(groups.OrderByDescending(g => g.LatestTimestamp));
    }

    /// <summary>
    /// Cancels a specific download.
    /// </summary>
    /// <param name="id">The active download identifier.</param>
    /// <returns>An OK result.</returns>
    [HttpDelete("{id}")]
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
    /// Cancels all active downloads.
    /// </summary>
    /// <returns>An OK result.</returns>
    [HttpDelete]
    public IActionResult CancelAllDownloads()
    {
        _downloadQueueManager.CancelAllJobs();
        return Ok("Abbruch aller Downloads angefordert.");
    }

    /// <summary>
    /// Clears all finished, failed or cancelled downloads from the active list.
    /// </summary>
    /// <returns>An OK result.</returns>
    [HttpPost("ClearInactive")]
    public IActionResult ClearInactiveDownloads()
    {
        _downloadQueueManager.ClearInactiveJobs();
        return Ok("Inaktive Downloads aus der Liste entfernt.");
    }

    /// <summary>
    /// Triggers a download for a single item.
    /// </summary>
    /// <param name="item">The item to download.</param>
    /// <returns>An OK result.</returns>
    [HttpPost]
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

        var subtitle = item.GetSubtitle();
        var subtitleUrl = (config.Download.DownloadSubtitles && !string.IsNullOrWhiteSpace(subtitle?.Url)) ? subtitle!.Url : null;

        var job = new DownloadJob
        {
            ItemId = item.Id,
            Title = item.Title,
            ItemInfo = videoInfo,
            MediaMetadata = MediaMetadataFactory.Create(item, videoUrl, subtitleUrl),
        };

        job.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.FFmpegDownload });

        if (subtitleUrl is not null)
        {
            job.DownloadItems.Add(new DownloadItem { SourceUrl = subtitleUrl, DestinationPath = paths.SubtitleFilePath, JobType = DownloadType.SubtitleDownload });
        }

        _downloadQueueManager.QueueJob(job);
        return Ok($"Download für '{item.Title}' in Warteschlange.");
    }

    /// <summary>
    /// Triggers an advanced download for a single item with custom options.
    /// </summary>
    /// <param name="options">The advanced download options.</param>
    /// <returns>An OK result.</returns>
    [HttpPost("Advanced")]
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

        if (!_fileNameBuilder.IsPathSafe(options.DownloadPath))
        {
            _logger.LogWarning("Blocked advanced download request to unsafe path: {Path}", options.DownloadPath);
            return BadRequest(new ApiErrorDto(ApiErrorId.UnsafePath, "Der angegebene Download-Pfad ist nicht zulässig. Bitte verwenden Sie einen Pfad innerhalb Ihrer Bibliothek oder der konfigurierten Download-Verzeichnisse."));
        }

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
        var subtitle = item.GetSubtitle();
        var subtitleUrl = (options.DownloadSubtitles && !string.IsNullOrWhiteSpace(subtitle?.Url)) ? subtitle!.Url : null;

        var job = new DownloadJob
        {
            ItemId = item.Id,
            Title = item.Title,
            ItemInfo = videoInfo,
            MediaMetadata = MediaMetadataFactory.Create(item, videoUrl, subtitleUrl),
        };

        job.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = videoDestinationPath, JobType = DownloadType.FFmpegDownload });

        if (subtitleUrl is not null)
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
            job.DownloadItems.Add(new DownloadItem { SourceUrl = subtitleUrl, DestinationPath = subtitleDestinationPath, JobType = DownloadType.SubtitleDownload });
        }

        _downloadQueueManager.QueueJob(job);
        return Ok($"Advanced download for '{item.Title}' queued.");
    }
}
