using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
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
    public SubscriptionProcessor(
        ILogger<SubscriptionProcessor> logger,
        IMediathekViewApiClient apiClient,
        IVideoParser videoParser,
        ILocalMediaScanner localMediaScanner,
        IFileNameBuilderService fileNameBuilderService,
        IStrmValidationService strmValidationService,
        IFFmpegService ffmpegService,
        IQualityCacheRepository qualityCacheRepository,
        IDownloadHistoryRepository downloadHistoryRepository)
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
    }

    /// <inheritdoc/>
    public async Task<List<DownloadJob>> GetJobsForSubscriptionAsync(
        Subscription subscription,
        bool downloadSubtitles,
        CancellationToken cancellationToken)
    {
        var jobs = new List<DownloadJob>();

        LocalEpisodeCache? localEpisodeCache = null;
        if (subscription.EnhancedDuplicateDetection)
        {
            var subscriptionBaseDir = _fileNameBuilderService.GetSubscriptionBaseDirectory(subscription, DownloadContext.Subscription);
            if (!string.IsNullOrWhiteSpace(subscriptionBaseDir))
            {
                localEpisodeCache = _localMediaScanner.ScanDirectory(subscriptionBaseDir, subscription.Name);
            }
        }

        await foreach (var item in QueryApiAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            if (await IsInDownloadCache(item.Id, subscription.Id).ConfigureAwait(false) && !subscription.AutoUpgradeToHigherQuality)
            {
                _logger.LogDebug("Skipping item '{Title}' (ID: {Id}) as it was already processed for subscription '{SubscriptionName}'.", item.Title, item.Id, subscription.Name);
                continue;
            }

            var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);
            SetOvLanguageIfSet(subscription, tempVideoInfo);

            if (!await MatchesSubCriteriaAsync(tempVideoInfo, subscription, item, localEpisodeCache).ConfigureAwait(false))
            {
                continue;
            }

            var paths = _fileNameBuilderService.GenerateDownloadPaths(tempVideoInfo!, subscription, DownloadContext.Subscription);
            if (!paths.IsValid)
            {
                continue;
            }

            string? videoUrl = await GetUrlCandidate(item, subscription, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                continue;
            }

            // Video/Main Job
            var downloadJob = new DownloadJob { ItemId = item.Id, Title = tempVideoInfo!.Title, ItemInfo = tempVideoInfo };

            switch (paths.MainType)
            {
                case FileType.Strm:
                    // Quality Upgrade is only available for Video
                    if (localEpisodeCache?.Contains(tempVideoInfo) == true || await IsInDownloadCache(item.Id, subscription.Id).ConfigureAwait(false))
                    {
                        continue;
                    }

                    downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.StreamingUrl });
                    break;
                case FileType.Video:
                    // Resolve the file path to check for existence/quality.
                    string? existingFilePath = localEpisodeCache?.GetExistingFilePath(tempVideoInfo);

                    // If not in cache, check the standard path
                    if (string.IsNullOrEmpty(existingFilePath) && File.Exists(paths.MainFilePath))
                    {
                        existingFilePath = paths.MainFilePath;
                    }

                    if (!string.IsNullOrEmpty(existingFilePath) && File.Exists(existingFilePath))
                    {
                        var upgradeItem = await CreateQualityUpgradeItemIfAvailable(subscription, existingFilePath, videoUrl, paths.MainFilePath, cancellationToken).ConfigureAwait(false);
                        if (upgradeItem != null)
                        {
                            downloadJob.DownloadItems.Add(upgradeItem);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        var jobType = videoUrl.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
                            ? DownloadType.M3U8Download
                            : DownloadType.DirectDownload;
                        downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = jobType });
                    }

                    break;
                case FileType.Audio:
                    // Quality Upgrade is only available for Video
                    if (localEpisodeCache?.Contains(tempVideoInfo) == true || await IsInDownloadCache(item.Id, subscription.Id).ConfigureAwait(false))
                    {
                        continue;
                    }

                    downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = videoUrl, DestinationPath = paths.MainFilePath, JobType = DownloadType.AudioExtraction });
                    break;
                // Subtitles are downloaded separately.
                case FileType.Subtitle:
                default:
                    _logger.LogError("Unknown file type '{FileType}'.", paths.MainType);
                    break;
            }

            jobs.Add(downloadJob);

            // Subtitle Job
            if (downloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
            {
                downloadJob.DownloadItems.Add(new DownloadItem { SourceUrl = item.UrlSubtitle, DestinationPath = paths.SubtitleFilePath, JobType = DownloadType.DirectDownload });
            }

            if (subscription.CreateNfo)
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
                    RunTime = TimeSpan.FromSeconds(item.Duration),
                    AirDate = DateTimeOffset.FromUnixTimeSeconds(item.Timestamp).DateTime,
                    Set = string.Empty
                };
            }
        }

        return jobs;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ResultItem> TestSubscriptionAsync(
        Subscription subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // For dry-run/test, we do not scan the disk for duplicate detection to avoid security risks (CA3003)
        // and because we want to test the query logic primarily.
        // We only pass null for the cache, effectively disabling the disk check part of ApplyFilters.

        await foreach (var item in QueryApiAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);

            SetOvLanguageIfSet(subscription, tempVideoInfo);

            if (!await MatchesSubCriteriaAsync(tempVideoInfo, subscription, item, null).ConfigureAwait(false))
            {
                continue;
            }

            var paths = _fileNameBuilderService.GenerateDownloadPaths(tempVideoInfo!, subscription, DownloadContext.Subscription);
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
    private async Task<bool> MatchesSubCriteriaAsync([NotNullWhen(true)] VideoInfo? tempVideoInfo, Subscription subscription, ResultItem item, LocalEpisodeCache? localEpisodeCache)
    {
        if (tempVideoInfo == null)
        {
            _logger.LogDebug("Skipping item '{Title}' due to video info parsing failure.", item.Title);
            return false;
        }

        if (localEpisodeCache != null && localEpisodeCache.Contains(tempVideoInfo) && !subscription.AutoUpgradeToHigherQuality)
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
    /// Creates a quality upgrade download item if an upgrade is available.
    /// </summary>
    /// <param name="subscription">The subscription.</param>
    /// <param name="existingFilePath">The path to the existing file.</param>
    /// <param name="videoUrl">The new video URL.</param>
    /// <param name="targetPath">The target path for the new file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A DownloadItem if an upgrade is available, otherwise null.</returns>
    private async Task<DownloadItem?> CreateQualityUpgradeItemIfAvailable(
        Subscription subscription,
        string existingFilePath,
        string videoUrl,
        string targetPath,
        CancellationToken cancellationToken)
    {
        if (!subscription.AutoUpgradeToHigherQuality)
        {
            return null;
        }

        if (await IsQualityUpgradeAvailable(existingFilePath, videoUrl, cancellationToken).ConfigureAwait(false))
        {
            return new DownloadItem()
            {
                SourceUrl = videoUrl,
                DestinationPath = targetPath,
                ReplaceFilePath = existingFilePath,
                JobType = DownloadType.QualityUpgrade
            };
        }

        return null;
    }

    /// <summary>
    /// Tests if a quality upgrade is available by comparing current file and online URL.
    /// </summary>
    /// <param name="currentFilePath">The path of the current file.</param>
    /// <param name="newUrl">The new URL.</param>
    /// <param name="cancellationToken">The CancellationToken.</param>
    /// <returns>True if a quality upgrade is available, false otherwise.</returns>
    private async Task<bool> IsQualityUpgradeAvailable(string currentFilePath, string newUrl, CancellationToken cancellationToken)
    {
        var currentQuality = await _ffmpegService.GetMediaInfoAsync(currentFilePath, cancellationToken).ConfigureAwait(false);
        var onlineQuality = await GetOnlineQualityInfoAsync(newUrl, cancellationToken).ConfigureAwait(false);

        if (currentQuality?.IsValid() != true || onlineQuality?.IsValid() != true)
        {
            _logger.LogWarning("Could not determine media info for quality comparison.");
            return false;
        }

        if (onlineQuality.Height <= currentQuality.Height)
        {
            _logger.LogInformation("No quality upgrade available. Current height: {CurrentHeight}, Online height: {OnlineHeight}.", currentQuality.Height, onlineQuality.Height);
            return false;
        }

        if (onlineQuality.Width <= currentQuality.Width)
        {
            _logger.LogInformation("No quality upgrade available. Current width: {CurrentWidth}, Online width: {OnlineWidth}.", currentQuality.Width, onlineQuality.Width);
            return false;
        }

        if (!onlineQuality.Duration.HasValue || !currentQuality.Duration.HasValue)
        {
            _logger.LogWarning("Could not determine duration for quality comparison.");
            return false;
        }

        var onlineDuration = onlineQuality.Duration.Value;
        var currentDuration = currentQuality.Duration.Value;
        var durationDifference = Math.Abs((onlineDuration - currentDuration).TotalSeconds);

        if (durationDifference > 2) // Max 2 seconds difference allowed, may add a configuration option later
        {
            _logger.LogInformation("No quality upgrade available due to duration mismatch. Current duration: {CurrentDuration}s, Online duration: {OnlineDuration}s.", currentDuration.TotalSeconds, onlineDuration.TotalSeconds);
            return false;
        }

        _logger.LogInformation(
            "Quality upgrade available! Current: {CurrentWidth}x{CurrentHeight}, Online: {OnlineWidth}x{OnlineHeight}.",
            currentQuality.Width,
            currentQuality.Height,
            onlineQuality.Width,
            onlineQuality.Height);
        return true;
    }

    private async Task<bool> IsInDownloadCache(string itemId, Guid subscriptionId)
    {
        var item = await _downloadHistoryRepository.GetByItemIdAndSubscriptionIdAsync(itemId, subscriptionId).ConfigureAwait(false);
        return item is not null;
    }

    private void SetOvLanguageIfSet(Subscription subscription, VideoInfo? videoInfo)
    {
        if (videoInfo is { Language: "und" } && !string.IsNullOrWhiteSpace(subscription.OriginalLanguage))
        {
            videoInfo.Language = subscription.OriginalLanguage;
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
    private async Task<string?> GetUrlCandidate(ResultItem item, Subscription subscription, CancellationToken cancellationToken = default)
    {
        // If no fallback is allowed, return HD URL if available
        if (!subscription.AllowFallbackToLowerQuality)
        {
            return string.IsNullOrWhiteSpace(item.UrlVideoHd) ? null : item.UrlVideoHd;
        }

        List<string?> candidateUrls = [item.UrlVideoHd, item.UrlVideo, item.UrlVideoLow];

        // If no url availability check is required, return the first non-empty URL
        if (!subscription.QualityCheckWithUrl)
        {
            return candidateUrls.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
        }

        string? candidateUrl = null;

        var validCandidates = candidateUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

        foreach (var url in validCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (await _strmValidationService.ValidateUrlAsync(url!, cancellationToken).ConfigureAwait(false))
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

            ResultChannels result;
            try
            {
                result = await _apiClient.SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
            }
            catch (MediathekException ex)
            {
                _logger.LogWarning(ex, "Could not retrieve search results for subscription '{SubscriptionName}' due to an API error.", subscription.Name);
                yield break;
            }

            if (result.QueryInfo?.TotalResults > (currentPage + 1) * pageSize)
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
