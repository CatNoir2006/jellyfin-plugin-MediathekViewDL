using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Helper class for parsing video information from media titles.
/// </summary>
public class VideoParser : IVideoParser
{
    private readonly ILogger<VideoParser> _logger;
    private readonly ILanguageDetectionService _languageDetectionService;

    // Regex for Audiodescription and Sign Language
    private readonly Regex _adRegex;
    private readonly Regex _gsRegex;
    private readonly Regex _trailerRegex;
    private readonly Regex _interviewRegex;

    // Regex for Number Parsing
    private readonly List<Regex> _seasonEpisodePatterns;
    private readonly List<Regex> _absoluteNumberingPatterns;
    private readonly List<Regex> _seasonOnlyPatterns;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoParser"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="languageDetectionService">The language detection service.</param>
    public VideoParser(ILogger<VideoParser> logger, ILanguageDetectionService languageDetectionService)
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

        // Compile regex for Trailer
        _trailerRegex = new Regex(
            @"(?:\bTrailer\b)|^(?:Darum geht's)|(?:Darum geht's)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        // Compile regex for Interview
        _interviewRegex = new Regex(
            @"\b(Interview)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        // Compile regex patterns for Normal Numbering (SXXEXX, SXX/EXX, Staffel X Episode Y, XxY, (SXX/EXX), (Staffel X, Folge Y))
        _seasonEpisodePatterns = new List<Regex>
        {
            // Standard: s01e01, staffel 1 episode 1, s01/e01, s01_e01
            new Regex(@"(?:s|staffel)[\s_]*(?<season>\d+)[\s_]*(?:e|episode|/)[\s_]*(?<episode>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),

            // X-Notation: 1x01, 1X01
            new Regex(@"(?<season>\d+)\s*[xX]\s*(?<episode>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),

            // Brackets: (s01e01), [s01/e01]
            new Regex(@"[\(\[]s?\s*(?<season>\d+)\s*(?:e|/)\s*(?<episode>\d+)[\)\]]", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),

            // German Verbose with comma: Staffel 1, Folge 1, (Staffel 1, Folge 1)
            new Regex(@"\s*[\(\[]?Staffel\s*(?<season>\d+),\s*Folge\s*(?<episode>\d+)[\)\]]?", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),

            // Alternative S/E: (S1 Episode 1), S01 Episode 01
            new Regex(@"\s*[\(\[]?S(?<season>\d+)(?:[\s\/]*E|Episode\s*)(?<episode>\d+)[\)\]]?", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1))
        };
        // Compile regex patterns for Absolute Numbering (Folge ZZZ, (ZZZ), ZZZ.)
        _absoluteNumberingPatterns = new List<Regex>
        {
            // "Folge 123"
            new Regex(@"(?:Folge\s*(?<absolute>\d+)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),

            // "(123)" - Number inside brackets
            new Regex(@"(?<=\()\s*(?<absolute>\d+)\s*(?=\))", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),

            // "123. " - Number at start
            new Regex(@"^\s*(?<absolute>\d+)\.\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1))
        };

        // Compile regex patterns for Season Only Numbering
        _seasonOnlyPatterns = new List<Regex>
        {
            // "Staffel 1"
            new Regex(@"(?:^|\s)Staffel\s*(?<season>\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),
            // "Season 1"
            new Regex(@"(?:^|\s)Season\s*(?<season>\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1)),
             // "(S1)"
            new Regex(@"[\(\[]S(?<season>\d+)[\)\]]", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1))
        };
    }

    /// <summary>
    /// Parses video information (season, episode, title, language, features) from a given media title.
    /// </summary>
    /// <param name="topic">The name of the topic the Video belongs to.</param>
    /// <param name="mediaTitle">The title of the media item from the API.</param>
    /// <returns>An <see cref="VideoInfo"/> object if parsing is successful, otherwise null.</returns>
    public VideoInfo? ParseVideoInfo(string? topic, string mediaTitle)
    {
        var videoInfo = new VideoInfo
        {
            Title = mediaTitle, // Initialize with original title as fallback
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

        // 4. Season Episode Parsing (Normal Season/Episode)
        foreach (var pattern in _seasonEpisodePatterns)
        {
            var match = pattern.Match(processedMediaTitle);
            if (match.Success)
            {
                if (int.TryParse(match.Groups["season"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int season) &&
                    int.TryParse(match.Groups["episode"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int episode))
                {
                    videoInfo.SeasonNumber = season;
                    videoInfo.EpisodeNumber = episode;
                    videoInfo.IsShow = true;
                    processedMediaTitle = CleanTagFromTitle(processedMediaTitle, match.Value);
                    break; // Stop after first match
                }
            }
        }

        // 5. Absolute Episode Numbering Parsing
        foreach (var pattern in _absoluteNumberingPatterns)
        {
            var match = pattern.Match(processedMediaTitle);
            if (match.Success)
            {
                if (int.TryParse(match.Groups["absolute"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int absolute))
                {
                    videoInfo.AbsoluteEpisodeNumber = absolute;
                    videoInfo.IsShow = true;
                    processedMediaTitle = CleanTagFromTitle(processedMediaTitle, match.Value);
                    break; // Stop after first match
                }
            }
        }

        // 6. Trailer Detection
        var trailerMatch = _trailerRegex.Match(processedMediaTitle);
        if (trailerMatch.Success)
        {
            videoInfo.IsTrailer = true;
        }

        // 7. Interview Detection
        var interviewMatch = _interviewRegex.Match(processedMediaTitle);
        if (interviewMatch.Success)
        {
            videoInfo.IsInterview = true;
        }

        // 8. Season Only Detection (if not already found)
        if (!videoInfo.SeasonNumber.HasValue)
        {
            foreach (var pattern in _seasonOnlyPatterns)
            {
                var match = pattern.Match(processedMediaTitle);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups["season"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int season))
                    {
                        videoInfo.SeasonNumber = season;
                        videoInfo.IsShow = true; // Implies it belongs to a show structure
                        processedMediaTitle = CleanTagFromTitle(processedMediaTitle, match.Value);
                        break; // Stop after first match
                    }
                }
            }
        }

        // Final title cleanup (topic removal, general cleanup)
        // Try to remove common series names/prefixes that might still be in the title
        if (!string.IsNullOrWhiteSpace(topic))
        {
            if (processedMediaTitle.StartsWith(topic, StringComparison.OrdinalIgnoreCase))
            {
                var remaining = processedMediaTitle.Substring(topic.Length);
                // Manually trim start chars that match [\s:_-]
                processedMediaTitle = remaining.TrimStart(' ', '\t', ':', '_', '-');
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

        videoInfo.Title = string.IsNullOrWhiteSpace(processedMediaTitle) ? mediaTitle : processedMediaTitle;

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
