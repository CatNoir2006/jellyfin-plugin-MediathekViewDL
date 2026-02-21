namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Interface for the LanguageDetectionService.
/// </summary>
public interface ILanguageDetectionService
{
    /// <summary>
    /// Detects the language from a given title string.
    /// </summary>
    /// <param name="title">The title string to analyze.</param>
    /// <param name="defaultLanguage">The default 3-letter ISO language code to return if no language is detected.</param>
    /// <returns>A <see cref="LanguageDetectionResult"/> object with the detected language and cleaned title.</returns>
    LanguageDetectionResult DetectLanguage(string title, string defaultLanguage = "deu");
}
