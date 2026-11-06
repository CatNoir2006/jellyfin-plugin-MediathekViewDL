namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Base settings for the download process.
/// </summary>
public record BaseDownloadSettings
{
    /// <summary>
    /// Gets a value indicating whether to use streaming URL files (.strm) instead of downloading the actual video files.
    /// </summary>
    public bool UseStreamingUrlFiles { get; init; }

    /// <summary>
    /// Gets a value indicating whether to download the full video for secondary audio languages.
    /// If false, only the audio track will be extracted for secondary languages.
    /// </summary>
    public bool DownloadFullVideoForSecondaryAudio { get; init; }

    /// <summary>
    /// Gets the minimum desired video height in pixels, or <see langword="null"/> for no limit.
    /// Streams below this height are skipped.
    /// </summary>
    public int? MinHeight { get; init; }

    /// <summary>
    /// Gets the maximum desired video height in pixels, or <see langword="null"/> for no limit.
    /// Streams above this height are skipped.
    /// </summary>
    public int? MaxHeight { get; init; }

    /// <summary>
    /// Gets the order in which available video qualities are preferred.
    /// Defaults to <see cref="QualitySelectionOrder.HighestToLowest"/>.
    /// </summary>
    public QualitySelectionOrder SelectionOrder { get; init; } = QualitySelectionOrder.HighestToLowest;

    /// <summary>
    /// Gets a value indicating whether to exclude streams whose resolution could not be determined
    /// when a height range is configured. If <see langword="false"/>, a fallback estimate based on
    /// the quality index (3→1080p, 2→720p, 1→480p) is used instead.
    /// </summary>
    public bool StrictResolution { get; init; }

    /// <summary>
    /// Gets a value indicating whether the video resolution probing (HTTP availability
    /// check and ffprobe) is skipped during enqueueing and the quality estimated from
    /// the API is used instead. This makes enqueueing significantly faster.
    /// </summary>
    public bool SkipVideoQualityProbing { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to always create a subfolder for the subscription (using the subscription name),
    /// even if the content is a movie and the global setting 'UseTopicForMoviePath' is disabled.
    /// </summary>
    public bool AlwaysCreateSubfolder { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable enhanced duplicate detection.
    /// If enabled, the target directory is scanned for existing files matching the season/episode pattern.
    /// </summary>
    public bool EnhancedDuplicateDetection { get; init; }
}
