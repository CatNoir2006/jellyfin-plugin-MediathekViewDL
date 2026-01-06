namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// Represents the recommended path for a video.
/// </summary>
public record RecommendedPath
{
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the path of the file.
    /// </summary>
    public required string Path { get; init; }
}
