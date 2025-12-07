using System;
using System.Collections.Generic;
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

        var currentPage = 0;
        var hasMoreResults = true;
        var pageSize = 50;
        var maxPages = 20;

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

            var results = result?.Results;
            if (results == null)
            {
                _logger.LogWarning("Could not retrieve search results for subscription '{SubscriptionName}'.", subscription.Name);
                continue;
            }

            foreach (var item in results)
            {
                if (subscription.ProcessedItemIds.Contains(item.Id))
                {
                    _logger.LogDebug("Skipping item '{Title}' (ID: {Id}) as it was already processed for subscription '{SubscriptionName}'.", item.Title, item.Id, subscription.Name);
                    continue;
                }

                var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);
                if (tempVideoInfo == null)
                {
                    _logger.LogDebug("Skipping item '{Title}' due to video info parsing failure.", item.Title);
                    continue;
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
                    continue;
                }

                if (!subscription.AllowAudioDescription && tempVideoInfo.HasAudiodescription)
                {
                    _logger.LogDebug("Skipping item '{Title}' due to Audiodescription and subscription preference.", item.Title);
                    continue;
                }

                if (!subscription.AllowSignLanguage && tempVideoInfo.HasSignLanguage)
                {
                    _logger.LogDebug("Skipping item '{Title}' due to Sign Language and subscription preference.", item.Title);
                    continue;
                }

                if (subscription.EnforceSeriesParsing && !tempVideoInfo.IsShow)
                {
                    _logger.LogDebug("Skipping item '{Title}' due to EnforceSeriesParsing and parsing result.", item.Title);
                    continue;
                }

                if (subscription is { EnforceSeriesParsing: true, AllowAbsoluteEpisodeNumbering: false } && tempVideoInfo is { HasSeasonEpisodeNumbering: false })
                {
                    _logger.LogDebug("Skipping item '{Title}' due to absolute episode numbering and subscription preference.", item.Title);
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
                var mainJob = new DownloadJob
                {
                    ItemId = item.Id,
                    Title = tempVideoInfo.Title,
                    SourceUrl = videoUrl
                };

                if (subscription.UseStreamingUrlFiles)
                {
                    mainJob.JobType = DownloadType.StreamingUrl;
                    mainJob.DestinationPath = paths.StrmFilePath;
                }
                else if (tempVideoInfo.Language == "deu" || subscription.DownloadFullVideoForSecondaryAudio)
                {
                    mainJob.JobType = DownloadType.DirectDownload;
                    mainJob.DestinationPath = paths.MainFilePath;
                }
                else
                {
                    mainJob.JobType = DownloadType.AudioExtraction;
                    mainJob.DestinationPath = paths.MainFilePath;
                    mainJob.AudioLanguage = tempVideoInfo.Language;
                }

                jobs.Add(mainJob);

                // Subtitle Job
                if (downloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
                {
                    jobs.Add(new DownloadJob
                    {
                        ItemId = item.Id,
                        Title = $"{tempVideoInfo.Title} (Subtitle)",
                        SourceUrl = item.UrlSubtitle,
                        DestinationPath = paths.SubtitleFilePath,
                        JobType = DownloadType.DirectDownload
                    });
                }
            }
        }

        return jobs;
    }
}
