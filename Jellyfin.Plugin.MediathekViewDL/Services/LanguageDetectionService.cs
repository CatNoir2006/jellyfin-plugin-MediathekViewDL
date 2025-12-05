using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly HashSet<string> _ovIdentifiers = ["(OV)", "(Originalversion mit Untertitel)", "(Originalversion)"];

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageDetectionService"/> class.
    /// It builds and compiles the regular expressions for all supported languages upon creation.
    /// </summary>
    public LanguageDetectionService()
    {
        _languageDataList = new List<LanguageData>();
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;

        try
        {
            // Set culture to German to get German display names for languages.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

            var neutralCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);

            foreach (var culture in neutralCultures)
            {
                if (string.IsNullOrEmpty(culture.Name) || culture.ThreeLetterISOLanguageName == "ivl")
                {
                    // Skip invariant culture and others without a proper 3-letter code.
                    continue;
                }

                var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    culture.TwoLetterISOLanguageName,
                    culture.ThreeLetterISOLanguageName,
                    culture.EnglishName, // e.g., "German"
                    culture.DisplayName, // e.g., "Deutsch" (because UI culture is de-DE)
                    culture.NativeName
                };

                // The name properties can sometimes be the same, and some might be empty.
                // HashSet handles duplicates, and we remove empty ones.
                identifiers.RemoveWhere(string.IsNullOrWhiteSpace);

                if (identifiers.Count == 0)
                {
                    continue;
                }

                // Create a single regex pattern string for all identifiers, e.g., \b(german|deutsch|de|deu)\b
                var pattern = $@"\b({string.Join("|", identifiers.Select(Regex.Escape))})\b";

                _languageDataList.Add(new LanguageData
                {
                    ThreeLetterIsoName = culture.ThreeLetterISOLanguageName,
                    // Compile the regex for optimal performance on repeated use.
                    CompiledRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1))
                });
            }
        }
        finally
        {
            // Restore the original culture to avoid side effects.
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }

        // For predictability, sort languages so that those with longer regex patterns are checked first.
        // This helps disambiguate cases where one language name might contain another.
        _languageDataList = _languageDataList
            .OrderByDescending(ld => ld.CompiledRegex.ToString().Length)
            .ToList();
    }

    /// <summary>
    /// Detects the language from a given title string using pre-compiled regular expressions.
    /// </summary>
    /// <param name="title">The title string to analyze.</param>
    /// <param name="defaultLanguage">The default 3-letter ISO language code to return if no language is detected.</param>
    /// <returns>The detected 3-letter ISO language code, "und" for original version (OV), or the default language.</returns>
    public string DetectLanguage(string title, string defaultLanguage = "deu")
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return defaultLanguage;
        }

        // Detect Original Version (OV) first. If found, return "und". As we do not know the actual language.
        if (_ovIdentifiers.Any(x => title.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            return "und";
        }

        // Iterate through the pre-compiled regex for each language.
        foreach (var langData in _languageDataList)
        {
            if (langData.CompiledRegex.IsMatch(title))
            {
                return langData.ThreeLetterIsoName;
            }
        }

        // Return the default if no language was detected.
        return defaultLanguage;
    }

    /// <summary>
    /// Holds data for a single language to aid in detection.
    /// </summary>
    private class LanguageData
    {
        /// <summary>
        /// Gets the three-letter ISO 639-2 language name.
        /// </summary>
        public required string ThreeLetterIsoName { get; init; }

        /// <summary>
        /// Gets the pre-compiled regular expression for detecting this language.
        /// </summary>
        public required Regex CompiledRegex { get; init; }
    }
}
