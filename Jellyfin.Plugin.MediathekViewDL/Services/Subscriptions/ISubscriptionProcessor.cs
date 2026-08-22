using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;

/// <summary>
/// Interface for the SubscriptionProcessor service.
/// </summary>
public interface ISubscriptionProcessor
{
    /// <summary>
    /// Processes a subscription to find new download jobs.
    /// </summary>
    /// <param name="subscription">The subscription to process.</param>
    /// <param name="downloadSubtitles">Whether to download subtitles globally.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of download jobs.</returns>
    Task<List<DownloadJob>> GetJobsForSubscriptionAsync(
        Subscription subscription,
        bool downloadSubtitles,
        CancellationToken cancellationToken);

    /// <summary>
    /// Processes a single subscription completely, finding new items, queuing them and updating the subscription status.
    /// </summary>
    /// <param name="subscription">The subscription to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of new items found and queued.</returns>
    Task<int> ProcessSubscriptionAsync(
        Subscription subscription,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets all eligible items for a subscription from the API, applying filters based on subscription settings.
    /// </summary>
    /// <param name="subscription">The subscription to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable of eligible result items and their parsed video info.</returns>
    IAsyncEnumerable<(ResultItemDto Item, VideoInfo VideoInfo)> GetEligibleItemsAsync(
        Subscription subscription,
        CancellationToken cancellationToken);

    /// <summary>
    /// Tests a subscription query and filters without creating download jobs.
    /// </summary>
    /// <param name="subscription">The subscription to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of items that would be downloaded.</returns>
    IAsyncEnumerable<ResultItemDto> TestSubscriptionAsync(
        Subscription subscription,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns all items matching the subscription that should be surfaced in the virtual channel,
    /// ignoring the download history so the channel always reflects the currently available items.
    /// </summary>
    /// <param name="subscription">The subscription.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching items and their parsed video info.</returns>
    IAsyncEnumerable<(ResultItemDto Item, VideoInfo VideoInfo)> GetChannelItemsAsync(
        Subscription subscription,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the best streamable video URL for a single API item, honoring the subscription's
    /// quality and fallback settings. Used by the virtual channel to build playable media sources.
    /// </summary>
    /// <param name="subscription">The subscription the item belongs to.</param>
    /// <param name="item">The API result item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The streamable URL, or <c>null</c> if none could be resolved.</returns>
    Task<string?> GetStreamUrlAsync(
        Subscription subscription,
        ResultItemDto item,
        CancellationToken cancellationToken = default);
}
