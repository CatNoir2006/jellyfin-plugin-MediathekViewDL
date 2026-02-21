using System;
using System.Collections.Generic;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Cache for local episodes to enable fast duplicate detection.
/// </summary>
public class LocalEpisodeCache
{
    private readonly Dictionary<(int Season, int Episode, string Language), string> _seasonEpisodes = new();
    private readonly Dictionary<(int Absolute, string Language), string> _absoluteEpisodes = new();

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
    /// <param name="filePath">The full path to the file.</param>
    /// <param name="language">The language code (default "deu").</param>
    public void Add(int? season, int? episode, int? absolute, string filePath, string language = "deu")
    {
        var lang = language.ToLowerInvariant();
        if (season.HasValue && episode.HasValue)
        {
            _seasonEpisodes[(season.Value, episode.Value, lang)] = filePath;
        }

        if (absolute.HasValue)
        {
            _absoluteEpisodes[(absolute.Value, lang)] = filePath;
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
        if (season.HasValue && episode.HasValue && _seasonEpisodes.ContainsKey((season.Value, episode.Value, lang)))
        {
            return true;
        }

        if (absolute.HasValue && _absoluteEpisodes.ContainsKey((absolute.Value, lang)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the file path for an existing episode if found in the cache.
    /// </summary>
    /// <param name="videoInfo">The video info object to search for.</param>
    /// <returns>The full file path if found, otherwise null.</returns>
    public string? GetExistingFilePath(VideoInfo videoInfo)
    {
        if (videoInfo == null)
        {
            return null;
        }

        var lang = videoInfo.Language.ToLowerInvariant();

        if (videoInfo.SeasonNumber.HasValue && videoInfo.EpisodeNumber.HasValue)
        {
            if (_seasonEpisodes.TryGetValue((videoInfo.SeasonNumber.Value, videoInfo.EpisodeNumber.Value, lang), out var path))
            {
                return path;
            }
        }

        if (videoInfo.AbsoluteEpisodeNumber.HasValue)
        {
            if (_absoluteEpisodes.TryGetValue((videoInfo.AbsoluteEpisodeNumber.Value, lang), out var path))
            {
                return path;
            }
        }

        return null;
    }
}
