using System;
using System.Diagnostics.CodeAnalysis;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Contains information about a local media file.
/// </summary>
public record LocalMediaInfo
{
    /// <summary>
    /// Gets the path to the file on disk.
    /// </summary>
    public required string FilePath { get; init; }

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

    /// <summary>
    /// Checks if the media info is valid (all properties have values).
    /// </summary>
    /// <returns>Returns true if all properties have values.</returns>
    [MemberNotNullWhen(true, nameof(FileSize), nameof(Duration), nameof(Width), nameof(Height))]
    public bool IsValid() =>
        this is { FileSize: not null, Duration: not null, Width: not null, Height: not null };
}
