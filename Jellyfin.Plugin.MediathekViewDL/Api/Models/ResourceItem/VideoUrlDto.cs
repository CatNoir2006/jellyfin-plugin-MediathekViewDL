namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;

/// <summary>
/// Data transfer object for video URLs.
/// </summary>
public record VideoUrlDto : ResourceBaseItem
{
    /// <summary>
    /// Gets the size of the video in bytes.
    /// </summary>
    public long? Size { get; init; }

    /// <summary>
    /// Gets the quality of the video.
    /// </summary>
    public int Quality { get; init; }

    /// <summary>
    /// Gets the codec of the video.
    /// </summary>
    public string? Codec { get; init; }
}
