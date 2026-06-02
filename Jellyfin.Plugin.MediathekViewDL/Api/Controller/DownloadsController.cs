using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadsController"/> class.
    /// </summary>
    /// <param name="downloadQueueManager">The download queue manager.</param>
    /// <param name="downloadHistoryRepository">The download history repository.</param>
    public DownloadsController(
        IDownloadQueueManager downloadQueueManager,
        IDownloadHistoryRepository downloadHistoryRepository)
    {
        _downloadQueueManager = downloadQueueManager;
        _downloadHistoryRepository = downloadHistoryRepository;
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
}
