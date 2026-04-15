namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Interface for the VideoParser service.
/// </summary>
public interface IVideoParser
{
    /// <summary>
    /// Parses video information (season, episode, title, language, features) from a given media title.
    /// </summary>
    /// <param name="topic">The name of the topic the Video belongs to.</param>
    /// <param name="mediaTitle">The title of the media item from the API.</param>
    /// <returns>An <see cref="VideoInfo"/> object if parsing is successful, otherwise null.</returns>
    VideoInfo? ParseVideoInfo(string? topic, string mediaTitle);
}
