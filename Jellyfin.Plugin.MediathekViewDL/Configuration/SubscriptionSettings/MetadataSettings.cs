namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Settings for metadata and file naming.
/// </summary>
public record MetadataSettings
{
    /// <summary>
    /// Gets a value indicating whether to create a local .nfo file with metadata (Episode number, description, etc.).
    /// </summary>
    public bool CreateNfo { get; init; } = false;

    /// <summary>
    /// Gets the language code (3-letter ISO) to use when the language cannot be detected or is detected as "und" (e.g. for OV content).
    /// </summary>
    public string? OriginalLanguage { get; init; }

    /// <summary>
    /// Gets a value indicating whether to append the broadcast date to the title.
    /// Useful for shows that don't have unique titles or season/episode numbers.
    /// </summary>
    public bool AppendDateToTitle { get; init; }

    /// <summary>
    /// Gets a value indicating whether to append the broadcast time to the title.
    /// Useful for shows that air multiple times a day (e.g. news).
    /// </summary>
    public bool AppendTimeToTitle { get; init; }
}
