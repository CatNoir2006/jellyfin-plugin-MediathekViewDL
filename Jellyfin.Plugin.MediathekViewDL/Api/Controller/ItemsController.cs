using System;
using System.IO;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Controller;

/// <summary>
/// The Controller for item parsing and path generation.
/// </summary>
[ApiController]
[Route("MediathekViewDL/[controller]")]
[Authorize(Policy = Policies.RequiresElevation)]
public class ItemsController : ControllerBase
{
    private readonly IVideoParser _videoParser;
    private readonly IFileNameBuilderService _fileNameBuilder;
    private readonly ILogger<ItemsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsController"/> class.
    /// </summary>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="fileNameBuilder">The file name builder service.</param>
    /// <param name="logger">The logger.</param>
    public ItemsController(IVideoParser videoParser, IFileNameBuilderService fileNameBuilder, ILogger<ItemsController> logger)
    {
        _videoParser = videoParser;
        _fileNameBuilder = fileNameBuilder;
        _logger = logger;
    }

    /// <summary>
    /// Parses a search item into video information.
    /// </summary>
    /// <param name="item">The search result item to parse.</param>
    /// <returns>The parsed video info.</returns>
    [HttpPost("Parse")]
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
    [HttpPost("RecommendedPath")]
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
}
