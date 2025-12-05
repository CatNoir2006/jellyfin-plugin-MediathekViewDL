using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// A service to detect a language from a string based on various language identifiers.
/// This service pre-compiles regex patterns for performance.
/// </summary>
public class LanguageDetectionService
{
    private readonly List<LanguageData> _languageDataList;
    private readonly Regex _ovDetectionRegex;
    private readonly Regex _ovCleaningRegex;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageDetectionService"/> class.
    /// </summary>
    public LanguageDetectionService()
    {
        // 1. Compile regex for "Original Version" (OV) identifiers.
        // As per your feedback, this list can be expanded.
        var ovIdentifiers = new List<string> { "OV", "OmU", "OmeU", "Originalversion", "Originalversion mit Untertitel" };

        // Regex for simple detection.
        var ovDetectionPattern = $@"\b({string.Join("|", ovIdentifiers.Select(Regex.Escape))})\b";
        _ovDetectionRegex = new Regex(ovDetectionPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        // A more complex regex for cleaning all occurrences of OV tags, including surrounding brackets and spaces.
        var ovCleaningPattern = $@"[\s\(\[]*({string.Join("|", ovIdentifiers.Select(Regex.Escape))})[\s\)\]]*";
        _ovCleaningRegex = new Regex(ovCleaningPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

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
                    culture.TwoLetterISOLanguageName, culture.ThreeLetterISOLanguageName,
                    culture.EnglishName, culture.DisplayName, culture.NativeName
                };
                identifiers.RemoveWhere(string.IsNullOrWhiteSpace);
                if (identifiers.Count == 0)
                {
                    continue;
                }

                var pattern = $@"\b({string.Join("|", identifiers.Select(Regex.Escape))})\b";
                _languageDataList.Add(new LanguageData
                {
                    ThreeLetterIsoName = culture.ThreeLetterISOLanguageName,
                    CompiledRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1))
                });
            }
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }

        _languageDataList = _languageDataList
            .OrderByDescending(ld => ld.CompiledRegex.ToString().Length)
            .ToList();
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

        // First, check for any "Original Version" tags.
        var ovMatch = _ovDetectionRegex.Match(title);
        if (ovMatch.Success)
        {
            // Per your feedback, remove all occurrences of any OV tag.
            string cleanedTitle = _ovCleaningRegex.Replace(title, " ");
            cleanedTitle = Regex.Replace(cleanedTitle, @"\s{2,}", " ").Trim(); // Collapse spaces.

            // "Instantly return" with "und" code if an OV tag was found.
            return new LanguageDetectionResult { LanguageCode = "und", MatchedIdentifier = ovMatch.Value, CleanedTitle = cleanedTitle };
        }

        // If not an OV title, proceed with specific language detection.
        foreach (var langData in _languageDataList)
        {
            var match = langData.CompiledRegex.Match(title);
            if (match.Success)
            {
                // A language tag was found. Build a specific cleaning regex for it and its surroundings.
                string cleaningPattern = $@"\s*[\(\[]?\s*{Regex.Escape(match.Value)}\s*[)\]]?\s*";
                string cleanedTitle = Regex.Replace(title, cleaningPattern, " ", RegexOptions.IgnoreCase);
                cleanedTitle = Regex.Replace(cleanedTitle, @"\s{2,}", " ").Trim();

                return new LanguageDetectionResult
                {
                    LanguageCode = langData.ThreeLetterIsoName,
                    MatchedIdentifier = match.Value,
                    CleanedTitle = cleanedTitle
                };
            }
        }

        // Return the default if no language was detected.
        return new LanguageDetectionResult { LanguageCode = defaultLanguage, CleanedTitle = title };
    }

    /// <summary>
    /// Holds data for a single language to aid in detection.
    /// </summary>
    private sealed class LanguageData
    {
        public required string ThreeLetterIsoName { get; init; }

        public required Regex CompiledRegex { get; init; }
    }
}
