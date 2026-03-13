using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
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
    private readonly Regex _clearLangRegex;
    private readonly Regex _trailerRegex;
    private readonly Regex _interviewRegex;

    // Regex for Number Parsing
    private readonly List<Regex> _seasonEpisodePatterns;
    private readonly List<Regex> _absoluteNumberingPatterns;
    private readonly List<Regex> _seasonOnlyPatterns;
    private readonly Regex _dateRegex;

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
        _adRegex = BuildPattern(@"\b(AD|Audiodeskription|Hörfassung)\b");

        // Compile regex for Sign Language
        _gsRegex = BuildPattern(@"\b(GS|Gebärdensprache|Gebärdendolmetscher)\b");

        // Compile regex for ClearLanguage / Klare Sprache
        _clearLangRegex = BuildPattern(@"\b(Video in )?klarer? Sprache\b");

        // Compile regex for Trailer
        _trailerRegex = BuildPattern(@"(?:\bTrailer\b)|^(?:Darum geht's)|(?:Darum geht's)$");

        // Compile regex for Interview
        _interviewRegex = BuildPattern(@"\b(Interview)\b");

        // Compile regex for Date (DD.MM.YYYY or D.M.YY)
        _dateRegex = BuildPattern(@"\b\d{1,2}\.\d{1,2}\.\d{2,4}\b");

        // Compile regex patterns for Normal Numbering (SXXEXX, SXX/EXX, Staffel X Episode Y, XxY, (SXX/EXX), (Staffel X, Folge Y))
        _seasonEpisodePatterns = BuildPatterns([
            @"(Staffel|Season|S)[\s/,_]?(?<season>\d+)[\s/,_]{0,3}(Episode|Folge|E)[\s/,_]?(?<episode>\d+)",
            // X-Notation: 1x01, 1X01
            @"(?<season>\d+)\s*[xX]\s*(?<episode>\d+)",
        ]);

        // Compile regex patterns for Absolute Numbering (Folge ZZZ, (ZZZ), ZZZ.)
        _absoluteNumberingPatterns = BuildPatterns([
            // "123. " - Number at start
            @"^\s*(?<absolute>\d+)\.\s*",
            // "Folge 123"
            @"(?:Folge\s*(?<absolute>\d+)\b)",
            // "(123)" - Number inside brackets
            @"(?<=\()\s*(?<absolute>\d+)\s*(?=\))",
        ]);

        // Compile regex patterns for Season Only Numbering
        _seasonOnlyPatterns = BuildPatterns(
        [
            @"(?:^|\s)(?:Staffel|Season)\s*(?<season>\d+)\b",
            // "(S1)"
            @"[\(\[]S(?<season>\d+)[\)\]]",
        ]);
    }

    /// <summary>
    /// Parses video information (season, episode, title, language, features) from a given media title.
    /// </summary>
    /// <param name="topic">The name of the topic the Video belongs to.</param>
    /// <param name="mediaTitle">The title of the media item from the API.</param>
    /// <returns>An <see cref="VideoInfo"/> object if parsing is successful, otherwise null.</returns>
    public VideoInfo? ParseVideoInfo(string? topic, string mediaTitle)
    {
        var ctx = new ParsingContext(mediaTitle, topic ?? string.Empty);

        // Language Detection
        var languageDetectionResult = _languageDetectionService.DetectLanguage(mediaTitle);
        ctx.Result.Language = languageDetectionResult.LanguageCode;
        ctx.UpdateTitle(languageDetectionResult.CleanedTitle);

        // Read Bool Flags
        CheckFlag(ctx, _adRegex, v => v.HasAudiodescription = true);
        CheckFlag(ctx, _gsRegex, v => v.HasSignLanguage = true);
        CheckFlag(ctx, _clearLangRegex, v => v.HasClearLanguage = true);
        CheckFlag(ctx, _trailerRegex, v => v.IsTrailer = true, updateTitle: false);
        CheckFlag(ctx, _interviewRegex, v => v.IsInterview = true, updateTitle: false);

        // Season Episode Parsing (Normal Season/Episode)
        TryExtractPattern(ctx, _seasonEpisodePatterns, (collection, info) =>
        {
            if (int.TryParse(collection["season"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int season) &&
                int.TryParse(collection["episode"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int episode))
            {
                info.SeasonNumber = season;
                info.EpisodeNumber = episode;
                info.IsShow = true;
            }
        });

        TryExtractPattern(ctx, _absoluteNumberingPatterns, (collection, info) =>
        {
            if (int.TryParse(collection["absolute"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int absolute))
            {
                info.AbsoluteEpisodeNumber = absolute;
                info.IsShow = true;
            }
        });

        // Season Only Detection (if not already found)
        if (!ctx.Result.SeasonNumber.HasValue)
        {
            TryExtractPattern(ctx, _seasonOnlyPatterns, (collection, info) =>
            {
                if (int.TryParse(collection["season"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int season))
                {
                    info.SeasonNumber = season;
                    info.IsShow = true;
                }
            });
        }

        if (!ctx.Result.IsShow)
        {
            CheckFlag(ctx, _dateRegex, v => v.IsShow = true, false);
        }

        ctx.FinalizeTitle();
        return ctx.Result;
    }

    /// <summary>
    /// Helper method to remove a detected tag and its surrounding delimiters from the title.
    /// </summary>
    /// <param name="title">The original title string.</param>
    /// <param name="tagToRemove">The exact string value of the tag that was matched.</param>
    /// <returns>The title with the tag and its delimiters removed.</returns>
    private static string CleanTagFromTitle(string title, string tagToRemove)
    {
        // This regex looks for the exact tag and removes it along with surrounding delimiters (paren, brackets, spaces).
        string pattern = @$"\s*([\(\[])?\s*{Regex.Escape(tagToRemove)}\s*([\)\]])?\s*";
        string cleaned = Regex.Replace(title, pattern, " ", RegexOptions.IgnoreCase);

        // Collapse multiple spaces and trim
        return Regex.Replace(cleaned, @"\s{2,}", " ")
            .Trim();
    }

    private List<Regex> BuildPatterns([StringSyntax(StringSyntaxAttribute.Regex)] IEnumerable<string> patternStrings)
    {
        return patternStrings.Select(BuildPattern).ToList();
    }

    private Regex BuildPattern([StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
    {
        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }

    private void CheckFlag(ParsingContext ctx, Regex regex, Action<VideoInfo> setter, bool updateTitle = true)
    {
        var match = regex.Match(ctx.CurrentTitle);
        if (match.Success)
        {
            setter(ctx.Result);
            if (updateTitle)
            {
                ctx.UpdateTitle(CleanTagFromTitle(ctx.CurrentTitle, match.Value));
            }
        }
    }

    private bool TryExtractPattern(
        ParsingContext ctx,
        IEnumerable<Regex> patterns,
        Action<GroupCollection, VideoInfo> applyResult)
    {
        foreach (var pattern in patterns)
        {
            var match = pattern.Match(ctx.CurrentTitle);
            if (match.Success)
            {
                // Apply Changes defined in action.
                applyResult(match.Groups, ctx.Result);

                // Alwas update the title
                ctx.UpdateTitle(CleanTagFromTitle(ctx.CurrentTitle, match.Value));
                return true; // Erfolg: Suche für diese Kategorie beenden
            }
        }

        return false;
    }

    private sealed class ParsingContext
    {
        public ParsingContext(string originalTitle, string topic)
        {
            Result = new VideoInfo { Title = originalTitle, Topic = topic };
            CurrentTitle = originalTitle;
            Topic = topic;
        }

        public VideoInfo Result { get; }

        public string CurrentTitle { get; private set; }

        public string Topic { get; }

        public void UpdateTitle(string newTitle) => CurrentTitle = newTitle;

        public void FinalizeTitle()
        {
            string cleaned = CurrentTitle;

            // Final title cleanup (topic removal, general cleanup)
            // Try to remove common series names/prefixes that might still be in the title
            if (!string.IsNullOrWhiteSpace(Topic))
            {
                if (cleaned.StartsWith(Topic, StringComparison.OrdinalIgnoreCase))
                {
                    var remaining = cleaned.Substring(Topic.Length);
                    // Manually trim start chars that match [\s:_-]
                    cleaned = remaining.TrimStart(' ', '\t', ':', '_', '-');
                }
            }

            // General cleanup for episode title
            // Remove date patterns like "· 13.06.13 |" only if we have other numbering
            if (Result.HasSeasonEpisodeNumbering || Result.HasAbsoluteNumbering)
            {
                cleaned = Regex.Replace(cleaned, @"\s*[\·\|\-]\s*\d{2}\.\d{2}\.\d{2,4}\s*[\·\|\-]?\s*", " ", RegexOptions.IgnoreCase);
            }

            // Remove "Folge NNN: " prefix for normal numbering and similar prefixes
            cleaned = Regex.Replace(cleaned, @"^(?:Folge\s*\d+:\s*)", string.Empty, RegexOptions.IgnoreCase);
            // Less aggressive period/underscore replacement
            cleaned = Regex.Replace(cleaned, @"(_+|\.{2,})", " ");
            // Remove trailing dashes and colons, and leading/trailing spaces
            cleaned = Regex.Replace(cleaned, @"^[\s\-:–\|·]+|[\s\-:–\|·]+$", string.Empty);
            // Collapse multiple spaces
            cleaned = Regex.Replace(cleaned, @"\s{2,}", " ").Trim();

            Result.Title = string.IsNullOrWhiteSpace(cleaned) ? Result.Title : cleaned;
        }
    }
}
