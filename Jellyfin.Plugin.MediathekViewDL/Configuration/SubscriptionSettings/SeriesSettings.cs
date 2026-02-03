namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Settings for series parsing and handling.
/// </summary>
public record SeriesSettings
{
    /// <summary>
    /// Gets a value indicating whether to only download content where season and episode can be parsed from the title.
    /// </summary>
    public bool EnforceSeriesParsing { get; init; }

    /// <summary>
    /// Gets a value indicating whether to allow downloading content with absolute episode numbering.
    /// This is ignored if EnforceSeriesParsing is false.
    /// </summary>
    public bool AllowAbsoluteEpisodeNumbering { get; init; }

    /// <summary>
    /// Gets a value indicating whether to treat videos that are not recognized as episodes as extras.
    /// This Option is ignored if EnforceSeriesParsing is true.
    /// </summary>
    public bool TreatNonEpisodesAsExtras { get; init; }

    /// <summary>
    /// Gets a value indicating whether trailers should be saved (only if TreatNonEpisodesAsExtras is true).
    /// </summary>
    public bool SaveTrailers { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether interviews should be saved (only if TreatNonEpisodesAsExtras is true).
    /// </summary>
    public bool SaveInterviews { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether generic extras (not trailers/interviews) should be saved (only if TreatNonEpisodesAsExtras is true).
    /// </summary>
    public bool SaveGenericExtras { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether extras should be saved as .strm files.
    /// </summary>
    public bool SaveExtrasAsStrm { get; init; }
}
