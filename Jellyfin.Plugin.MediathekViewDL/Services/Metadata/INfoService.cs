using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

/// <summary>
/// Service for generating NFO metadata files.
/// </summary>
public interface INfoService
{
    /// <summary>
    /// Creates an NFO file for the specified video.
    /// </summary>
    /// <param name="item">The MediathekView result item containing metadata.</param>
    /// <param name="videoInfo">The parsed video info (optional).</param>
    /// <param name="videoFilePath">The full path to the video file.</param>
    void CreateNfo(ResultItem item, VideoInfo? videoInfo, string videoFilePath);
}
