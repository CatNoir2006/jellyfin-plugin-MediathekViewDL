using System;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Contains information about a local media file.
/// </summary>
public record struct LocalMediaInfo
{
    /// <summary>
    /// Gets the path to the file on disk.
    /// </summary>
    public string FilePath { get; init; }

    /// <summary>
    /// Gets the width of the video in pixels.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Gets the height of the video in pixels.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Gets the duration of the media.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    public long? FileSize { get; init; }
}
