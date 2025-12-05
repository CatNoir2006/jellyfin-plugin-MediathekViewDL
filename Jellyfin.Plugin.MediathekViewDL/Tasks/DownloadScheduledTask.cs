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
    public DownloadScheduledTask(
        ILogger<DownloadScheduledTask> logger,
        IServerConfigurationManager configurationManager,
        MediathekViewApiClient apiClient,
        FFmpegService ffmpegService,
        ILibraryManager libraryManager,
        VideoParser videoParser,
        FileDownloader fileDownloader)
    {
        _logger = logger;
        _configurationManager = configurationManager;
        _apiClient = apiClient;
        _ffmpegService = ffmpegService;
        _libraryManager = libraryManager;
        _videoParser = videoParser;
        _fileDownloader = fileDownloader;
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
            var progressValue = (double)i / subscriptions.Count * 100;
            progress.Report(progressValue);

            _logger.LogInformation("Processing subscription: {SubscriptionName}", subscription.Name);

            var apiQuery = new ApiQuery
            {
                Queries = subscription.Queries,
                Size = 150, // Get a decent number of results
                MinDuration = subscription.MinDurationMinutes.HasValue ? subscription.MinDurationMinutes * 60 : null,
                MaxDuration = subscription.MaxDurationMinutes.HasValue ? subscription.MaxDurationMinutes * 60 : null
            };

            var results = await _apiClient.SearchAsync(apiQuery, cancellationToken).ConfigureAwait(false);
            if (results == null)
            {
                _logger.LogWarning("Could not retrieve search results for subscription '{SubscriptionName}'.", subscription.Name);
                continue;
            }

            // Filter out items already processed or not matching criteria
            // Now using the VideoParser for language/feature detection
            var filteredItems = new List<ResultItem>();
            foreach (var item in results)
            {
                var tempVideoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);
                if (tempVideoInfo == null)
                {
                    continue;
                }

                // Check against LastRun
                if (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(item.Timestamp) <= config.LastRun)
                {
                    continue;
                }

                // Check Audiodescription preference
                if (!subscription.AllowAudioDescription && tempVideoInfo.HasAudiodescription)
                {
                    _logger.LogDebug("Skipping item '{Title}' due to Audiodescription and subscription preference.", item.Title);
                    continue;
                }

                filteredItems.Add(item);
            }

            _logger.LogInformation("Found {Count} new, filtered items for '{SubscriptionName}'.", filteredItems.Count, subscription.Name);

            foreach (var item in filteredItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var videoInfo = _videoParser.ParseVideoInfo(subscription.Name, item.Title);

                if (videoInfo is null || (subscription.EnforceSeriesParsing && !videoInfo.IsShow))
                {
                    _logger.LogDebug("Skipping item '{Title}' for '{SubscriptionName}' due to parsing failure.", item.Title, subscription.Name);
                    continue;
                }

                // Determine base download path
                var baseDownloadPath = string.IsNullOrWhiteSpace(subscription.DownloadPath)
                    ? config.DefaultDownloadPath
                    : subscription.DownloadPath;

                if (string.IsNullOrWhiteSpace(baseDownloadPath))
                {
                    _logger.LogError("No download path configured for subscription '{SubscriptionName}' or globally. Skipping item '{Title}'.", subscription.Name, item.Title);
                    continue;
                }

                // Construct full paths
                var seriesPath = Path.Combine(baseDownloadPath, subscription.Name); // Using subscription.Name directly for series path

                string targetDirectory; // This replaces seasonPath
                string fileNamePart;

                if (videoInfo.SeasonNumber.HasValue && videoInfo.EpisodeNumber.HasValue)
                {
                    targetDirectory = Path.Combine(seriesPath, $"Staffel {videoInfo.SeasonNumber.Value:D2}");
                    fileNamePart = $"S{videoInfo.SeasonNumber.Value:D2}E{videoInfo.EpisodeNumber.Value:D2}";
                }
                else if (videoInfo.AbsoluteEpisodeNumber.HasValue)
                {
                    targetDirectory = seriesPath; // No specific season folder for absolute numbered items without season info
                    fileNamePart = $"{videoInfo.AbsoluteEpisodeNumber.Value:D3}"; // Pad absolute number to 3 digits for consistency
                }
                else
                {
                    targetDirectory = seriesPath; // Treat as movie/special in series folder
                    fileNamePart = "Movie"; // Default prefix for items without specific numbering
                }

                // Ensure target directory exists
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                var baseFileName = $"{fileNamePart} - {videoInfo.Title}";

                // --- Handle Video/Audio ---
                var masterVideoPath = Path.Combine(targetDirectory, $"{baseFileName}.mkv");
                var tempVideoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4"); // Temp path for non-DE video download

                // If German version: download full video if not exists
                // Use "deu" for language comparison, as per VideoInfo
                if (videoInfo.Language == "deu")
                {
                    if (!File.Exists(masterVideoPath))
                    {
                        _logger.LogInformation("Downloading master video for '{Title}' to '{Path}'", item.Title, masterVideoPath);
                        if (!await _fileDownloader.DownloadFileAsync(item.UrlVideo, masterVideoPath, progress, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to download master video for '{Title}'.", item.Title);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Master video for '{Title}' already exists.", item.Title);
                    }
                }
                else // Non-German version: extract audio if not exists
                {
                    var externalAudioPath = Path.Combine(targetDirectory, $"{baseFileName}.{videoInfo.Language}.mka");
                    if (!File.Exists(externalAudioPath))
                    {
                        _logger.LogInformation("Downloading temporary video for '{Title}' to extract '{Language}' audio.", item.Title, videoInfo.Language);
                        if (!await _fileDownloader.DownloadFileAsync(item.UrlVideo, tempVideoPath, progress, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to download temporary video for '{Title}'.", item.Title);
                            continue;
                        }

                        _logger.LogInformation("Extracting '{Language}' audio for '{Title}' to '{Path}'.", videoInfo.Language, item.Title, externalAudioPath);
                        if (!await _ffmpegService.ExtractAudioAsync(tempVideoPath, externalAudioPath, videoInfo.Language, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to extract audio for '{Title}'.", item.Title);
                        }

                        // Clean up temporary video file
                        if (File.Exists(tempVideoPath))
                        {
                            File.Delete(tempVideoPath);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("External '{Language}' audio for '{Title}' already exists.", videoInfo.Language, item.Title);
                    }
                }

                // --- Handle Subtitles ---
                if (config.DownloadSubtitles && !string.IsNullOrWhiteSpace(item.UrlSubtitle))
                {
                    var subtitlePath = Path.Combine(targetDirectory, $"{baseFileName}.{videoInfo.Language}.ttml");
                    if (!File.Exists(subtitlePath))
                    {
                        _logger.LogInformation("Downloading '{Language}' subtitle for '{Title}' to '{Path}'.", videoInfo.Language, item.Title, subtitlePath);
                        if (!await _fileDownloader.DownloadFileAsync(item.UrlVideo, subtitlePath, progress, cancellationToken).ConfigureAwait(false))
                        {
                            _logger.LogError("Failed to download subtitle for '{Title}'.", item.Title);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Subtitle in '{Language}' for '{Title}' already exists.", videoInfo.Language, item.Title);
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
}
