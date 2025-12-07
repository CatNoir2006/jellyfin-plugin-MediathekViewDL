using System.Collections.Generic;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Cache for local episodes to enable fast duplicate detection.
/// </summary>
public class LocalEpisodeCache
{
    private readonly HashSet<(int Season, int Episode, string Language)> _seasonEpisodes = new();
    private readonly HashSet<(int Absolute, string Language)> _absoluteEpisodes = new();

    /// <summary>
    /// Gets the count of unique Season/Episode pairs in the cache.
    /// </summary>
    public int SeasonEpisodeCount => _seasonEpisodes.Count;

    /// <summary>
    /// Gets the count of unique Absolute Episode numbers in the cache.
    /// </summary>
    public int AbsoluteEpisodeCount => _absoluteEpisodes.Count;

    /// <summary>
    /// Adds an episode to the cache.
    /// </summary>
    /// <param name="season">The season number.</param>
    /// <param name="episode">The episode number.</param>
    /// <param name="absolute">The absolute episode number.</param>
    /// <param name="language">The language code (default "deu").</param>
    public void Add(int? season, int? episode, int? absolute, string language = "deu")
    {
        var lang = language.ToLowerInvariant();
        if (season.HasValue && episode.HasValue)
        {
            _seasonEpisodes.Add((season.Value, episode.Value, lang));
        }

        if (absolute.HasValue)
        {
            _absoluteEpisodes.Add((absolute.Value, lang));
        }
    }

    /// <summary>
    /// Checks if the cache contains the episode described in the VideoInfo object.
    /// </summary>
    /// <param name="videoInfo">The video info object to check.</param>
    /// <returns>True if the episode exists in the cache, otherwise false.</returns>
    public bool Contains(VideoInfo videoInfo)
    {
        if (videoInfo == null)
        {
            return false;
        }

        return Contains(videoInfo.SeasonNumber, videoInfo.EpisodeNumber, videoInfo.AbsoluteEpisodeNumber, videoInfo.Language);
    }

    /// <summary>
    /// Checks if the cache contains the specified episode.
    /// </summary>
    /// <param name="season">The season number.</param>
    /// <param name="episode">The episode number.</param>
    /// <param name="absolute">The absolute episode number.</param>
    /// <param name="language">The language code (default "deu").</param>
    /// <returns>True if the episode exists in the cache, otherwise false.</returns>
    public bool Contains(int? season, int? episode, int? absolute, string language = "deu")
    {
        var lang = language.ToLowerInvariant();
        if (season.HasValue && episode.HasValue && _seasonEpisodes.Contains((season.Value, episode.Value, lang)))
        {
            return true;
        }

        if (absolute.HasValue && _absoluteEpisodes.Contains((absolute.Value, lang)))
        {
            return true;
        }

        return false;
    }
}
