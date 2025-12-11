using Jellyfin.Plugin.MediathekViewDL.Configuration;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Interface for the FileNameBuilderService.
/// </summary>
public interface IFileNameBuilderService
{
    /// <summary>
    /// Generates all necessary download paths for a given video and subscription.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <returns>A <see cref="DownloadPaths"/> object containing all generated paths.</returns>
    DownloadPaths GenerateDownloadPaths(VideoInfo videoInfo, Subscription subscription);

    /// <summary>
    /// Sanitizes a string to be used as a file name.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>A sanitized file name.</returns>
    string SanitizeFileName(string fileName);

    /// <summary>
    /// Sanitizes a string to be used as a directory name.
    /// </summary>
    /// <param name="directoryName">The directory name to sanitize.</param>
    /// <returns>A sanitized directory name.</returns>
    string SanitizeDirectoryName(string directoryName);

    /// <summary>
    /// Gets the base directory for a subscription.
    /// </summary>
    /// <param name="subscription">The subscription.</param>
    /// <returns>The base directory path.</returns>
    string GetSubscriptionBaseDirectory(Subscription subscription);
}
