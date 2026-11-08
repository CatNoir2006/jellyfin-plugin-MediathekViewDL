using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using MediaBrowser.Controller.Library;
using MediathekView.Api.External;
using MediathekView.Api.Models;
using MediathekView.Api.Models.Enums;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Implements virtual streaming library synchronization using .strm and .nfo pointers.
/// </summary>
public class VirtualLibrarySyncService : IVirtualLibrarySyncService
{
    private static readonly char[] TopicSeparators = [',', ';'];

    /// <summary>
    /// Default curated popular series across German public broadcasters.
    /// </summary>
    public static readonly string[] DefaultPopularSeries =
    [
        "The Rookie",
        "Babylon Berlin",
        "Sherlock",
        "Peaky Blinders",
        "Das Boot",
        "Bad Banks",
        "Charité",
        "Tatort",
        "Polizeiruf 110",
        "Nord bei Nordwest",
        "Der Bergdoktor",
        "Die Bergretter",
        "Der Pass",
        "Kafka",
        "Mord mit Aussicht",
        "Hubert ohne Staller",
        "Lauchhammer",
        "Sløborn",
        "Oderbruch",
        "Deutsches Haus",
        "Gestern waren wir noch Kinder",
        "Tschugger",
        "Die Kanzlei",
        "Morden im Norden",
        "WaPo Bodensee",
        "WaPo Berlin"
    ];

    /// <summary>
    /// Default movie topics that index full-length feature films and blockbusters.
    /// </summary>
    public static readonly string[] DefaultBlockbusterMovieTopics =
    [
        "Filme",
        "Spielfilm",
        "Fernsehfilm",
        "Kino"
    ];

    private readonly ILogger<VirtualLibrarySyncService> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IMediathekViewApiClient _apiClient;
    private readonly IVideoParser _videoParser;
    private readonly ILibraryManager? _libraryManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualLibrarySyncService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="apiClient">The MediathekView API client.</param>
    /// <param name="videoParser">The video parser.</param>
    /// <param name="libraryManager">The library manager.</param>
    public VirtualLibrarySyncService(
        ILogger<VirtualLibrarySyncService> logger,
        IConfigurationProvider configurationProvider,
        IMediathekViewApiClient apiClient,
        IVideoParser videoParser,
        ILibraryManager? libraryManager = null)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _apiClient = apiClient;
        _videoParser = videoParser;
        _libraryManager = libraryManager;
    }

    /// <inheritdoc />
    public async Task<int> SyncVirtualLibraryAsync(CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null || !config.VirtualLibrary.EnableVirtualLibrary)
        {
            _logger.LogDebug("Virtual library sync is disabled in configuration.");
            return 0;
        }

        var rootPath = config.VirtualLibrary.VirtualLibraryPath;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            _logger.LogWarning("Virtual library path is not configured.");
            return 0;
        }

        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath);
        }

        CleanupLegacyAccessibilityDirectories(rootPath);

        var topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Curated popular series and blockbuster movie topics
        if (config.VirtualLibrary.CuratedPopularOnly)
        {
            foreach (var series in DefaultPopularSeries)
            {
                topics.Add(series);
            }

            foreach (var movieTopic in DefaultBlockbusterMovieTopics)
            {
                topics.Add(movieTopic);
            }
        }

        // 2. Custom topics
        if (!string.IsNullOrWhiteSpace(config.VirtualLibrary.CustomTopics))
        {
            foreach (var t in config.VirtualLibrary.CustomTopics.Split(TopicSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                topics.Add(t);
            }
        }

        // 3. Subscription topics
        if (config.VirtualLibrary.IncludeSubscriptions)
        {
            foreach (var sub in config.Subscriptions)
            {
                if (!string.IsNullOrWhiteSpace(sub.Name))
                {
                    topics.Add(sub.Name);
                }
            }
        }

        if (topics.Count == 0)
        {
            _logger.LogInformation("No topics configured for virtual library sync.");
            return 0;
        }

        var totalItems = 0;
        var index = 0;

        foreach (var topic in topics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            index++;
            progress?.Report((double)index / topics.Count * 100);

            try
            {
                var synced = await SyncTopicAsync(topic, rootPath, cancellationToken).ConfigureAwait(false);
                totalItems += synced;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync virtual library topic '{Topic}'.", topic);
            }
        }

        if (totalItems > 0 && config.VirtualLibrary.AutoTriggerLibraryScan)
        {
            _libraryManager?.QueueLibraryScan();
        }

        return totalItems;
    }

    /// <inheritdoc />
    public async Task<int> SyncTopicAsync(string topic, string rootPath, CancellationToken cancellationToken)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        var minDuration = (config?.VirtualLibrary.MinDurationMinutes ?? 10) * 60;
        var minMovieDuration = (config?.VirtualLibrary.MinMovieDurationMinutes ?? 75) * 60;

        var queryDto = new QueryFieldsDto
        {
            Query = topic
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
            _logger.LogDebug(ex, "Could not fetch items from Mediathek for topic '{Topic}'.", topic);
            return 0;
        }

        if (result.Results.Count == 0)
        {
            return 0;
        }

        var seriesDir = Path.Combine(rootPath, "series");
        var moviesDir = Path.Combine(rootPath, "movies");

        var synced = 0;

        foreach (var item in result.Results)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var videoUrl = item.GetVideoByQuality(3)?.Url;
            if (item.Duration < TimeSpan.FromSeconds(minDuration) || string.IsNullOrWhiteSpace(videoUrl))
            {
                continue;
            }

            var videoInfo = _videoParser.ParseVideoInfo(item.Topic, item.Title);

            var allowAd = config?.SubscriptionDefaults.AccessibilitySettings?.AllowAudioDescription == true;
            var allowSign = config?.SubscriptionDefaults.AccessibilitySettings?.AllowSignLanguage == true;

            if (AccessibilityFilterHelper.ShouldFilterOut(item.Title, videoInfo, allowAd, allowSign))
            {
                continue;
            }

            if (videoInfo?.IsTrailer == true || videoInfo?.IsInterview == true)
            {
                continue;
            }

            if (videoInfo?.SeasonNumber.HasValue == true && videoInfo.EpisodeNumber.HasValue)
            {
                // Series Episode
                var showTitle = SanitizePath(item.Topic);

                // Skip series that are already present in primary local TV libraries (merging handles them directly)
                if (config != null && IsSeriesLocallyPresent(showTitle, config))
                {
                    continue;
                }

                var showDir = Path.Combine(seriesDir, showTitle);
                var seasonDir = Path.Combine(showDir, $"Season {videoInfo.SeasonNumber.Value:D2}");
                Directory.CreateDirectory(seasonDir);

                // Write tvshow.nfo if missing
                var tvshowNfo = Path.Combine(showDir, "tvshow.nfo");
                if (!File.Exists(tvshowNfo))
                {
                    var showNfoContent = BuildSeriesNfo(item.Topic, item.Description);
                    await File.WriteAllTextAsync(tvshowNfo, showNfoContent, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                }

                var fileName = $"{showTitle} - S{videoInfo.SeasonNumber.Value:D2}E{videoInfo.EpisodeNumber.Value:D2} - {SanitizePath(videoInfo.Title)}";
                var strmPath = Path.Combine(seasonDir, $"{fileName}.strm");
                var nfoPath = Path.Combine(seasonDir, $"{fileName}.nfo");

                if (!File.Exists(strmPath))
                {
                    await File.WriteAllTextAsync(strmPath, videoUrl, cancellationToken).ConfigureAwait(false);
                    var epNfo = BuildEpisodeNfo(videoInfo.Title, item.Description, videoInfo.SeasonNumber.Value, videoInfo.EpisodeNumber.Value, item.Timestamp.DateTime, videoUrl);
                    await File.WriteAllTextAsync(nfoPath, epNfo, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                    synced++;
                }
            }
            else
            {
                // Standalone Movie / Feature
                if (config?.VirtualLibrary.CuratedPopularOnly == true && item.Duration < TimeSpan.FromSeconds(minMovieDuration))
                {
                    continue;
                }

                var (cleanTitle, year) = ParseMovieTitleAndYear(item.Title, item.Timestamp);

                // Skip if movie is already present in primary local movie libraries
                if (config != null && IsMovieLocallyPresent(cleanTitle, config))
                {
                    continue;
                }

                var movieTitle = year.HasValue ? $"{SanitizePath(cleanTitle)} ({year.Value})" : SanitizePath(cleanTitle);
                var movieFolder = Path.Combine(moviesDir, movieTitle);
                Directory.CreateDirectory(movieFolder);

                var strmPath = Path.Combine(movieFolder, $"{movieTitle}.strm");
                var nfoPath = Path.Combine(movieFolder, $"{movieTitle}.nfo");

                if (!File.Exists(strmPath))
                {
                    await File.WriteAllTextAsync(strmPath, videoUrl, cancellationToken).ConfigureAwait(false);
                    var movNfo = BuildMovieNfo(cleanTitle, item.Description, year ?? item.Timestamp.Year, videoUrl);
                    await File.WriteAllTextAsync(nfoPath, movNfo, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                    synced++;
                }
            }
        }

        _logger.LogInformation("Synced {Count} virtual items for topic '{Topic}'.", synced, topic);
        return synced;
    }

    private static string SanitizePath(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
    }

    private static string BuildSeriesNfo(string title, string? plot)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        sb.AppendLine("<tvshow>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <title>{WebUtility.HtmlEncode(title)}</title>");
        if (!string.IsNullOrWhiteSpace(plot))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <plot>{WebUtility.HtmlEncode(plot)}</plot>");
        }

        sb.AppendLine("</tvshow>");
        return sb.ToString();
    }

    private static string BuildEpisodeNfo(string title, string? plot, int season, int episode, DateTime aired, string webUrl)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        sb.AppendLine("<episodedetails>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <title>{WebUtility.HtmlEncode(title)}</title>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <season>{season}</season>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <episode>{episode}</episode>");
        if (aired.Year > 1900)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <aired>{aired:yyyy-MM-dd}</aired>");
        }

        if (!string.IsNullOrWhiteSpace(plot))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <plot>{WebUtility.HtmlEncode(plot)}</plot>");
        }

        if (!string.IsNullOrWhiteSpace(webUrl))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <uniqueid type=\"MediathekWebUrl\">{WebUtility.HtmlEncode(webUrl)}</uniqueid>");
        }

        sb.AppendLine("</episodedetails>");
        return sb.ToString();
    }

    private static string BuildMovieNfo(string title, string? plot, int year, string webUrl)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        sb.AppendLine("<movie>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <title>{WebUtility.HtmlEncode(title)}</title>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <year>{year}</year>");
        if (!string.IsNullOrWhiteSpace(plot))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <plot>{WebUtility.HtmlEncode(plot)}</plot>");
        }

        if (!string.IsNullOrWhiteSpace(webUrl))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <uniqueid type=\"MediathekWebUrl\">{WebUtility.HtmlEncode(webUrl)}</uniqueid>");
        }

        sb.AppendLine("</movie>");
        return sb.ToString();
    }

    private void CleanupLegacyAccessibilityDirectories(string rootPath)
    {
        var dirsToScan = new[]
        {
            Path.Combine(rootPath, "movies"),
            Path.Combine(rootPath, "series")
        };

        foreach (var baseDir in dirsToScan)
        {
            if (!Directory.Exists(baseDir))
            {
                continue;
            }

            try
            {
                foreach (var dir in Directory.GetDirectories(baseDir))
                {
                    var dirName = Path.GetFileName(dir);
                    if (AccessibilityFilterHelper.IsAudioDescription(dirName) || AccessibilityFilterHelper.IsSignLanguage(dirName))
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            _logger.LogInformation("Removed legacy accessibility directory: {Path}", dir);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to remove legacy accessibility directory: {Path}", dir);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error scanning for legacy accessibility directories in {Path}", baseDir);
            }
        }
    }

    private static (string Title, int? Year) ParseMovieTitleAndYear(string rawTitle, DateTimeOffset broadcastDate)
    {
        // Check for broadcaster pattern: "- Spielfilm, Deutschland 2020" or "- Spielfilm, USA 1979_2019"
        var broadMatch = Regex.Match(rawTitle, @"\s*-\s*(?:Spielfilm|Fernsehfilm|Kino|Film).*?([12][90]\d{2})", RegexOptions.IgnoreCase);
        if (broadMatch.Success && int.TryParse(broadMatch.Groups[1].Value, out var yearVal))
        {
            var cleaned = Regex.Replace(rawTitle, @"\s*-\s*(?:Spielfilm|Fernsehfilm|Kino|Film).*$", string.Empty, RegexOptions.IgnoreCase).Trim();
            return (cleaned, yearVal);
        }

        // Check for (YYYY) in title: e.g. "Cash (2024)"
        var parenMatch = Regex.Match(rawTitle, @"\((\d{4})\)");
        if (parenMatch.Success && int.TryParse(parenMatch.Groups[1].Value, out var pYear))
        {
            var cleaned = Regex.Replace(rawTitle, @"\s*\(\d{4}\)", string.Empty).Trim();
            return (cleaned, pYear);
        }

        return (rawTitle.Trim(), null);
    }

    private bool IsSeriesLocallyPresent(string showTitle, PluginConfiguration config)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(config.Paths.DefaultSubscriptionShowPath))
        {
            candidates.Add(config.Paths.DefaultSubscriptionShowPath);
        }

        if (!string.IsNullOrWhiteSpace(config.Paths.DefaultManualShowPath))
        {
            candidates.Add(config.Paths.DefaultManualShowPath);
        }

        var virtRoot = config.VirtualLibrary?.VirtualLibraryPath;

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
                            if (!string.IsNullOrWhiteSpace(virtRoot) && loc.StartsWith(virtRoot, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            candidates.Add(loc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to query library virtual folders for series deduplication.");
            }
        }

        foreach (var dir in candidates)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            var target = Path.Combine(dir, showTitle);
            if (Directory.Exists(target))
            {
                return true;
            }

            try
            {
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var name = Path.GetFileName(sub);
                    if (string.Equals(name, showTitle, StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith(showTitle + " ", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith(showTitle + "(", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore enumeration errors
            }
        }

        return false;
    }

    private bool IsMovieLocallyPresent(string movieTitle, PluginConfiguration config)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(config.Paths.DefaultSubscriptionMoviePath))
        {
            candidates.Add(config.Paths.DefaultSubscriptionMoviePath);
        }

        if (!string.IsNullOrWhiteSpace(config.Paths.DefaultManualMoviePath))
        {
            candidates.Add(config.Paths.DefaultManualMoviePath);
        }

        var virtRoot = config.VirtualLibrary?.VirtualLibraryPath;

        if (_libraryManager != null)
        {
            try
            {
                foreach (var vf in _libraryManager.GetVirtualFolders())
                {
                    if (string.Equals(vf.CollectionType?.ToString(), "movies", StringComparison.OrdinalIgnoreCase) && vf.Locations != null)
                    {
                        foreach (var loc in vf.Locations)
                        {
                            if (!string.IsNullOrWhiteSpace(virtRoot) && loc.StartsWith(virtRoot, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            candidates.Add(loc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to query library virtual folders for movie deduplication.");
            }
        }

        foreach (var dir in candidates)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            var target = Path.Combine(dir, movieTitle);
            if (Directory.Exists(target))
            {
                return true;
            }

            try
            {
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var name = Path.GetFileName(sub);
                    if (string.Equals(name, movieTitle, StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith(movieTitle + " ", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith(movieTitle + "(", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore enumeration errors
            }
        }

        return false;
    }
}
