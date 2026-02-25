using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Represents a file found during a local directory scan.
/// </summary>
public record ScannedFile
{
    /// <summary>
    /// Gets the full path to the file.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets the type of the file.
    /// </summary>
    public required FileType Type { get; init; }

    /// <summary>
    /// Gets the parsed video information, if applicable.
    /// </summary>
    public VideoInfo? VideoInfo { get; init; }
}
