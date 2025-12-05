using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        for (int i = 0; i < subscriptions.Count; i++)
        {
            var subscription = subscriptions[i];
            var progressValue = (double)(i + 1) / subscriptions.Count * 100;
            progress.Report(progressValue);

            _logger.LogInformation("Processing subscription: {SubscriptionName}", subscription.Name);
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
                var filteredItems = new List<VideoParseResult>();
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

                    filteredItems.Add(new VideoParseResult { Item = item!, VideoInfo = tempVideoInfo });
                }

                _logger.LogInformation("Found {Count} new, filtered items for '{SubscriptionName}'.", filteredItems.Count, subscription.Name);

                foreach (var item in filteredItems)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string targetPath = _fileNameBuilderService.BuildDirectoryName(item.VideoInfo, subscription);
                    if (string.IsNullOrWhiteSpace(targetPath))
                    {
                        _logger.LogError("No download path configured for subscription '{SubscriptionName}' or globally. Skipping item '{Title}'.", subscription.Name, item.VideoInfo.Title);
                        continue;
                    }

                    // Ensure target directory exists
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    // --- Handle Video/Audio ---
                    var subtitleFilePath = Path.Combine(targetPath, _fileNameBuilderService.BuildFileName(item.VideoInfo, true));
                    var mainFilePath = Path.Combine(targetPath, _fileNameBuilderService.BuildFileName(item.VideoInfo, false));
                    var tempVideoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4"); // Temp path for non-DE video download
                    var videoUrl = item.Item.UrlVideoHd ?? item.Item.UrlVideo ?? item.Item.UrlVideoLow;

                    // If German version: download full video if not exists
                    // Use "deu" for language comparison, as per VideoInfo
                    if (item.VideoInfo.Language == "deu")
                    {
                        if (!File.Exists(mainFilePath))
                        {
                            _logger.LogInformation("Downloading master video for '{Title}' to '{Path}'", item.VideoInfo.Title, mainFilePath);
                            if (!await _fileDownloader.DownloadFileAsync(videoUrl, mainFilePath, progress, cancellationToken).ConfigureAwait(false))
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
                        if (!File.Exists(mainFilePath))
                        {
                            _logger.LogInformation("Downloading temporary video for '{Title}' to extract '{Language}' audio.", item.VideoInfo.Title, item.VideoInfo.Language);
                            if (!await _fileDownloader.DownloadFileAsync(videoUrl, tempVideoPath, progress, cancellationToken).ConfigureAwait(false))
                            {
                                _logger.LogError("Failed to download temporary video for '{Title}'.", item.VideoInfo.Title);
                                continue;
                            }

                            _logger.LogInformation("Extracting '{Language}' audio for '{Title}' to '{Path}'.", item.VideoInfo.Language, item.VideoInfo.Title, mainFilePath);
                            if (!await _ffmpegService.ExtractAudioAsync(tempVideoPath, mainFilePath, item.VideoInfo.Language, cancellationToken).ConfigureAwait(false))
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
                        if (!File.Exists(subtitleFilePath))
                        {
                            _logger.LogInformation("Downloading '{Language}' subtitle for '{Title}' to '{Path}'.", item.VideoInfo.Language, item.VideoInfo.Title, subtitleFilePath);
                            if (!await _fileDownloader.DownloadFileAsync(item.Item.UrlSubtitle, subtitleFilePath, progress, cancellationToken).ConfigureAwait(false))
                            {
                                _logger.LogError("Failed to download subtitle for '{Title}'.", item.VideoInfo.Title);
                            }
                        }
                        else
                        {
                            _logger.LogDebug("Subtitle in '{Language}' for '{Title}' already exists.", item.VideoInfo.Language, item.VideoInfo.Title);
                        }
                    }
                }
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
