using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Configuration;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

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
    /// Tests a subscription query and filters without creating download jobs.
    /// </summary>
    /// <param name="subscription">The subscription to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of items that would be downloaded.</returns>
    IAsyncEnumerable<ResultItem> TestSubscriptionAsync(
        Subscription subscription,
        CancellationToken cancellationToken);
}
