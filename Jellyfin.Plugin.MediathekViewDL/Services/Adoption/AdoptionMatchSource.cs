namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Defines the source or method used to determine a match.
/// </summary>
public enum AdoptionMatchSource
{
    /// <summary>
    /// Match was found in the download history (confirmed).
    /// </summary>
    History,

    /// <summary>
    /// Match was found via an exact URL in an info file.
    /// </summary>
    Url,

    /// <summary>
    /// Match was determined primarily by season and episode numbering.
    /// </summary>
    SeriesNumbering,

    /// <summary>
    /// Match was determined by fuzzy title/topic matching.
    /// </summary>
    Fuzzy
}
