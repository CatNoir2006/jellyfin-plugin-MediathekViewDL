using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;

/// <summary>
/// Service responsible for searching and filtering content for subscriptions.
/// </summary>
public class SubscriptionProcessor : ISubscriptionProcessor
{
    private readonly ILogger<SubscriptionProcessor> _logger;
    private readonly IMediathekViewApiClient _apiClient;
    private readonly IVideoParser _videoParser;
    private readonly ILocalMediaScanner _localMediaScanner;
    private readonly IFileNameBuilderService _fileNameBuilderService;
    private readonly IStrmValidationService _strmValidationService;
    private readonly IFFmpegService _ffmpegService;
    private readonly IQualityCacheRepository _qualityCacheRepository;
    private readonly IDownloadHistoryRepository _downloadHistoryRepository;
    private readonly IConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionProcessor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="apiClient">The API client.</param>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="localMediaScanner">The local media scanner.</param>
    /// <param name="fileNameBuilderService">The file name builder service.</param>
    /// <param name="strmValidationService">The STRM validation service.</param>
    /// <param name="ffmpegService">The ffmpeg Service.</param>
    /// <param name="qualityCacheRepository">The QualityCacheRepository.</param>
    /// <param name="downloadHistoryRepository">The Download History Repo.</param>
    /// <param name="configurationProvider">The Configuration Provider.</param>
    public SubscriptionProcessor(
        ILogger<SubscriptionProcessor> logger,
        IMediathekViewApiClient apiClient,
        IVideoParser videoParser,
        ILocalMediaScanner localMediaScanner,
        IFileNameBuilderService fileNameBuilderService,
        IStrmValidationService strmValidationService,
        IFFmpegService ffmpegService,
        IQualityCacheRepository qualityCacheRepository,
        IDownloadHistoryRepository downloadHistoryRepository,
        IConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _apiClient = apiClient;
        _videoParser = videoParser;
        _localMediaScanner = localMediaScanner;
        _fileNameBuilderService = fileNameBuilderService;
        _strmValidationService = strmValidationService;
        _ffmpegService = ffmpegService;
        _qualityCacheRepository = qualityCacheRepository;
        _downloadHistoryRepository = downloadHistoryRepository;
        _configurationProvider = configurationProvider;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<(ResultItemDto Item, VideoInfo VideoInfo)> GetEligibleItemsAsync(
        Subscription subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        LocalEpisodeCache? localEpisodeCache = null;
        if (subscription.Download.EnhancedDuplicateDetection && !subscription.IgnoreLocalFiles)
        {
            var subscriptionBaseDir = _fileNameBuilderService.GetSubscriptionBaseDirectory(subscription, DownloadContext.Subscription);
            if (!string.IsNullOrWhiteSpace(subscriptionBaseDir))
            {
                localEpisodeCache = _localMediaScanner.ScanDirectory(subscriptionBaseDir, subscription.Name);
            }
        }

        await foreach (var item in QueryApiAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (!subscription.IgnoreHistory && await IsInDownloadCache(item.Id, subscription.Id).ConfigureAwait(false))
            {
                _logger.LogDebug("Skipping item '{Title}' (ID: {Id}) as it was already processed for subscription '{SubscriptionName}'.", item.Title, item.Id, subscription.Name);
                continue;
            }

            var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);
            SetOvLanguageIfSet(subscription, tempVideoInfo);

            if (tempVideoInfo != null && (subscription.Metadata.AppendDateToTitle || subscription.Metadata.AppendTimeToTitle))
            {
                var suffixParts = new List<string>();

                if (subscription.Metadata.AppendDateToTitle)
                {
                    var dateStr = item.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (!tempVideoInfo.Title.Contains(dateStr, StringComparison.OrdinalIgnoreCase))
                    {
                        suffixParts.Add(dateStr);
                    }
                }

                if (subscription.Metadata.AppendTimeToTitle)
                {
                    // using HH-mm because : is invalid in filenames
                    var timeStr = item.Timestamp.ToString("HH-mm", CultureInfo.InvariantCulture);
                    if (!tempVideoInfo.Title.Contains(timeStr, StringComparison.OrdinalIgnoreCase))
                    {
                        suffixParts.Add(timeStr);
                    }
                }

                if (suffixParts.Count > 0)
                {
                    tempVideoInfo.Title = $"{tempVideoInfo.Title} - {string.Join(" ", suffixParts)}";
                }

                tempVideoInfo.IsShow = true;
            }

            if (!await MatchesSubCriteriaAsync(tempVideoInfo, subscription, item, localEpisodeCache).ConfigureAwait(false))
            {
                continue;
            }

            yield return (item, tempVideoInfo!);
        }
    }

    /// <inheritdoc/>
    public async Task<List<DownloadJob>> GetJobsForSubscriptionAsync(
        Subscription subscription,
        bool downloadSubtitles,
        CancellationToken cancellationToken)
    {
        var jobs = new List<DownloadJob>();

        await foreach (var (item, tempVideoInfo) in GetEligibleItemsAsync(subscription, cancellationToken).ConfigureAwait(false))
        {
            var paths = _fileNameBuilderService.GenerateDownloadPaths(tempVideoInfo, subscription, DownloadContext.Subscription);
            if (!paths.IsValid)
            {
                continue;
            }

            string? videoUrl = await GetUrlCandidate(item, subscription, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                continue;
            }

            // Download Task
            var downloadJob = new DownloadJob { ItemId = item.Id, Title = tempVideoInfo.Title, ItemInfo = tempVideoInfo };

            // Video/Main Item
            switch (paths.MainType)
            {
                case FileType.Strm:
                    downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.StreamingUrl });
                    break;
                case FileType.Video:
                    var jobType = videoUrl.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
                        ? DownloadType.M3U8Download
                        : DownloadType.DirectDownload;
                    downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = jobType });

                    break;
                case FileType.Audio:
                    downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.AudioExtraction });
                    break;
                // Subtitles are downloaded separately.
                case FileType.Subtitle:
                default:
                    _logger.LogError("Unknown file type '{FileType}'.", paths.MainType);
                    break;
            }

            // Subtitle Item
            if (downloadSubtitles)
            {
                foreach (var sub in item.SubtitleUrls)
                {
                    if (sub.Type == SubtitleType.Unknown)
                    {
                        continue;
                    }

                    string subPath = paths.SubtitleFilePath;
                    if (sub.Type == SubtitleType.WEBVTT)
                    {
                        subPath = Path.ChangeExtension(subPath, ".vtt");
                    }

                    downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = sub.Url, DestinationPath = subPath, JobType = DownloadType.DirectDownload });
                }
            }

            if (subscription.Metadata.CreateNfo)
            {
                var topic = string.IsNullOrWhiteSpace(subscription.Name) ? item.Topic : subscription.Name;

                downloadJob.NfoMetadata = new NfoDTO()
                {
                    Title = tempVideoInfo.Title,
                    Description = item.Description,
                    Show = tempVideoInfo.SeasonNumber.HasValue ? topic : string.Empty,
                    Season = tempVideoInfo.SeasonNumber,
                    Episode = tempVideoInfo.EpisodeNumber,
                    Id = item.Id,
                    FilePath = paths.NfoFilePath,
                    Studio = item.Channel,
                    RunTime = item.Duration,
                    AirDate = item.Timestamp.DateTime,
                    Set = string.Empty
                };
            }

            jobs.Add(downloadJob);
        }

        return jobs;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ResultItemDto> TestSubscriptionAsync(
        Subscription subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // For dry-run/test, we do not scan the disk for duplicate detection to avoid security risks (CA3003)
        // and because we want to test the query logic primarily.
        // We ensure IgnoreLocalFiles is set for this call.
        var testSub = subscription with { IgnoreLocalFiles = true };

        await foreach (var (item, tempVideoInfo) in GetEligibleItemsAsync(testSub, cancellationToken).ConfigureAwait(false))
        {
            var paths = _fileNameBuilderService.GenerateDownloadPaths(tempVideoInfo, testSub, DownloadContext.Subscription);
            string path = paths.MainFilePath;
            if (!paths.IsValid)
            {
                path = "Warnung: Ungültiger Pfad";
            }

            var description = item.Description ?? string.Empty;
            if (description.Length > 100)
            {
                description = string.Concat(description.AsSpan(0, 100), "...");
            }

            yield return item with { Description = $"Pfad: {path} | {description}" };
        }
    }

    /// <summary>
    /// Applies filtering rules to determine if the item should be processed.
    /// </summary>
    /// <returns>True if the item passes all filters; otherwise, false.</returns>
    private async Task<bool> MatchesSubCriteriaAsync([NotNullWhen(true)] VideoInfo? tempVideoInfo, Subscription subscription, ResultItemDto item, LocalEpisodeCache? localEpisodeCache)
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

            var localPath = localEpisodeCache.GetExistingFilePath(tempVideoInfo);
            await _downloadHistoryRepository.AddAsync(string.Empty, item.Id, subscription.Id, localPath!, item.Title, tempVideoInfo.Language).ConfigureAwait(false);
            return false;
        }

        if (!subscription.Accessibility.AllowAudioDescription && tempVideoInfo.HasAudiodescription)
        {
            _logger.LogDebug("Skipping item '{Title}' due to Audiodescription and subscription preference.", item.Title);
            return false;
        }

        if (!subscription.Accessibility.AllowSignLanguage && tempVideoInfo.HasSignLanguage)
        {
            _logger.LogDebug("Skipping item '{Title}' due to Sign Language and subscription preference.", item.Title);
            return false;
        }

        if (subscription.Series.EnforceSeriesParsing && !tempVideoInfo.IsShow && !subscription.Series.TreatNonEpisodesAsExtras)
        {
            _logger.LogDebug("Skipping item '{Title}' due to EnforceSeriesParsing and parsing result.", item.Title);
            return false;
        }

        if ((subscription.Series.EnforceSeriesParsing && !subscription.Series.AllowAbsoluteEpisodeNumbering && !tempVideoInfo.HasSeasonEpisodeNumbering) && (!subscription.Series.TreatNonEpisodesAsExtras && !tempVideoInfo.IsShow))
        {
            _logger.LogDebug("Skipping item '{Title}' due to absolute episode numbering and subscription preference.", item.Title);
            return false;
        }

        if (subscription.Series.TreatNonEpisodesAsExtras)
        {
            if (tempVideoInfo.IsTrailer && !subscription.Series.SaveTrailers)
            {
                _logger.LogDebug("Skipping item '{Title}' because it is a trailer and SaveTrailers is disabled.", item.Title);
                return false;
            }

            if (tempVideoInfo.IsInterview && !subscription.Series.SaveInterviews)
            {
                _logger.LogDebug("Skipping item '{Title}' because it is an interview and SaveInterviews is disabled.", item.Title);
                return false;
            }

            if (tempVideoInfo is { IsTrailer: false, IsInterview: false, IsShow: false } && !subscription.Series.SaveGenericExtras)
            {
                _logger.LogDebug("Skipping item '{Title}' because it is a generic extra and SaveGenericExtras is disabled.", item.Title);
                return false;
            }
        }

        return true;
    }

    private async Task<bool> IsInDownloadCache(string itemId, Guid subscriptionId)
    {
        var item = await _downloadHistoryRepository.GetByItemIdAndSubscriptionIdAsync(itemId, subscriptionId).ConfigureAwait(false);
        return item is not null;
    }

    private void SetOvLanguageIfSet(Subscription subscription, VideoInfo? videoInfo)
    {
        if (videoInfo is { Language: "und" } && !string.IsNullOrWhiteSpace(subscription.Metadata.OriginalLanguage))
        {
            videoInfo.Language = subscription.Metadata.OriginalLanguage;
        }
    }

    private async Task<LocalMediaInfo?> GetOnlineQualityInfoAsync(string url, CancellationToken cancellationToken)
    {
        // First, try to get from cache
        var cachedInfo = await GetOnlineQualityInfoFromCacheAsync(url, cancellationToken).ConfigureAwait(false);
        if (cachedInfo != null)
        {
            return cachedInfo;
        }

        // If not in cache, get from ffmpeg
        var mediaInfo = await _ffmpegService.GetMediaInfoAsync(url, cancellationToken).ConfigureAwait(false);
        if (mediaInfo is null || !mediaInfo.IsValid())
        {
            return null;
        }

        // Store in cache for future use
        await _qualityCacheRepository.AddOrUpdateAsync(
            url,
            mediaInfo.Width.Value,
            mediaInfo.Height.Value,
            mediaInfo.Duration.Value,
            mediaInfo.FileSize.Value).ConfigureAwait(false);

        return mediaInfo;
    }

    private async Task<LocalMediaInfo?> GetOnlineQualityInfoFromCacheAsync(string url, CancellationToken cancellationToken)
    {
        var cacheEntry = await _qualityCacheRepository.GetByUrlAsync(url).ConfigureAwait(false);
        if (cacheEntry is null)
        {
            return null;
        }

        // ReSharper disable once InvertIf
        if (cacheEntry.Duration == TimeSpan.Zero)
        {
            _logger.LogInformation("Cached entry for URL '{Url}' has zero duration, ignoring cache.", url);
            cancellationToken.ThrowIfCancellationRequested();
            await _qualityCacheRepository.RemoveByUrlAsync(url).ConfigureAwait(false);
            return null;
        }

        return new LocalMediaInfo
        {
            Width = cacheEntry.Width,
            Height = cacheEntry.Height,
            FileSize = cacheEntry.Size,
            Duration = cacheEntry.Duration,
            FilePath = url
        };
    }

    /// <summary>
    /// Gets the best available URL candidate for downloading the video.
    /// </summary>
    /// <param name="item">The item to get the url for.</param>
    /// <param name="subscription">The subscription.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The best URL candidate, or null if none found.</returns>
    private async Task<string?> GetUrlCandidate(ResultItemDto item, Subscription subscription, CancellationToken cancellationToken = default)
    {
        // Quality: 3=HD, 2=Std, 1=Low
        var hdUrl = item.VideoUrls.FirstOrDefault(v => v.Quality == 3)?.Url;

        // If no fallback is allowed, return HD URL if available
        if (!subscription.Download.AllowFallbackToLowerQuality)
        {
            return hdUrl;
        }

        List<string> candidateUrls = item.VideoUrls.OrderByDescending(s => s.Quality).Select(s => s.Url).ToList();

        // If no url availability check is required, return the first URL
        if (!subscription.Download.QualityCheckWithUrl)
        {
            return candidateUrls.Count > 0 ? candidateUrls[0] : null;
        }

        string? candidateUrl = null;

        var validCandidates = candidateUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

        foreach (var url in validCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (await _strmValidationService.ValidateUrlAsync(url, cancellationToken).ConfigureAwait(false))
                {
                    candidateUrl = url;
                    if (url != validCandidates.First())
                    {
                        _logger.LogWarning("Primary quality download failed for '{Title}'. Fallback to: {Url}", item.Title, url);
                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate URL '{Url}' for '{Title}'. Trying next quality...", url, item.Title);
            }
        }

        if (string.IsNullOrWhiteSpace(candidateUrl))
        {
            _logger.LogWarning("No valid video URL found for item '{Title}'.", item.Title);
            return null;
        }

        return candidateUrl;
    }

    /// <summary>
    /// Queries the MediathekView API for results matching the subscription.
    /// </summary>
    /// <param name="subscription">The subscription to query for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of result items retrieved from the API.</returns>
    private async IAsyncEnumerable<ResultItemDto> QueryApiAsync(Subscription subscription, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var currentPage = 0;
        var hasMoreResults = true;
        var pageSize = _configurationProvider.Configuration.Search.PageSize;
        var maxPages = _configurationProvider.Configuration.Search.MaxPages;

        while (hasMoreResults && currentPage < maxPages)
        {
            var apiQuery = new ApiQueryDto
            {
                Queries = subscription.Search.Criteria,
                Size = pageSize,
                Offset = currentPage * pageSize,
                MinDuration = subscription.Search.MinDurationMinutes * 60,
                MaxDuration = subscription.Search.MaxDurationMinutes * 60,
                MinBroadcastDate = subscription.Search.MinBroadcastDate,
                MaxBroadcastDate = subscription.Search.MaxBroadcastDate,
                Future = _configurationProvider.Configuration.Search.SearchInFutureBroadcasts,
            };

            QueryResultDto result;
            try
            {
                result = await _apiClient.SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
            }
            catch (MediathekException ex)
            {
                _logger.LogWarning(ex, "Could not retrieve search results for subscription '{SubscriptionName}' due to an API error.", subscription.Name);
                yield break;
            }

            if (result.QueryInfo.TotalResults > (currentPage + 1) * pageSize)
            {
                hasMoreResults = true;
                currentPage++;
            }
            else
            {
                hasMoreResults = false;
            }

            foreach (var item in result.Results)
            {
                yield return item;
            }
        }
    }
}
