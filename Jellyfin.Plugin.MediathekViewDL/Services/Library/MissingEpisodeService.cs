using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Controller.Library;
using MediathekView.Api.External;
using MediathekView.Api.Models;
using MediathekView.Api.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Service that scans local series, detects missing episodes, and synchronizes them via download or stream link.
/// </summary>
public class MissingEpisodeService : IMissingEpisodeService
{
    private static readonly char[] SeriesSeparators = [',', ';'];

    private readonly ILogger<MissingEpisodeService> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ILocalMediaScanner _localMediaScanner;
    private readonly IMediathekViewApiClient _apiClient;
    private readonly IVideoParser _videoParser;
    private readonly IFileNameBuilderService _fileNameBuilderService;
    private readonly IDownloadQueueManager _downloadQueueManager;
    private readonly IVideoQualitySelector _qualitySelector;
    private readonly ILibraryManager? _libraryManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingEpisodeService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="localMediaScanner">The local media scanner.</param>
    /// <param name="apiClient">The MediathekView API client.</param>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="fileNameBuilderService">The file name builder service.</param>
    /// <param name="downloadQueueManager">The download queue manager.</param>
    /// <param name="qualitySelector">The quality selector.</param>
    /// <param name="libraryManager">The library manager.</param>
    public MissingEpisodeService(
        ILogger<MissingEpisodeService> logger,
        IConfigurationProvider configurationProvider,
        ILocalMediaScanner localMediaScanner,
        IMediathekViewApiClient apiClient,
        IVideoParser videoParser,
        IFileNameBuilderService fileNameBuilderService,
        IDownloadQueueManager downloadQueueManager,
        IVideoQualitySelector qualitySelector,
        ILibraryManager? libraryManager = null)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _localMediaScanner = localMediaScanner;
        _apiClient = apiClient;
        _videoParser = videoParser;
        _fileNameBuilderService = fileNameBuilderService;
        _downloadQueueManager = downloadQueueManager;
        _qualitySelector = qualitySelector;
        _libraryManager = libraryManager;
    }

    /// <inheritdoc />
    public async Task<int> SyncMissingEpisodesAsync(CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null || !config.MissingEpisodes.EnableMissingEpisodesSync)
        {
            _logger.LogDebug("Missing episodes sync is disabled in configuration.");
            return 0;
        }

        var seriesDirs = DiscoverSeriesDirectories(config);
        if (seriesDirs.Count == 0)
        {
            _logger.LogInformation("No series directories found to scan for missing episodes.");
            return 0;
        }

        var whitelist = !string.IsNullOrWhiteSpace(config.MissingEpisodes.SeriesWhitelist)
            ? config.MissingEpisodes.SeriesWhitelist.Split(SeriesSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : Array.Empty<string>();

        var totalSynced = 0;
        var index = 0;

        foreach (var dir in seriesDirs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            index++;
            progress?.Report((double)index / seriesDirs.Count * 100);

            var seriesName = Path.GetFileName(dir);
            if (whitelist.Length > 0 && !whitelist.Any(w => seriesName.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            try
            {
                var synced = await SyncSeriesMissingEpisodesAsync(seriesName, dir, cancellationToken).ConfigureAwait(false);
                totalSynced += synced;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan missing episodes for series '{SeriesName}'.", seriesName);
            }
        }

        if (totalSynced > 0 && config.MissingEpisodes.AutoTriggerLibraryScan)
        {
            _libraryManager?.QueueLibraryScan();
        }

        return totalSynced;
    }

    /// <inheritdoc />
    public async Task<int> SyncSeriesMissingEpisodesAsync(string seriesName, string seriesPath, CancellationToken cancellationToken)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return 0;
        }

        // Clean series name (remove year like " (2018)")
        var cleanSeriesName = System.Text.RegularExpressions.Regex.Replace(seriesName, @"\s*\(\d{4}\).*", string.Empty).Trim();
        var localResult = _localMediaScanner.ScanSubscriptionDirectory(seriesPath, cleanSeriesName);
        var localCache = localResult.EpisodeCache;

        // Query MediathekView API for this topic/series
        var queryDto = new QueryFieldsDto
        {
            Query = cleanSeriesName
        };
        queryDto.Fields.Add(QueryFieldType.Topic);

        var query = new ApiQueryDto
        {
            Queries = new Collection<QueryFieldsDto> { queryDto },
            Size = 100
        };

        QueryResultDto result;
        try
        {
            result = await _apiClient.SearchAsync(query, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not fetch items from Mediathek for '{Series}'.", cleanSeriesName);
            return 0;
        }

        if (result.Results.Count == 0)
        {
            return 0;
        }

        var ignoredSeasons = new HashSet<int>();
        if (!string.IsNullOrWhiteSpace(config.MissingEpisodes.IgnoredSeasons))
        {
            foreach (var s in config.MissingEpisodes.IgnoredSeasons.Split(SeriesSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var match = Regex.Match(s, @"\d+");
                if (match.Success && int.TryParse(match.Value, out var seasonNum))
                {
                    ignoredSeasons.Add(seasonNum);
                }
            }
        }

        var syncedCount = 0;
        var isStreamMode = config.MissingEpisodes.SyncMode == MissingEpisodeSyncMode.StreamLink;

        foreach (var item in result.Results)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var videoInfo = _videoParser.ParseVideoInfo(item.Topic, item.Title);
            if (videoInfo?.SeasonNumber == null || videoInfo.EpisodeNumber == null)
            {
                continue;
            }

            var allowAd = config.SubscriptionDefaults.AccessibilitySettings?.AllowAudioDescription == true;
            var allowSign = config.SubscriptionDefaults.AccessibilitySettings?.AllowSignLanguage == true;

            if (AccessibilityFilterHelper.ShouldFilterOut(item.Title, videoInfo, allowAd, allowSign))
            {
                continue;
            }

            if (videoInfo.IsTrailer || videoInfo.IsInterview)
            {
                continue;
            }

            var season = videoInfo.SeasonNumber.Value;
            var episode = videoInfo.EpisodeNumber.Value;

            if (ignoredSeasons.Contains(season))
            {
                continue;
            }

            // Check if episode is already locally present (in any audio/file format)
            if (localCache.ContainsSeasonEpisode(season, episode))
            {
                continue;
            }

            // Select video quality / URL
            var videoUrl = item.GetVideoByQuality(3)?.Url;
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                continue;
            }

            if (isStreamMode)
            {
                // Write .strm file directly in target series folder
                var seasonDir = Path.Combine(seriesPath, $"Season {season:D2}");
                Directory.CreateDirectory(seasonDir);

                var strmFileName = $"{cleanSeriesName} - S{season:D2}E{episode:D2} - {SanitizeFileName(videoInfo.Title)}.strm";
                var strmFilePath = Path.Combine(seasonDir, strmFileName);

                if (!File.Exists(strmFilePath))
                {
                    await File.WriteAllTextAsync(strmFilePath, videoUrl, cancellationToken).ConfigureAwait(false);
                    localCache.Add(season, episode, null, strmFilePath, videoInfo.Language);
                    syncedCount++;
                    _logger.LogInformation("Created .strm stream link for missing episode S{Season:D2}E{Episode:D2} of '{Series}' at {Path}", season, episode, cleanSeriesName, strmFilePath);
                }
            }
            else
            {
                // Queue full download
                var dummySub = new Subscription
                {
                    Name = cleanSeriesName,
                    Download = new DownloadSettings { DownloadPath = seriesPath }
                };

                var paths = _fileNameBuilderService.GenerateDownloadPaths(videoInfo, dummySub, DownloadContext.Manual);
                if (!paths.IsValid)
                {
                    continue;
                }

                var job = new DownloadJob
                {
                    ItemId = item.Id,
                    Title = videoInfo.Title,
                    ItemInfo = videoInfo,
                    MediaMetadata = MediaMetadataFactory.Create(item, videoUrl, null, videoInfo)
                };

                job.DownloadItems.Add(new DownloadItem
                {
                    SourceUrl = videoUrl,
                    DestinationPath = paths.MainFilePath,
                    JobType = DownloadType.FFmpegDownload
                });

                _downloadQueueManager.QueueJob(job, Guid.Empty);
                localCache.Add(season, episode, null, paths.MainFilePath, videoInfo.Language);
                syncedCount++;
                _logger.LogInformation("Queued download for missing episode S{Season:D2}E{Episode:D2} of '{Series}'", season, episode, cleanSeriesName);
            }
        }

        return syncedCount;
    }

    private List<string> DiscoverSeriesDirectories(PluginConfiguration config)
    {
        var dirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. From Configured Paths
        var defaultShowPath = !string.IsNullOrWhiteSpace(config.Paths.DefaultSubscriptionShowPath)
            ? config.Paths.DefaultSubscriptionShowPath
            : config.Paths.DefaultDownloadPath;

        if (!string.IsNullOrWhiteSpace(defaultShowPath) && Directory.Exists(defaultShowPath))
        {
            foreach (var subDir in Directory.GetDirectories(defaultShowPath))
            {
                dirs.Add(subDir);
            }
        }

        // 2. From Subscriptions
        foreach (var sub in config.Subscriptions)
        {
            if (!string.IsNullOrWhiteSpace(sub.Download.DownloadPath) && Directory.Exists(sub.Download.DownloadPath))
            {
                dirs.Add(sub.Download.DownloadPath);
            }
        }

        // 3. From Jellyfin Library Manager virtual folders
        if (_libraryManager != null)
        {
            try
            {
                foreach (var vf in _libraryManager.GetVirtualFolders())
                {
                    if (string.Equals(vf.CollectionType?.ToString(), "tvshows", StringComparison.OrdinalIgnoreCase) && vf.Locations != null)
                    {
                        foreach (var loc in vf.Locations)
                        {
                            if (Directory.Exists(loc))
                            {
                                foreach (var d in Directory.GetDirectories(loc))
                                {
                                    dirs.Add(d);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to inspect Jellyfin library virtual folders.");
            }
        }

        return dirs.ToList();
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
    }
}
