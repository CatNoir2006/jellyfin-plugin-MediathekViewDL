using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service responsible for searching and filtering content for subscriptions.
/// </summary>
public class SubscriptionProcessor
{
    private readonly ILogger<SubscriptionProcessor> _logger;
    private readonly MediathekViewApiClient _apiClient;
    private readonly VideoParser _videoParser;
    private readonly LocalMediaScanner _localMediaScanner;
    private readonly FileNameBuilderService _fileNameBuilderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionProcessor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiClient">The API client.</param>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="localMediaScanner">The local media scanner.</param>
    /// <param name="fileNameBuilderService">The file name builder service.</param>
    public SubscriptionProcessor(
        ILogger<SubscriptionProcessor> logger,
        MediathekViewApiClient apiClient,
        VideoParser videoParser,
        LocalMediaScanner localMediaScanner,
        FileNameBuilderService fileNameBuilderService)
    {
        _logger = logger;
        _apiClient = apiClient;
        _videoParser = videoParser;
        _localMediaScanner = localMediaScanner;
        _fileNameBuilderService = fileNameBuilderService;
    }

    /// <summary>
    /// Processes a subscription to find new download jobs.
    /// </summary>
    /// <param name="subscription">The subscription to process.</param>
    /// <param name="downloadSubtitles">Whether to download subtitles globally.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of download jobs.</returns>
    public async Task<List<DownloadJob>> GetJobsForSubscriptionAsync(
        Subscription subscription,
        bool downloadSubtitles,
        CancellationToken cancellationToken)
    {
        var jobs = new List<DownloadJob>();

        LocalEpisodeCache? localEpisodeCache = null;
        if (subscription.EnhancedDuplicateDetection)
        {
            var subscriptionBaseDir = _fileNameBuilderService.GetSubscriptionBaseDirectory(subscription);
            if (!string.IsNullOrWhiteSpace(subscriptionBaseDir))
            {
                localEpisodeCache = _localMediaScanner.ScanDirectory(subscriptionBaseDir, subscription.Name);
            }
        }

        await foreach (var item in QueryApiAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (subscription.ProcessedItemIds.Contains(item.Id))
            {
                _logger.LogDebug("Skipping item '{Title}' (ID: {Id}) as it was already processed for subscription '{SubscriptionName}'.", item.Title, item.Id, subscription.Name);
                continue;
            }

            var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);

            if (!ApplyFilters(tempVideoInfo, subscription, item, localEpisodeCache))
            {
                continue;
            }

            var paths = _fileNameBuilderService.GenerateDownloadPaths(tempVideoInfo, subscription);
            if (!paths.IsValid)
            {
                continue;
            }

            var videoUrl = item.UrlVideoHd ?? item.UrlVideo ?? item.UrlVideoLow;
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                _logger.LogWarning("No video URL found for item '{Title}'.", item.Title);
                continue;
            }

            // Video/Main Job
            var downloadJob = new DownloadJob { ItemId = item.Id, Title = tempVideoInfo.Title, };

            bool useStrmForThisItem = subscription.UseStreamingUrlFiles || (subscription is { SaveExtrasAsStrm: true, TreatNonEpisodesAsExtras: true } && !tempVideoInfo.IsShow);

            if (useStrmForThisItem)
            {
                downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.StrmFilePath, JobType = DownloadType.StreamingUrl });
            }
            else if (tempVideoInfo.Language == "deu" || subscription.DownloadFullVideoForSecondaryAudio)
            {
                downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.DirectDownload });
            }
            else
            {
                downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.DirectDownload });
            }

            jobs.Add(downloadJob);

            // Subtitle Job
            if (downloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
            {
                downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = item.UrlSubtitle, DestinationPath = paths.SubtitleFilePath, JobType = DownloadType.DirectDownload });
            }
        }

        return jobs;
    }

    /// <summary>
    /// Tests a subscription query and filters without creating download jobs.
    /// </summary>
    /// <param name="subscription">The subscription to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of items that would be downloaded.</returns>
    public async IAsyncEnumerable<ResultItem> TestSubscriptionAsync(
        Subscription subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // For dry-run/test, we do not scan the disk for duplicate detection to avoid security risks (CA3003)
        // and because we want to test the query logic primarily.
        // We only pass null for the cache, effectively disabling the disk check part of ApplyFilters.

        await foreach (var item in QueryApiAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            // Note: For test/dry-run, we might want to ignore the "already processed" check
            // if we assume the user wants to see what the query *covers*,
            // but strictly speaking, "what would be downloaded" implies filtering processed ones.
            // Since the subscription object coming from the UI might not have the full history
            // (unless we merge it or the UI sends it), this check depends on what the UI sends.
            // Usually, the UI sends the full object including IDs.
            if (subscription.ProcessedItemIds.Contains(item.Id))
            {
                continue;
            }

            var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);

            if (!ApplyFilters(tempVideoInfo, subscription, item, null))
            {
                continue;
            }

            var paths = _fileNameBuilderService.GenerateDownloadPaths(tempVideoInfo, subscription);
            if (!paths.IsValid)
            {
                continue;
            }

            yield return item;
        }
    }

    /// <summary>
    /// Applies filtering rules to determine if the item should be processed.
    /// </summary>
    /// <returns>True if the item passes all filters; otherwise, false.</returns>
    private bool ApplyFilters([NotNullWhen(true)]VideoInfo? tempVideoInfo, Subscription subscription, ResultItem item, LocalEpisodeCache? localEpisodeCache)
    {
        if (tempVideoInfo == null)
        {
            _logger.LogDebug("Skipping item '{Title}' due to video info parsing failure.", item.Title);
            return false;
        }

        if (localEpisodeCache != null && localEpisodeCache.Contains(tempVideoInfo))
        {
            _logger.LogInformation(
                "Skipping item '{Title}' (S{Season}E{Episode} / Abs: {Abs}) as it was found locally via enhanced duplicate detection.",
                item.Title,
                tempVideoInfo.SeasonNumber,
                tempVideoInfo.EpisodeNumber,
                tempVideoInfo.AbsoluteEpisodeNumber);

            subscription.ProcessedItemIds.Add(item.Id);
            return false;
        }

        if (!subscription.AllowAudioDescription && tempVideoInfo.HasAudiodescription)
        {
            _logger.LogDebug("Skipping item '{Title}' due to Audiodescription and subscription preference.", item.Title);
            return false;
        }

        if (!subscription.AllowSignLanguage && tempVideoInfo.HasSignLanguage)
        {
            _logger.LogDebug("Skipping item '{Title}' due to Sign Language and subscription preference.", item.Title);
            return false;
        }

        if (subscription.EnforceSeriesParsing && !tempVideoInfo.IsShow && !subscription.TreatNonEpisodesAsExtras)
        {
            _logger.LogDebug("Skipping item '{Title}' due to EnforceSeriesParsing and parsing result.", item.Title);
            return false;
        }

        if ((subscription is { EnforceSeriesParsing: true, AllowAbsoluteEpisodeNumbering: false } && tempVideoInfo is { HasSeasonEpisodeNumbering: false }) && (!subscription.TreatNonEpisodesAsExtras && !tempVideoInfo.IsShow))
        {
            _logger.LogDebug("Skipping item '{Title}' due to absolute episode numbering and subscription preference.", item.Title);
            return false;
        }

        if (subscription.TreatNonEpisodesAsExtras)
        {
            if (tempVideoInfo.IsTrailer && !subscription.SaveTrailers)
            {
                _logger.LogDebug("Skipping item '{Title}' because it is a trailer and SaveTrailers is disabled.", item.Title);
                return false;
            }

            if (tempVideoInfo.IsInterview && !subscription.SaveInterviews)
            {
                _logger.LogDebug("Skipping item '{Title}' because it is an interview and SaveInterviews is disabled.", item.Title);
                return false;
            }

            if (tempVideoInfo is { IsTrailer: false, IsInterview: false, IsShow: false } && !subscription.SaveGenericExtras)
            {
                _logger.LogDebug("Skipping item '{Title}' because it is a generic extra and SaveGenericExtras is disabled.", item.Title);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Queries the MediathekView API for results matching the subscription.
    /// </summary>
    /// <param name="subscription">The subscription to query for.</param>
    /// <param name="pageSize">The number of results per page.</param>
    /// <param name="maxPages">The maximum number of pages to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of result items retrieved from the API.</returns>
    private async IAsyncEnumerable<ResultItem> QueryApiAsync(Subscription subscription, int pageSize = 50, int maxPages = 20, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var currentPage = 0;
        var hasMoreResults = true;

        while (hasMoreResults && currentPage < maxPages)
        {
            var apiQuery = new ApiQuery
            {
                Queries = subscription.Queries,
                Size = pageSize,
                Offset = currentPage * pageSize,
                MinDuration = subscription.MinDurationMinutes.HasValue ? subscription.MinDurationMinutes * 60 : null,
                MaxDuration = subscription.MaxDurationMinutes.HasValue ? subscription.MaxDurationMinutes * 60 : null
            };

            var result = await _apiClient.SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
            if (result?.QueryInfo?.TotalResults > (currentPage + 1) * pageSize)
            {
                hasMoreResults = true;
                currentPage++;
            }
            else
            {
                hasMoreResults = false;
            }

            if (result?.Results == null)
            {
                _logger.LogWarning("Could not retrieve search results for subscription '{SubscriptionName}'.", subscription.Name);
                yield break;
            }

            foreach (var item in result.Results)
            {
                yield return item;
            }
        }
    }
}
