namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Represents parsed episode information.
/// </summary>
public class EpisodeInfo
{
    /// <summary>
    /// Gets or sets the name of the series.
    /// </summary>
    public string SeriesName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the season number.
    /// </summary>
    public int SeasonNumber { get; set; }

    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    public int EpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the title of the episode.
    /// </summary>
    public string EpisodeTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected language code (e.g., 'de', 'en', 'ov').
    /// </summary>
    public string Language { get; set; } = "de";

    /// <summary>
    /// Gets or sets a value indicating whether season and episode numbers were successfully parsed.
    /// </summary>
    public bool IsParsed { get; set; }
}
