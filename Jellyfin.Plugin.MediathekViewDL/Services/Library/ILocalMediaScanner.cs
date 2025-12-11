namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Interface for the LocalMediaScanner service.
/// </summary>
public interface ILocalMediaScanner
{
    /// <summary>
    /// Scans the specified directory for video files and builds a cache of existing episodes.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to scan.</param>
    /// <param name="seriesName">The name of the series (used for parsing context).</param>
    /// <returns>A <see cref="LocalEpisodeCache"/> containing the found episodes.</returns>
    LocalEpisodeCache ScanDirectory(string directoryPath, string seriesName);
}
