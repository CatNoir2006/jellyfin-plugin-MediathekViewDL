using System;
using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Helper for detecting and filtering out accessibility content (Audiodeskription, Gebärdensprache, Untertitel).
/// </summary>
public static class AccessibilityFilterHelper
{
    private static readonly Regex AdWordBoundaryRegex = new(
        @"(?:^|[\s\(\[\-_])AD(?:[\s\)\]\-_]|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SignWordBoundaryRegex = new(
        @"(?:^|[\s\(\[\-_])(?:DGS|GS)(?:[\s\)\]\-_]|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Checks whether the given title or text contains audio description markers.
    /// </summary>
    /// <param name="title">The title or filename to inspect.</param>
    /// <returns>True if audio description markers are detected; otherwise false.</returns>
    public static bool IsAudioDescription(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        return title.Contains("Audiodeskription", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("Hörfassung", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("(AD)", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("[AD]", StringComparison.OrdinalIgnoreCase) ||
               title.Contains(" - AD", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("(mit Untertitel)", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("[mit Untertitel]", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("(mit UT)", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("[mit UT]", StringComparison.OrdinalIgnoreCase) ||
               AdWordBoundaryRegex.IsMatch(title);
    }

    /// <summary>
    /// Checks whether the given title or text contains sign language markers (Gebärdensprache).
    /// </summary>
    /// <param name="title">The title or filename to inspect.</param>
    /// <returns>True if sign language markers are detected; otherwise false.</returns>
    public static bool IsSignLanguage(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        return title.Contains("Gebärdensprache", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("Gebärdendolmetscher", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("(DGS)", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("[DGS]", StringComparison.OrdinalIgnoreCase) ||
               title.Contains(" - DGS", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("(GS)", StringComparison.OrdinalIgnoreCase) ||
               title.Contains("[GS]", StringComparison.OrdinalIgnoreCase) ||
               SignWordBoundaryRegex.IsMatch(title);
    }

    /// <summary>
    /// Checks whether the item or title should be excluded based on accessibility settings.
    /// </summary>
    /// <param name="title">The raw title or directory name.</param>
    /// <param name="videoInfo">The parsed video info, if available.</param>
    /// <param name="allowAudioDescription">Whether audio description is allowed.</param>
    /// <param name="allowSignLanguage">Whether sign language is allowed.</param>
    /// <returns>True if the item should be excluded; otherwise false.</returns>
    public static bool ShouldFilterOut(string? title, VideoInfo? videoInfo, bool allowAudioDescription = false, bool allowSignLanguage = false)
    {
        if (!allowAudioDescription && (videoInfo?.HasAudiodescription == true || IsAudioDescription(title)))
        {
            return true;
        }

        if (!allowSignLanguage && (videoInfo?.HasSignLanguage == true || IsSignLanguage(title)))
        {
            return true;
        }

        return false;
    }
}
