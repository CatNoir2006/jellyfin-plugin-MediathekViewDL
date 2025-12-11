namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Represents parsed video information, including numbering, language, and specific features.
/// </summary>
public class VideoInfo
{
    /// <summary>
    /// Gets or sets the season number, if available.
    /// </summary>
    public int? SeasonNumber { get; set; }

    /// <summary>
    /// Gets or sets the episode number, if available.
    /// </summary>
    public int? EpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the absolute episode number, if available.
    /// </summary>
    public int? AbsoluteEpisodeNumber { get; set; }

    /// <summary>
    /// Gets or sets the title of the video, cleaned from parsing tags and language identifiers.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected 3-letter ISO language code (e.g., 'deu', 'eng').
    /// </summary>
    public string Language { get; set; } = "deu";

    /// <summary>
    /// Gets or sets a value indicating whether the content represents an episode of a show.
    /// </summary>
    public bool IsShow { get; set; }

    /// <summary>
    /// Gets a value indicating whether the video has absolute numbering.
    /// </summary>
    public bool HasAbsoluteNumbering => AbsoluteEpisodeNumber.HasValue;

    /// <summary>
    /// Gets a value indicating whether the video has season and episode numbering.
    /// </summary>
    public bool HasSeasonEpisodeNumbering => SeasonNumber.HasValue && EpisodeNumber.HasValue;

    /// <summary>
    /// Gets or sets a value indicating whether the video has audio description.
    /// </summary>
    public bool HasAudiodescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video has sign language (Gebärdensprache).
    /// </summary>
    public bool HasSignLanguage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is a trailer.
    /// </summary>
    public bool IsTrailer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is an interview.
    /// </summary>
    public bool IsInterview { get; set; }

    /// <summary>
    /// Gets a value indicating whether the video is considered an extra.
    /// This is only true if the Subscription is configured to treat non-episodes as extras.
    /// This should be ignored if non-episodes-as-extras treatment is disabled.
    /// </summary>
    public bool IsExtra => IsInterview || IsTrailer || (!IsShow);
}
