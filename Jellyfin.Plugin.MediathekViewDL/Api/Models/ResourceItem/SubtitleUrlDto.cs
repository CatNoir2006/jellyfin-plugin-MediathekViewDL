using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;

/// <summary>
/// Data transfer object for subtitle URLs.
/// </summary>
public record SubtitleUrlDto : ResourceBaseItem
{
    /// <summary>
    /// Gets the type of the subtitle.
    /// </summary>
    public SubtitleType Type { get; init; }
}
