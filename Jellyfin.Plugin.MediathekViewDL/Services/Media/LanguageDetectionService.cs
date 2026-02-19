using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// A service to detect a language from a string based on various language identifiers.
/// This service pre-compiles regex patterns for performance.
/// </summary>
public class LanguageDetectionService : ILanguageDetectionService
{
    private readonly List<LanguageData> _languageDataList;
    private readonly HashSet<string> _ovIdentifiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "OV", "OmU", "OmeU", "Originalversion", "Originalversion mit Untertitel"
    };

    private readonly Regex _parenthesesRegex;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageDetectionService"/> class.
    /// </summary>
    public LanguageDetectionService()
    {
        var parenthesesPattern = @"\((?<content>[^)]*)\)";
        _parenthesesRegex = new Regex(parenthesesPattern, RegexOptions.Compiled);

        // 2. Build and compile regex for all neutral cultures.
        _languageDataList = new List<LanguageData>();
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
            var neutralCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);

            foreach (var culture in neutralCultures)
            {
                if (string.IsNullOrEmpty(culture.Name) || culture.ThreeLetterISOLanguageName == "ivl")
                {
                    continue;
                }

                var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    culture.EnglishName, culture.DisplayName, culture.NativeName
                };
                identifiers.RemoveWhere(string.IsNullOrWhiteSpace);
                if (identifiers.Count == 0)
                {
                    continue;
                }

                _languageDataList.Add(new LanguageData
                {
                    ThreeLetterIsoName = culture.ThreeLetterISOLanguageName,
                    TwoLetterISOLanguageName = culture.TwoLetterISOLanguageName,
                    LanguageNames = identifiers,
                });
            }
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }
    }

    /// <summary>
    /// Detects the language from a given title string.
    /// </summary>
    /// <param name="title">The title string to analyze.</param>
    /// <param name="defaultLanguage">The default 3-letter ISO language code to return if no language is detected.</param>
    /// <returns>A <see cref="LanguageDetectionResult"/> object with the detected language and cleaned title.</returns>
    public virtual LanguageDetectionResult DetectLanguage(string title, string defaultLanguage = "deu")
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return new LanguageDetectionResult { LanguageCode = defaultLanguage, CleanedTitle = title ?? string.Empty };
        }

        var matches = _parenthesesRegex.Matches(title);
        foreach (Match match in matches)
        {
            string content = match.Groups["content"].Value.Trim();
            // If any parentheses content is empty, return default.
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            if (_ovIdentifiers.Contains(content))
            {
                string cleanedTitle = GetCleandRegexMatch(title, match);

                // "Instantly return" with "und" code if an OV tag was found.
                return new LanguageDetectionResult { LanguageCode = "und", MatchedIdentifier = content, CleanedTitle = cleanedTitle };
            }
            else if (_languageDataList.Any(ld => ld.LanguageNames.Contains(content)))
            {
                var langData = _languageDataList.First(ld => ld.LanguageNames.Contains(content));
                string cleanedTitle = GetCleandRegexMatch(title, match);

                return new LanguageDetectionResult
                {
                    LanguageCode = langData.ThreeLetterIsoName,
                    MatchedIdentifier = content,
                    CleanedTitle = cleanedTitle
                };
            }
        }

        // Next, check for a language extension in the filename (if its an filename).
        string langExtension = GetLanguageExtension(title);
        if (!string.IsNullOrEmpty(langExtension) && _languageDataList.Any(id => id.TwoLetterISOLanguageName.Equals(langExtension, StringComparison.OrdinalIgnoreCase) ||
                                           id.ThreeLetterIsoName.Equals(langExtension, StringComparison.OrdinalIgnoreCase)))
        {
            string cleanedTitle = GetCleanedLanguageExtensionTitle(title, langExtension);
            return new LanguageDetectionResult
            {
                LanguageCode = langExtension,
                MatchedIdentifier = langExtension,
                CleanedTitle = cleanedTitle
            };
        }

        // Return the default if no language was detected.
        return new LanguageDetectionResult { LanguageCode = defaultLanguage, CleanedTitle = title };
    }

    /// <summary>
    /// Extracts a potential language extension from the title.
    /// This method does assume that the language extension is the second last dot-separated segment in the filename.
    /// </summary>
    /// <param name="title">The title string to analyze.</param>
    /// <returns>Returns the language extension if found; otherwise, an empty string.</returns>
    private string GetLanguageExtension(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var filename = Path.GetFileName(title);
        ReadOnlySpan<char> span = filename.AsSpan();

        int lastDot = span.LastIndexOf('.');
        if (lastDot <= 0 || (filename.Length - lastDot - 1) > 5) // 0 = leading dot (".hidden") or no dot, or extension too long
        {
            return string.Empty;
        }

        int prevDot = span.Slice(0, lastDot).LastIndexOf('.');
        if (prevDot < 0 || (lastDot - prevDot - 1) is > 3 or < 2) // no previous dot or segment too long, lang codes are 2 or 3 letters
        {
            return string.Empty;
        }

        var langSpan = span.Slice(prevDot + 1, lastDot - prevDot - 1);
        return langSpan.ToString(); // nur hier wird ein string alloziert
    }

    /// <summary>
    /// Cleans the title by removing the specified language extension segment.
    /// </summary>
    /// <param name="title">The original title string.</param>
    /// <param name="langExtension">The language extension segment to remove.</param>
    /// <returns>The cleaned title string with the language extension removed.</returns>
    private string GetCleanedLanguageExtensionTitle(string title, string langExtension)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(langExtension))
        {
            return title;
        }

        var filename = Path.GetFileName(title);
        ReadOnlySpan<char> span = filename.AsSpan();

        int lastDot = span.LastIndexOf('.');
        if (lastDot <= 0) // 0 = leading dot (".hidden") or no dot
        {
            return title;
        }

        int prevDot = span.Slice(0, lastDot).LastIndexOf('.');
        if (prevDot < 0)
        {
            return title;
        }

        var langSpan = span.Slice(prevDot + 1, lastDot - prevDot - 1);
        if (!langSpan.Equals(langExtension, StringComparison.OrdinalIgnoreCase))
        {
            return title;
        }

        // Build cleaned title by removing the language extension segment.
        string cleanedTitle = string.Concat(span.Slice(0, prevDot), span.Slice(lastDot));
        return cleanedTitle.ToString();
    }

    private string GetCleandRegexMatch(string title, Match match)
    {
        var start = title.Substring(0, match.Index);
        var end = title.Substring(match.Index + match.Length);
        string cleanedTitle = start + end;
        cleanedTitle = Regex.Replace(cleanedTitle, @"\s{2,}", " ").Trim(); // Collapse spaces.
        return cleanedTitle;
    }

    /// <summary>
    /// Holds data for a single language to aid in detection.
    /// </summary>
    private sealed class LanguageData
    {
        public required string ThreeLetterIsoName { get; init; }

        public required string TwoLetterISOLanguageName { get; init; }

        public required HashSet<string> LanguageNames { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
