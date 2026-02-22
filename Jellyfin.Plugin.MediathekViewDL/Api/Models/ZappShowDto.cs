using System;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Data transfer object for Zapp shows.
/// </summary>
public record ZappShowDto
{
    /// <summary>
    /// Gets the title of the show.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the subtitle of the show.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Gets the description of the show.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the start time of the show.
    /// </summary>
    public DateTimeOffset? StartTime { get; init; }

    /// <summary>
    /// Gets the end time of the show.
    /// </summary>
    public DateTimeOffset? EndTime { get; init; }
}
