using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Represents an API result combined with its parsed video information.
/// </summary>
/// <param name="Item">The original API result item.</param>
/// <param name="VideoInfo">The parsed video information.</param>
public record ApiResultWithInfo(ResultItemDto Item, VideoInfo VideoInfo);
