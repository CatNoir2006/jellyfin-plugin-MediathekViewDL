using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;
using MediathekView.Api.Models.ResourceItem;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;

/// <summary>
/// Filters and orders available video streams according to the configured download quality settings.
/// </summary>
public interface IVideoQualitySelector
{
    /// <summary>
    /// Filters the streams by the configured height range and orders them according to the configured
    /// quality selection order. The first entry of the returned list is the preferred candidate.
    /// </summary>
    /// <param name="videoUrls">The available video streams.</param>
    /// <param name="settings">The download settings that define range and ordering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered candidate streams, or an empty list if none matches.</returns>
    Task<IReadOnlyList<VideoUrlDto>> GetOrderedCandidatesAsync(IReadOnlyList<VideoUrlDto> videoUrls, BaseDownloadSettings settings, CancellationToken cancellationToken);
}
