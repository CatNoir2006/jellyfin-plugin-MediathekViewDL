namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// The result of a language detection operation.
/// </summary>
public class LanguageDetectionResult
{
    /// <summary>
    /// Gets or sets the detected 3-letter ISO 639-2 language code (e.g., "deu", "eng", or "und").
    /// </summary>
    public required string LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets the specific identifier string that was matched in the title (e.g., "German", "de").
    /// </summary>
    public string? MatchedIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the title after removing the matched language identifier and cleaning up surrounding delimiters.
    /// </summary>
    public required string CleanedTitle { get; set; }
}
