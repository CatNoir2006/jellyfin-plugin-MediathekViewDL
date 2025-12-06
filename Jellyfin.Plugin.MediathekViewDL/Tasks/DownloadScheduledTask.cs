using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to process download subscriptions.
/// </summary>
public class DownloadScheduledTask : IScheduledTask
{
    private readonly ILogger<DownloadScheduledTask> _logger;
    private readonly IServerConfigurationManager _configurationManager;
    private readonly MediathekViewApiClient _apiClient;
    private readonly FFmpegService _ffmpegService;
    private readonly ILibraryManager _libraryManager;
    private readonly VideoParser _videoParser;
    private readonly FileDownloader _fileDownloader;
    private readonly FileNameBuilderService _fileNameBuilderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadScheduledTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationManager">The server configuration manager.</param>
    /// <param name="apiClient">The MediathekView API client.</param>
    /// <param name="ffmpegService">The FFmpeg service.</param>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="fileDownloader">The file downloader.</param>
    /// <param name="fileNameBuilderService">The file name builder service.</param>
    public DownloadScheduledTask(
        ILogger<DownloadScheduledTask> logger,
        IServerConfigurationManager configurationManager,
        MediathekViewApiClient apiClient,
        FFmpegService ffmpegService,
        ILibraryManager libraryManager,
        VideoParser videoParser,
        FileDownloader fileDownloader,
        FileNameBuilderService fileNameBuilderService)
    {
        _logger = logger;
        _configurationManager = configurationManager;
        _apiClient = apiClient;
        _ffmpegService = ffmpegService;
        _libraryManager = libraryManager;
        _videoParser = videoParser;
        _fileDownloader = fileDownloader;
        _fileNameBuilderService = fileNameBuilderService;
    }

    /// <inheritdoc />
    public string Name => "Mediathek Abo-Downloader";

    /// <inheritdoc />
    public string Key => "MediathekAboDownloader";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Sucht nach neuen Inhalten für Abonnements und lädt sie herunter.";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Run every 6 hours
        yield return new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerInterval, IntervalTicks = TimeSpan.FromHours(6).Ticks };
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Mediathek subscription download task.");
        progress.Report(0);

        var config = Plugin.Instance?.Configuration;
        if (config == null || config.Subscriptions.Count == 0)
        {
            _logger.LogInformation("No subscriptions configured. Task finished.");
            return;
        }

        var newLastRun = DateTime.UtcNow;

        var subscriptions = config.Subscriptions.ToList();
        var subscriptionProgressShare = subscriptions.Count > 0 ? 100.0 / subscriptions.Count : 0;

        for (int i = 0; i < subscriptions.Count; i++)
        {
            var subscription = subscriptions[i];
            var baseProgressForSubscription = (double)i * subscriptionProgressShare;
            progress.Report(baseProgressForSubscription);

            _logger.LogInformation("Processing subscription: {SubscriptionName}", subscription.Name);

            // Stage 1: Collect all items for the current subscription
            var allItemsToDownload = new List<VideoParseResult>();
            var currentPage = 0;
            var hasMoreResults = true;
            var pageSize = 50;
            var maxPages = 20; // Limit to avoid excessive requests. Adjust as needed. Should be enough for most cases.

            // Paginate through results for the subscription
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
                if (result?.Total > (currentPage + 1) * pageSize)
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

                // Filter out items already processed or not matching criteria
                // Now using the VideoParser for language/feature detection
                foreach (var item in results)
                {
                    var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);
                    if (tempVideoInfo == null)
                    {
                        continue;
                    }

                    // Check Audiodescription preference
                    if (!subscription.AllowAudioDescription && tempVideoInfo.HasAudiodescription)
                    {
                        _logger.LogDebug("Skipping item '{Title}' due to Audiodescription and subscription preference.", item.Title);
                        continue;
                    }

                    // Check Sign Language preference
                    if (!subscription.AllowSignLanguage && tempVideoInfo.HasSignLanguage)
                    {
                        _logger.LogDebug("Skipping item '{Title}' due to Sign Language and subscription preference.", item.Title);
                        continue;
                    }

                    // Check if Show is required
                    if (subscription.EnforceSeriesParsing && !tempVideoInfo.IsShow)
                    {
                        _logger.LogDebug("Skipping item '{Title}' due to EnforceSeriesParsing and parsing result.", item.Title);
                        continue;
                    }

                    // Check if Season/Episode parsing is required
                    if (subscription is { EnforceSeriesParsing: true, AllowAbsoluteEpisodeNumbering: false } && tempVideoInfo is { HasSeasonEpisodeNumbering: false })
                    {
                        _logger.LogDebug("Skipping item '{Title}' due to absolute episode numbering and subscription preference.", item.Title);
                        continue;
                    }

                    allItemsToDownload.Add(new VideoParseResult { Item = item!, VideoInfo = tempVideoInfo });
                }
            }

            _logger.LogInformation("Found {Count} new, filtered items for '{SubscriptionName}'.", allItemsToDownload.Count, subscription.Name);

            // Stage 2: Download collected items and report progress
            var numItemsToDownload = allItemsToDownload.Count;
            if (numItemsToDownload == 0)
            {
                progress.Report(baseProgressForSubscription + subscriptionProgressShare);
                continue;
            }

            var progressPerItem = subscriptionProgressShare / numItemsToDownload;

            for (int j = 0; j < numItemsToDownload; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = allItemsToDownload[j];
                var baseProgressForItem = baseProgressForSubscription + (j * progressPerItem);

                var paths = _fileNameBuilderService.GenerateDownloadPaths(item.VideoInfo, subscription);
                if (!paths.IsValid)
                {
                    // Error logged in service
                    continue;
                }

                // Ensure target directory exists
                if (!Directory.Exists(paths.DirectoryPath))
                {
                    Directory.CreateDirectory(paths.DirectoryPath);
                }

                // --- Handle Video/Audio ---
                var videoUrl = item.Item.UrlVideoHd ?? item.Item.UrlVideo ?? item.Item.UrlVideoLow;

                var downloadProgress = new Progress<double>(p =>
                {
                    var itemDownloadProgress = p / 100.0 * progressPerItem;
                    progress.Report(baseProgressForItem + itemDownloadProgress);
                });

                if (subscription.UseStreamingUrlFiles)
                {
                    if (!File.Exists(paths.StrmFilePath))
                    {
                        _logger.LogInformation(
                            "Creating streaming URL file for '{Title}' at '{Path}'",
                            item.VideoInfo.Title,
                            paths.StrmFilePath);
                        await _fileDownloader.GenerateStreamingUrlFileAsync(videoUrl, paths.StrmFilePath, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (item.VideoInfo.Language == "deu")
                {
                    if (!File.Exists(paths.MainFilePath))
                    {
                        _logger.LogInformation("Downloading master video for '{Title}' to '{Path}'", item.VideoInfo.Title, paths.MainFilePath);
                        if (!await _fileDownloader.DownloadFileAsync(videoUrl, paths.MainFilePath, downloadProgress, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to download master video for '{Title}'.", item.VideoInfo.Title);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Master video for '{Title}' already exists.", item.VideoInfo.Title);
                    }
                }
                else // Non-German version: extract audio if not exists
                {
                    if (!File.Exists(paths.MainFilePath))
                    {
                        var tempVideoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4"); // Temp path for non-DE video download
                        _logger.LogInformation("Downloading temporary video for '{Title}' to extract '{Language}' audio.", item.VideoInfo.Title, item.VideoInfo.Language);
                        if (!await _fileDownloader.DownloadFileAsync(videoUrl, tempVideoPath, downloadProgress, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to download temporary video for '{Title}'.", item.VideoInfo.Title);
                            continue;
                        }

                        _logger.LogInformation("Extracting '{Language}' audio for '{Title}' to '{Path}'.", item.VideoInfo.Language, item.VideoInfo.Title, paths.MainFilePath);
                        if (!await _ffmpegService.ExtractAudioAsync(tempVideoPath, paths.MainFilePath, item.VideoInfo.Language, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to extract audio for '{Title}'.", item.VideoInfo.Title);
                        }

                        // Clean up temporary video file
                        if (File.Exists(tempVideoPath))
                        {
                            File.Delete(tempVideoPath);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("External '{Language}' audio for '{Title}' already exists.", item.VideoInfo.Language, item.VideoInfo.Title);
                    }
                }

                // --- Handle Subtitles ---
                if (config.DownloadSubtitles && !string.IsNullOrWhiteSpace(item.Item.UrlSubtitle))
                {
                    if (!File.Exists(paths.SubtitleFilePath))
                    {
                        _logger.LogInformation("Downloading '{Language}' subtitle for '{Title}' to '{Path}'.", item.VideoInfo.Language, item.VideoInfo.Title, paths.SubtitleFilePath);
                        if (!await _fileDownloader.DownloadFileAsync(item.Item.UrlSubtitle, paths.SubtitleFilePath, new Progress<double>(), cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to download subtitle for '{Title}'.", item.VideoInfo.Title);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Subtitle in '{Language}' for '{Title}' already exists.", item.VideoInfo.Language, item.VideoInfo.Title);
                    }
                }

                progress.Report(baseProgressForItem + progressPerItem);
            }
        }

        // Save the new timestamp
        config.LastRun = newLastRun;
        Plugin.Instance?.UpdateConfiguration(config);

        // Trigger library scans
        _logger.LogInformation("Triggering library scan");
        _libraryManager.QueueLibraryScan();

        progress.Report(100);
        _logger.LogInformation("Mediathek subscription download task finished.");
    }

    private sealed class VideoParseResult
    {
        public ResultItem Item { get; set; } = null!;

        public VideoInfo VideoInfo { get; set; } = null!;
    }
}
