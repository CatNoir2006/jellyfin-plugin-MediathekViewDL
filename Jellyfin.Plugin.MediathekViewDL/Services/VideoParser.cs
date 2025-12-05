using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Helper class for parsing video information from media titles.
/// </summary>
public class VideoParser
{
    private readonly ILogger<VideoParser> _logger;
    private readonly LanguageDetectionService _languageDetectionService;

    // Regex for Audiodescription and Sign Language
    private readonly Regex _adRegex;
    private readonly Regex _gsRegex;

    // Regex for Number Parsing
    private readonly Regex _normalNumberingRegex;
    private readonly Regex _absoluteNumberingRegex;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoParser"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="languageDetectionService">The language detection service.</param>
    public VideoParser(ILogger<VideoParser> logger, LanguageDetectionService languageDetectionService)
    {
        _logger = logger;
        _languageDetectionService = languageDetectionService;

        // Compile regex for Audiodescription
        _adRegex = new Regex(
            @"\b(AD|Audiodeskription|Hörfassung)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        // Compile regex for Sign Language
        _gsRegex = new Regex(
            @"\b(GS|Gebärdensprache|Gebärdendolmetscher)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        // Compile regex for Normal Numbering (SXXEXX, SXX/EXX, Staffel X Episode Y, XxY, (SXX/EXX), (Staffel X, Folge Y))
        _normalNumberingRegex = new Regex(
            @"(?:s|staffel)\s*(?<season_n1>\d+)\s*(?:e|episode|/)\s*(?<episode_n1>\d+)|(?<season_x1>\d+)\s*[xX]\s*(?<episode_x1>\d+)|[\(\[]s?\s*(?<season_n2>\d+)\s*(?:e|/)\s*(?<episode_n2>\d+)[\)\]]|\s*(?:[\(\[]?Staffel\s*(?<season_s1>\d+),\s*Folge\s*(?<episode_f1>\d+)[\)\]]?|[\(\[]?S(?<season_s2>\d+)(?:[\s\/]*E|Episode\s*)(?<episode_e2>\d+)[\)\]]?)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        // Compile regex for Absolute Numbering (Folge ZZZ, (ZZZ), ZZZ.)
        _absoluteNumberingRegex = new Regex(
            @"(?:Folge\s*(?<abs1>\d+)\b)|(?<=\()\s*(?<abs2>\d+)\s*(?=\))|^\s*(?<abs3>\d+)\.\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Parses video information (season, episode, title, language, features) from a given media title.
    /// </summary>
    /// <param name="subscriptionName">The name of the subscription, used as a hint for series name.</param>
    /// <param name="mediaTitle">The title of the media item from the API.</param>
    /// <param name="enforceParsing">If true, returns null if no numbering (season/episode or absolute) can be parsed.</param>
    /// <returns>An <see cref="VideoInfo"/> object if parsing is successful, otherwise null.</returns>
    public VideoInfo? ParseVideoInfo(string subscriptionName, string mediaTitle, bool enforceParsing)
    {
        var videoInfo = new VideoInfo
        {
            EpisodeTitle = mediaTitle, // Initialize with original title as fallback
        };
        string processedMediaTitle = mediaTitle;

        // 1. Language Detection
        var languageDetectionResult = _languageDetectionService.DetectLanguage(processedMediaTitle);
        videoInfo.Language = languageDetectionResult.LanguageCode;
        processedMediaTitle = languageDetectionResult.CleanedTitle;

        // 2. Audiodescription Detection
        var adMatch = _adRegex.Match(processedMediaTitle);
        if (adMatch.Success)
        {
            videoInfo.HasAudiodescription = true;
            processedMediaTitle = CleanTagFromTitle(processedMediaTitle, adMatch.Value);
        }

        // 3. Sign Language Detection
        var gsMatch = _gsRegex.Match(processedMediaTitle);
        if (gsMatch.Success)
        {
            videoInfo.HasSignLanguage = true;
            processedMediaTitle = CleanTagFromTitle(processedMediaTitle, gsMatch.Value);
        }

        // 4. Number Parsing (Normal Season/Episode)
        var normalMatch = _normalNumberingRegex.Match(processedMediaTitle);
        if (normalMatch.Success)
        {
            // Try to parse from the first successful group.
            if (normalMatch.Groups["season_n1"].Success && normalMatch.Groups["episode_n1"].Success)
            {
                videoInfo.SeasonNumber = int.Parse(normalMatch.Groups["season_n1"].Value, CultureInfo.InvariantCulture);
                videoInfo.EpisodeNumber = int.Parse(normalMatch.Groups["episode_n1"].Value, CultureInfo.InvariantCulture);
            }
            else if (normalMatch.Groups["season_x1"].Success && normalMatch.Groups["episode_x1"].Success)
            {
                videoInfo.SeasonNumber = int.Parse(normalMatch.Groups["season_x1"].Value, CultureInfo.InvariantCulture);
                videoInfo.EpisodeNumber = int.Parse(normalMatch.Groups["episode_x1"].Value, CultureInfo.InvariantCulture);
            }
            else if (normalMatch.Groups["season_n2"].Success && normalMatch.Groups["episode_n2"].Success)
            {
                videoInfo.SeasonNumber = int.Parse(normalMatch.Groups["season_n2"].Value, CultureInfo.InvariantCulture);
                videoInfo.EpisodeNumber = int.Parse(normalMatch.Groups["episode_n2"].Value, CultureInfo.InvariantCulture);
            }
            else if (normalMatch.Groups["season_s1"].Success && normalMatch.Groups["episode_f1"].Success) // New pattern: Staffel X, Folge Y
            {
                videoInfo.SeasonNumber = int.Parse(normalMatch.Groups["season_s1"].Value, CultureInfo.InvariantCulture);
                videoInfo.EpisodeNumber = int.Parse(normalMatch.Groups["episode_f1"].Value, CultureInfo.InvariantCulture);
            }
            else if (normalMatch.Groups["season_s2"].Success && normalMatch.Groups["episode_e2"].Success) // New pattern: SXXEXX, SXX/EXX
            {
                videoInfo.SeasonNumber = int.Parse(normalMatch.Groups["season_s2"].Value, CultureInfo.InvariantCulture);
                videoInfo.EpisodeNumber = int.Parse(normalMatch.Groups["episode_e2"].Value, CultureInfo.InvariantCulture);
            }

            if (videoInfo.SeasonNumber.HasValue && videoInfo.EpisodeNumber.HasValue)
            {
                videoInfo.IsShow = true; // If S/E numbers are found, it's a show.
                videoInfo.IsParsed = true;
                processedMediaTitle = CleanTagFromTitle(processedMediaTitle, normalMatch.Value);
            }
        }

        // 5. Absolute Numbering (only if normal S/E was not found)
        if (!videoInfo.IsParsed)
        {
            var absoluteMatch = _absoluteNumberingRegex.Match(processedMediaTitle);
            if (absoluteMatch.Success)
            {
                // Try to parse from the first successful group.
                int? absoluteNum = null;
                if (absoluteMatch.Groups["abs1"].Success)
                {
                    absoluteNum = int.Parse(absoluteMatch.Groups["abs1"].Value, CultureInfo.InvariantCulture);
                }
                else if (absoluteMatch.Groups["abs2"].Success)
                {
                    absoluteNum = int.Parse(absoluteMatch.Groups["abs2"].Value, CultureInfo.InvariantCulture);
                }
                else if (absoluteMatch.Groups["abs3"].Success)
                {
                    absoluteNum = int.Parse(absoluteMatch.Groups["abs3"].Value, CultureInfo.InvariantCulture);
                }

                if (absoluteNum.HasValue)
                {
                    videoInfo.AbsoluteEpisodeNumber = absoluteNum;
                    videoInfo.IsParsed = true;
                    processedMediaTitle = CleanTagFromTitle(processedMediaTitle, absoluteMatch.Value);
                }
            }
        }

        // Final title cleanup (subscription name removal, general cleanup)
        // Try to remove common series names/prefixes that might still be in the title
        if (!string.IsNullOrWhiteSpace(subscriptionName))
        {
            var nameWithoutPrefix = Regex.Replace(processedMediaTitle, $"^" + Regex.Escape(subscriptionName) + @"[\s:_-]*", string.Empty, RegexOptions.IgnoreCase).Trim();
            if (!string.IsNullOrWhiteSpace(nameWithoutPrefix))
            {
                processedMediaTitle = nameWithoutPrefix;
            }
        }

        // General cleanup for episode title
        // Remove "u.a. " prefix
        processedMediaTitle = Regex.Replace(processedMediaTitle, @"^u\.a\.\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
        // Remove date patterns like "· 13.06.13 |"
        processedMediaTitle = Regex.Replace(processedMediaTitle, @"\s*[\·\|\-]\s*\d{2}\.\d{2}\.\d{2,4}\s*[\·\|\-]?\s*", " ", RegexOptions.IgnoreCase).Trim();
        // Remove "Folge NNN: " prefix for normal numbering and similar prefixes
        processedMediaTitle = Regex.Replace(processedMediaTitle, @"^(?:Folge\s*\d+:\s*)", string.Empty, RegexOptions.IgnoreCase).Trim();
        // Less aggressive period/underscore replacement
        processedMediaTitle = Regex.Replace(processedMediaTitle, @"(?<!\d)\.(?!\d)|_", " ").Trim();
        // Remove trailing dashes and colons, and leading/trailing spaces
        processedMediaTitle = Regex.Replace(processedMediaTitle, @"^[\s\-:–]+|[\s\-:–]+$", string.Empty).Trim();
        // Collapse multiple spaces
        processedMediaTitle = Regex.Replace(processedMediaTitle, @"\s{2,}", " ").Trim();

        // If parsing enforced and failed (no numbering found)
        if (enforceParsing && !videoInfo.IsParsed)
        {
            _logger.LogWarning("Enforced parsing failed for '{Title}' from subscription '{SubName}'. Skipping item.", mediaTitle, subscriptionName);
            return null;
        }

        videoInfo.EpisodeTitle = string.IsNullOrWhiteSpace(processedMediaTitle) ? mediaTitle : processedMediaTitle;

        return videoInfo;
    }

    /// <summary>
    /// Helper method to remove a detected tag and its surrounding delimiters from the title.
    /// </summary>
    /// <param name="title">The original title string.</param>
    /// <param name="tagToRemove">The exact string value of the tag that was matched.</param>
    /// <returns>The title with the tag and its delimiters removed.</returns>
    private string CleanTagFromTitle(string title, string tagToRemove)
    {
        // This regex looks for the exact tag and removes it along with surrounding delimiters (paren, brackets, spaces).
        // It's important to make sure the tagToRemove is escaped for regex.
        string pattern = @$"\s*([\(\[])?\s*{Regex.Escape(tagToRemove)}\s*([\)\]])?\s*";
        string cleaned = Regex.Replace(title, pattern, " ", RegexOptions.IgnoreCase);
        return Regex.Replace(cleaned, @"\s{2,}", " ").Trim(); // Collapse multiple spaces
    }
}
