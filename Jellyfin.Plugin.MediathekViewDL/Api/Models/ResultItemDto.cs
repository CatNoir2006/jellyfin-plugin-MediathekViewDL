using System;
using System.Collections.Generic;
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
}
