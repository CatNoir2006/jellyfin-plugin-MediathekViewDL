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

        CleanupLegacyMovieAndAccessibilityDirectories(rootPath);

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
                var showTitle = SanitizePath(CleanTitlePunctuation(item.Topic));

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
                    var showNfoContent = BuildSeriesNfo(CleanTitlePunctuation(item.Topic), item.Description);
                    await File.WriteAllTextAsync(tvshowNfo, showNfoContent, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                }

                var epCleanTitle = CleanTitlePunctuation(videoInfo.Title);
                var fileName = $"{showTitle} - S{videoInfo.SeasonNumber.Value:D2}E{videoInfo.EpisodeNumber.Value:D2} - {SanitizePath(epCleanTitle)}";
                var strmPath = Path.Combine(seasonDir, $"{fileName}.strm");
                var nfoPath = Path.Combine(seasonDir, $"{fileName}.nfo");

                if (!File.Exists(strmPath))
                {
                    await File.WriteAllTextAsync(strmPath, videoUrl, cancellationToken).ConfigureAwait(false);
                    var epNfo = BuildEpisodeNfo(epCleanTitle, item.Description, videoInfo.SeasonNumber.Value, videoInfo.EpisodeNumber.Value, item.Timestamp.DateTime, videoUrl);
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
                    var movNfo = BuildMovieNfo(cleanTitle, item.Description, year, videoUrl);
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
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <originaltitle>{WebUtility.HtmlEncode(title)}</originaltitle>");
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

    private static string BuildMovieNfo(string title, string? plot, int? year, string webUrl)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        sb.AppendLine("<movie>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <title>{WebUtility.HtmlEncode(title)}</title>");
        sb.AppendLine(CultureInfo.InvariantCulture, $"  <originaltitle>{WebUtility.HtmlEncode(title)}</originaltitle>");
        if (year.HasValue && year.Value >= 1900 && year.Value <= DateTime.UtcNow.Year + 1)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"  <year>{year.Value}</year>");
        }

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

    private void CleanupLegacyMovieAndAccessibilityDirectories(string rootPath)
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
                    var isAccessibility = AccessibilityFilterHelper.IsAudioDescription(dirName) || AccessibilityFilterHelper.IsSignLanguage(dirName);

                    var isDirtyMovie = false;
                    if (baseDir.EndsWith("movies", StringComparison.OrdinalIgnoreCase))
                    {
                        isDirtyMovie = dirName.EndsWith(" (2026)", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("–", StringComparison.Ordinal) ||
                                       dirName.Contains("—", StringComparison.Ordinal) ||
                                       dirName.Contains("‧", StringComparison.Ordinal) ||
                                       dirName.Contains("(klare Sprache)", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("(Originalversion", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("- Spielfilm", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("- Fernsehfilm", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("- Kurzfilm", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("- Film,", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("| Thriller", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("| Krimi", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("| Drama", StringComparison.OrdinalIgnoreCase) ||
                                       dirName.Contains("| Komödie", StringComparison.OrdinalIgnoreCase);
                    }

                    if (isAccessibility || isDirtyMovie)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            _logger.LogInformation("Removed legacy dirty directory: {Path}", dir);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to remove legacy dirty directory: {Path}", dir);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error scanning for legacy directories in {Path}", baseDir);
            }
        }
    }

    /// <summary>
    /// Normalizes punctuation and special Unicode characters in titles to prevent scraper search failures.
    /// </summary>
    /// <param name="title">The raw title string.</param>
    /// <returns>A cleaned title string.</returns>
    internal static string CleanTitlePunctuation(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        // Replace unicode middle dot, en-dash, em-dash, bullet with standard hyphen
        var normalized = title.Replace('‧', '-')
            .Replace('–', '-')
            .Replace('—', '-')
            .Replace('•', '-');

        // Normalize spacing around hyphens
        normalized = Regex.Replace(normalized, @"\s*-\s*", " - ");
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized.Trim(' ', '-', '.', ':');
    }

    /// <summary>
    /// Parses a raw movie title to strip broadcaster artifacts and extract the actual release year.
    /// </summary>
    /// <param name="rawTitle">The raw movie title.</param>
    /// <param name="broadcastDate">The broadcast date.</param>
    /// <returns>A tuple containing the cleaned movie title and an optional release year.</returns>
    internal static (string Title, int? Year) ParseMovieTitleAndYear(string rawTitle, DateTimeOffset broadcastDate)
    {
        if (string.IsNullOrWhiteSpace(rawTitle))
        {
            return (string.Empty, null);
        }

        var cleaned = rawTitle;

        // 1. Strip language/accessibility markers in parentheses or brackets:
        cleaned = Regex.Replace(cleaned, @"\s*[\(\[](?:klare Sprache|Englisch|Französisch|Originalversion[^)\]]*|OmU|OmeU|OV|DF)[\)\]]", string.Empty, RegexOptions.IgnoreCase);

        // 2. Strip pipe categories often added by ARD/ZDF/Arte (e.g. "Zero | Thriller"):
        cleaned = Regex.Replace(cleaned, @"\s*\|\s*(?:Thriller|Krimi|Drama|Komödie|Film|Spielfilm|Dokumentation|Kino|Action|Sci-Fi|Fantasy).*$", string.Empty, RegexOptions.IgnoreCase);

        // 3. Extract release year and country from broadcaster patterns:
        // "- Spielfilm, Deutschland 2020", "- Spielfilm, USA 1979_2019", "- Fernsehfilm Deutschland 2018", "- Film, F 2021"
        int? year = null;
        var broadMatch = Regex.Match(cleaned, @"\s*-\s*(?:Spielfilm|Fernsehfilm|Kurzfilm|Kino|Film|Drama|Komödie|Thriller|Krimi).*?([12][90]\d{2})", RegexOptions.IgnoreCase);
        if (broadMatch.Success && int.TryParse(broadMatch.Groups[1].Value, out var yearVal))
        {
            year = yearVal;
            cleaned = Regex.Replace(cleaned, @"\s*-\s*(?:Spielfilm|Fernsehfilm|Kurzfilm|Kino|Film|Drama|Komödie|Thriller|Krimi).*$", string.Empty, RegexOptions.IgnoreCase);
        }
        else
        {
            cleaned = Regex.Replace(cleaned, @"\s*-\s*(?:Spielfilm|Fernsehfilm|Kurzfilm|Kino).*$", string.Empty, RegexOptions.IgnoreCase);
        }

        // 4. Extract standalone (YYYY) or [YYYY]
        if (!year.HasValue)
        {
            var yearMatch = Regex.Match(cleaned, @"[\(\[]([12][90]\d{2})[\)\]]");
            if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var y))
            {
                year = y;
                cleaned = Regex.Replace(cleaned, @"\s*[\(\[]" + y + @"[\)\]]", string.Empty);
            }
        }

        if (year.HasValue && (year.Value < 1900 || year.Value > DateTime.UtcNow.Year + 1))
        {
            year = null;
        }

        cleaned = CleanTitlePunctuation(cleaned);
        return (cleaned, year);
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
