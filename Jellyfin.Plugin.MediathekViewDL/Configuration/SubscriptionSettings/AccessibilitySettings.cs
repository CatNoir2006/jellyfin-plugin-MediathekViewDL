namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Settings for accessibility features.
/// </summary>
public record AccessibilitySettings
{
    /// <summary>
    /// Gets a value indicating whether to allow downloading versions with audio descriptions.
    /// </summary>
    public bool AllowAudioDescription { get; init; }

    /// <summary>
    /// Gets a value indicating whether to allow downloading versions with sign language (Gebärdensprache).
    /// </summary>
    public bool AllowSignLanguage { get; init; }
}
