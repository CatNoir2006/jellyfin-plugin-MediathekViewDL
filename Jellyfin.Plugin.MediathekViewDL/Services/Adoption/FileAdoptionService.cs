using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FuzzySharp;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Implementation of the FileAdoptionService.
/// </summary>
public class FileAdoptionService : IFileAdoptionService
{
    private readonly ILogger<FileAdoptionService> _logger;
    private readonly IConfigurationProvider _configProvider;
    private readonly ILocalMediaScanner _localMediaScanner;
    private readonly IDownloadHistoryRepository _historyRepository;
    private readonly ISubscriptionProcessor _subscriptionProcessor;
    private readonly IFileNameBuilderService _fileNameBuilder;
    private readonly ITempMetadataCache _tempMetadataCache;

    // Regex to find URL in MediathekView info files
    private static readonly Regex _urlRegex = new(@"URL\s*(https?://[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="FileAdoptionService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="localMediaScanner">The local media scanner.</param>
    /// <param name="historyRepository">The download history repository.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    /// <param name="fileNameBuilder">The file name builder service.</param>
    /// <param name="tempMetadataCache">The temporary metadata cache.</param>
    public FileAdoptionService(
        ILogger<FileAdoptionService> logger,
        IConfigurationProvider configProvider,
        ILocalMediaScanner localMediaScanner,
        IDownloadHistoryRepository historyRepository,
        ISubscriptionProcessor subscriptionProcessor,
        IFileNameBuilderService fileNameBuilder,
        ITempMetadataCache tempMetadataCache)
    {
        _logger = logger;
        _configProvider = configProvider;
        _localMediaScanner = localMediaScanner;
        _historyRepository = historyRepository;
        _subscriptionProcessor = subscriptionProcessor;
        _fileNameBuilder = fileNameBuilder;
        _tempMetadataCache = tempMetadataCache;
    }

    /// <inheritdoc />
    public async Task<AdoptionInfo> GetAdoptionCandidatesAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = _configProvider.Configuration.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription {SubscriptionId} not found.", subscriptionId);
            return new AdoptionInfo
            {
                Candidates = Array.Empty<AdoptionCandidate>(),
                ApiResults = Array.Empty<ApiResultWithInfo>()
            };
        }

        // Create a copy of the subscription with IgnoreLocalFiles and IgnoreHistory set to true
        // to get all possible items from the API without being filtered by what we already have.
        var searchSub = subscription with { IgnoreLocalFiles = true, IgnoreHistory = true };

        // 1. Get all potential items from API
        var apiItems = new List<ApiResultWithInfo>();
        await foreach (var (item, videoInfo) in _subscriptionProcessor.GetEligibleItemsAsync(searchSub, cancellationToken).ConfigureAwait(false))
        {
            apiItems.Add(new ApiResultWithInfo(item, videoInfo));
        }

        // 2. Get local files
        var baseDirectories = _fileNameBuilder.GetSubscriptionBaseDirectories(subscription, DownloadContext.Subscription).ToList();
        var allScannedFiles = new List<ScannedFile>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dir in baseDirectories)
        {
            var scanResult = _localMediaScanner.ScanSubscriptionDirectory(dir, subscription.Name);
            foreach (var scannedFile in scanResult.Files)
            {
                if (seenPaths.Add(scannedFile.FilePath))
                {
                    allScannedFiles.Add(scannedFile);
                }
            }
        }

        // 3. Get history for pre-matching
        var history = (await _historyRepository.GetBySubscriptionIdAsync(subscription.Id).ConfigureAwait(false)).ToList();

        var candidates = new List<AdoptionCandidate>();

        // Group files by their base name
        var videoFiles = allScannedFiles.Where(f => f.Type == FileType.Video || f.Type == FileType.Strm).ToList();
        var otherFiles = allScannedFiles.Where(f => f.Type != FileType.Video && f.Type != FileType.Strm).ToList();

        foreach (var video in videoFiles)
        {
            var videoFileName = Path.GetFileNameWithoutExtension(video.FilePath);
            var relatedFiles = otherFiles
                .Where(f => Path.GetFileName(f.FilePath).StartsWith(videoFileName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var allFiles = new List<string> { video.FilePath };
            allFiles.AddRange(relatedFiles.Select(f => f.FilePath));

            var matches = await FindMatchesAsync(subscription, video, relatedFiles, apiItems, history, cancellationToken).ConfigureAwait(false);

            candidates.Add(new AdoptionCandidate
            {
                Id = video.FilePath,
                FilePaths = allFiles,
                Matches = matches
            });
        }

        return new AdoptionInfo
        {
            Candidates = candidates,
            ApiResults = apiItems
        };
    }

    /// <inheritdoc />
    public async Task SetApiIdAsync(Guid subscriptionId, string candidateId, string apiId, string? videoUrl = null, CancellationToken cancellationToken = default)
    {
        var subscription = _configProvider.Configuration.Subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        if (subscription == null)
        {
            return;
        }

        var finalUrl = videoUrl ?? "adopted://" + apiId;
        var title = Path.GetFileNameWithoutExtension(candidateId);

        await _historyRepository.AddAsync(
            finalUrl,
            apiId,
            subscriptionId,
            candidateId,
            title,
            "deu").ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SetMappingsAsync(Guid subscriptionId, IReadOnlyList<FileAdoptionMapping> mappings, CancellationToken cancellationToken = default)
    {
        foreach (var mapping in mappings)
        {
            await SetApiIdAsync(subscriptionId, mapping.CandidateId, mapping.ApiId, mapping.VideoUrl, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<AdoptionMatch>> FindMatchesAsync(
        Subscription subscription,
        ScannedFile video,
        List<ScannedFile> relatedFiles,
        List<ApiResultWithInfo> apiItems,
        List<DownloadHistoryEntry> history,
        CancellationToken cancellationToken)
    {
        var matches = new List<AdoptionMatch>();

        // 1. Check if already in history by path
        var existingEntry = history.FirstOrDefault(h => h.DownloadPath == video.FilePath);
        if (existingEntry != null && !string.IsNullOrEmpty(existingEntry.ItemId))
        {
            matches.Add(new AdoptionMatch
            {
                ApiId = existingEntry.ItemId,
                ApiTitle = existingEntry.Title,
                VideoUrl = existingEntry.VideoUrl,
                Confidence = 100,
                IsConfirmed = true,
                Source = AdoptionMatchSource.History
            });
        }

        // 2. Try to find URL in .txt info files to match with apiItems
        string? urlFromInfo = null;
        foreach (var infoFile in relatedFiles.Where(f => f.Type == FileType.Info))
        {
            if (Path.GetExtension(infoFile.FilePath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var content = await File.ReadAllTextAsync(infoFile.FilePath, cancellationToken).ConfigureAwait(false);
                    var urlMatch = _urlRegex.Match(content);

                    if (urlMatch.Success)
                    {
                        urlFromInfo = urlMatch.Groups[1].Value;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read info file {Path}", infoFile.FilePath);
                }
            }
        }

        // 3. Match against the retrieved API items
        if (video.VideoInfo != null)
        {
            var potentialMatches = apiItems
                .Select(item =>
                {
                    var (score, source) = CalculateMatchScoreWithSource(video.VideoInfo, item, urlFromInfo);
                    return new { Item = item, Score = score, Source = source };
                })
                .Where(x => x.Score > 0.1) // Lower threshold to get more potential matches
                .OrderByDescending(x => x.Score)
                .Take(5)
                .ToList();

            foreach (var match in potentialMatches)
            {
                // If it's already the confirmed match from history, skip it
                if (matches.Any(m => m.ApiId == match.Item.Item.Id))
                {
                    continue;
                }

                matches.Add(new AdoptionMatch
                {
                    ApiId = match.Item.Item.Id,
                    ApiTitle = match.Item.Item.Title,
                    VideoUrl = urlFromInfo ?? match.Item.Item.GetVideoByQuality()?.Url,
                    Confidence = Math.Min(100.0, match.Score * 100.0),
                    IsConfirmed = false,
                    Source = match.Source
                });
            }
        }

        return matches.OrderByDescending(m => m.Confidence).Take(5).ToList();
    }

    private double CalculateMatchScore(VideoInfo localInfo, ApiResultWithInfo apiResult, string? urlFromInfo)
    {
        return CalculateMatchScoreWithSource(localInfo, apiResult, urlFromInfo).Score;
    }

    private (double Score, AdoptionMatchSource Source) CalculateMatchScoreWithSource(VideoInfo localInfo, ApiResultWithInfo apiResult, string? urlFromInfo)
    {
        // If we have a URL from info file, and it matches one of the item's URLs, it's a perfect match
        if (!string.IsNullOrEmpty(urlFromInfo))
        {
            if (apiResult.Item.VideoUrls.Any(v => v.Url.Equals(urlFromInfo, StringComparison.OrdinalIgnoreCase)))
            {
                return (10.0, AdoptionMatchSource.Url); // Extremely high score for exact URL match
            }
        }

        var apiInfo = apiResult.VideoInfo;
        var scores = new List<AdoptionScore>();
        var source = AdoptionMatchSource.Fuzzy;

        // Fuzzy search for Title and Topic, we do both Ratio and PartialRatio to give some weight to partial matches which are common in file names
        scores.Add(new AdoptionScore(Fuzz.PartialRatio(localInfo.Title, apiInfo.Title) / 100.0, 0.7));
        scores.Add(new AdoptionScore(Fuzz.Ratio(localInfo.Title, apiInfo.Title) / 100.0, 0.7));
        scores.Add(new AdoptionScore(Fuzz.PartialRatio(localInfo.Topic, apiInfo.Topic) / 100.0, 0.3));
        scores.Add(new AdoptionScore(Fuzz.Ratio(localInfo.Topic, apiInfo.Topic) / 100.0, 0.3));

        // Boost for Season/Episode match
        if (localInfo.HasSeasonEpisodeNumbering && apiInfo.HasSeasonEpisodeNumbering)
        {
            if (localInfo.SeasonNumber == apiInfo.SeasonNumber && localInfo.EpisodeNumber == apiInfo.EpisodeNumber)
            {
                // Apply multiplier of 1.4 for exact S/E match
                scores.Add(new AdoptionScore(1.0, 1.4, AdoptionScoreType.Multiply));
                source = AdoptionMatchSource.SeriesNumbering;
            }
            else
            {
                // Penalize if season/episode numbers are present but don't match, as this is a strong signal they might be different
                scores.Add(new AdoptionScore(1.0, -1.4, AdoptionScoreType.Multiply));
            }
        }

        // Boost for Absolute Episode match
        if (localInfo.HasAbsoluteNumbering && apiInfo.HasAbsoluteNumbering)
        {
            if (localInfo.AbsoluteEpisodeNumber == apiInfo.AbsoluteEpisodeNumber)
            {
                // Apply multiplier of 1.3 for absolute episode match
                scores.Add(new AdoptionScore(1.0, 1.3, AdoptionScoreType.Multiply));
                source = AdoptionMatchSource.SeriesNumbering;
            }
            else
            {
                // Penalize if absolute numbers are present but don't match, as this is a strong signal they might be different
                scores.Add(new AdoptionScore(1.0, -1.3, AdoptionScoreType.Multiply));
            }
        }

        return (CalculateTotalScore(scores), source);
    }

    private double CalculateTotalScore(IEnumerable<AdoptionScore> scores)
    {
        double weightedSum = 0;
        double totalWeight = 0;
        double multiplier = 1.0;

        foreach (var score in scores)
        {
            if (score.Type == AdoptionScoreType.Value)
            {
                weightedSum += score.Value * score.Weight;
                totalWeight += score.Weight;
            }
            else if (score.Type == AdoptionScoreType.Multiply)
            {
                if (score.Value >= 1.0)
                {
                    if (score.Weight >= 0)
                    {
                        multiplier *= score.Weight;
                    }
                    else
                    {
                        // Penalties (negative weights) reduce the score.
                        // We divide by the absolute value to ensure consistent reduction.
                        multiplier /= Math.Abs(score.Weight);
                    }
                }
            }
        }

        if (totalWeight <= 0)
        {
            return 0;
        }

        return (weightedSum / totalWeight) * multiplier;
    }
}
