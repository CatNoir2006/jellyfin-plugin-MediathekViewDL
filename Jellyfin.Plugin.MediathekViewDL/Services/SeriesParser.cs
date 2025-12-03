using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Helper class for parsing episode information from media titles.
/// </summary>
public class SeriesParser
{
    private readonly ILogger<SeriesParser> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesParser"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SeriesParser(ILogger<SeriesParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses episode information (season, episode, title, language) from a given media title.
    /// </summary>
    /// <param name="subscriptionName">The name of the subscription, used as the base series name.</param>
    /// <param name="mediaTitle">The title of the media item from the API.</param>
    /// <param name="enforceSeriesParsing">If true, returns null if season/episode cannot be parsed.</param>
    /// <returns>An <see cref="EpisodeInfo"/> object if parsing is successful, otherwise null.</returns>
    public EpisodeInfo? ParseEpisodeInfo(string subscriptionName, string mediaTitle, bool enforceSeriesParsing)
    {
        // Default language to German if not explicitly found
        string language = "de";

        // Regex to find language tags like (OV), [OV], (Englisch), [Englisch]
        // Todo: Add Service to extract language form Name
        var langMatch = Regex.Match(mediaTitle, @"[\[(](OV|Originalfassung|Originalversion|Englisch|English|Französisch|French|Spanisch|Spanish)[\])]", RegexOptions.IgnoreCase);
        if (langMatch.Success)
        {
            language = langMatch.Groups[1].Value.ToLowerInvariant() switch
            {
                "ov" or "originalfassung" or "originalversion" => "ov",
                "englisch" or "english" => "en",
                "französisch" or "french" => "fr",
                "spanisch" or "spanish" => "es",
                _ => language
            };
            mediaTitle = mediaTitle.Replace(langMatch.Value, string.Empty, StringComparison.OrdinalIgnoreCase).Trim(); // Remove language tag from title
        }

        // Regex to find season and episode numbers (e.g., S01E01, Staffel 1 Episode 1, 1x01)
        var seasonEpisodeMatch = Regex.Match(mediaTitle, @"(?:s(\d+)\s*e(\d+))|staffel\s*(\d+)\s*episode\s*(\d+)|(\d+)[xX](\d+)", RegexOptions.IgnoreCase);

        int? seasonNumber = null;
        int? episodeNumber = null;
        string episodeTitle = mediaTitle;

        if (seasonEpisodeMatch.Success)
        {
            if (seasonEpisodeMatch.Groups[1].Success && seasonEpisodeMatch.Groups[2].Success) // S01E01 format
            {
                seasonNumber = int.Parse(seasonEpisodeMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                episodeNumber = int.Parse(seasonEpisodeMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            }
            else if (seasonEpisodeMatch.Groups[3].Success && seasonEpisodeMatch.Groups[4].Success) // Staffel 1 Episode 1 format
            {
                seasonNumber = int.Parse(seasonEpisodeMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                episodeNumber = int.Parse(seasonEpisodeMatch.Groups[4].Value, CultureInfo.InvariantCulture);
            }
            else if (seasonEpisodeMatch.Groups[5].Success && seasonEpisodeMatch.Groups[6].Success) // 1x01 format
            {
                seasonNumber = int.Parse(seasonEpisodeMatch.Groups[5].Value, CultureInfo.InvariantCulture);
                episodeNumber = int.Parse(seasonEpisodeMatch.Groups[6].Value, CultureInfo.InvariantCulture);
            }

            if (seasonNumber.HasValue && episodeNumber.HasValue)
            {
                // Remove the season/episode part from the title
                episodeTitle = Regex.Replace(mediaTitle, seasonEpisodeMatch.Value, string.Empty, RegexOptions.IgnoreCase).Trim();

                // Try to remove common series names/prefixes that might still be in the title
                // Example: "Tatort: Münster - S01E01 - Der Fall" -> subscriptionName "Tatort"
                var nameWithoutPrefix = Regex.Replace(episodeTitle, $"^{Regex.Escape(subscriptionName)}[\\s:_-]*", string.Empty, RegexOptions.IgnoreCase).Trim();
                if (!string.IsNullOrWhiteSpace(nameWithoutPrefix))
                {
                    episodeTitle = nameWithoutPrefix;
                }
            }
        }

        // Final cleanup for episode title
        episodeTitle = episodeTitle.Replace(":", string.Empty, StringComparison.Ordinal)
            .Replace(" - ", " ", StringComparison.Ordinal)
            .Replace("_", " ", StringComparison.Ordinal).Trim();

        // If parsing enforced and failed
        if (enforceSeriesParsing && (!seasonNumber.HasValue || !episodeNumber.HasValue))
        {
            _logger.LogWarning("Enforced series parsing failed for '{Title}' from subscription '{SubName}'. Skipping item.", mediaTitle, subscriptionName);
            return null;
        }

        // Fallback for series name if not explicitly found, or just use subscription name
        string finalSeriesName = subscriptionName;

        return new EpisodeInfo
        {
            SeriesName = finalSeriesName,
            SeasonNumber = seasonNumber ?? 1, // Default to Season 1 if not parsed
            EpisodeNumber = episodeNumber ?? 0, // Default to 0 if not parsed (for movies or specials)
            EpisodeTitle = string.IsNullOrWhiteSpace(episodeTitle) ? mediaTitle : episodeTitle,
            Language = language,
            IsParsed = seasonNumber.HasValue && episodeNumber.HasValue
        };
    }
}
