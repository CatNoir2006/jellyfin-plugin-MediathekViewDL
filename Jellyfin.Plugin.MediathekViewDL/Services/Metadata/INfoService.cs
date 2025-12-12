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
    /// <param name="item">The NFO item to create.</param>
    void CreateNfo(NfoDTO item);
}
