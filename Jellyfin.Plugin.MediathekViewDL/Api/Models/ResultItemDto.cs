using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Data transfer object for result items.
/// </summary>
public record ResultItemDto
{
    /// <summary>
    /// Gets the ID of the item.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the title of the item.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the topic of the item.
    /// </summary>
    public required string Topic { get; init; }

    /// <summary>
    /// Gets the channel of the item.
    /// </summary>
    public required string Channel { get; init; }

    /// <summary>
    /// Gets the description of the item.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the timestamp of the item.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the duration of the item.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the size of the item.
    /// </summary>
    public long? Size { get; init; }

    /// <summary>
    /// Gets the list of video URLs.
    /// </summary>
    public required IReadOnlyList<VideoUrlDto> VideoUrls { get; init; }

    /// <summary>
    /// Gets the list of subtitle URLs.
    /// </summary>
    public required IReadOnlyList<SubtitleUrlDto> SubtitleUrls { get; init; }

    /// <summary>
    /// Gets the list of external IDs.
    /// </summary>
    public required IReadOnlyList<ExternalId> ExternalIds { get; init; }

    /// <summary>
    /// Retrieves the preferred subtitle URL based on type priority.
    /// </summary>
    /// <returns>The Subtitle.</returns>
    public SubtitleUrlDto? GetSubtitle()
    {
        int GetSortNumber(SubtitleType type)
        {
            return type switch
            {
                SubtitleType.WEBVTT => 1,
                SubtitleType.TTML => 2,
                _ => 3,
            };
        }

        return SubtitleUrls
            .OrderBy(s => GetSortNumber(s.Type))
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the video URL by specified quality. Defaults to quality 3 (HD).
    /// </summary>
    /// <param name="quality">The Target Quality.</param>
    /// <returns>The VideoURL.</returns>
    public VideoUrlDto? GetVideoByQuality(int quality = 3)
    {
        var video = VideoUrls.FirstOrDefault(v => v.Quality == quality);
        return video ?? VideoUrls.OrderByDescending(v => v.Quality).FirstOrDefault();
    }
}
