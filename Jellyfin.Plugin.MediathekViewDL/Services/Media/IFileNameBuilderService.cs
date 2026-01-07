using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

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
    /// <param name="context">The download context.</param>
    /// <param name="forceType">Forces the usage of a specific file type.</param>
    /// <returns>A <see cref="DownloadPaths"/> object containing all generated paths.</returns>
    DownloadPaths GenerateDownloadPaths(VideoInfo videoInfo, Subscription subscription, DownloadContext context, FileType? forceType = null);

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
    /// <param name="context">The download context.</param>
    /// <returns>The base directory path.</returns>
    string GetSubscriptionBaseDirectory(Subscription subscription, DownloadContext context);

    /// <summary>
    /// Validates if a path is safe to write to (within configured download directories or Jellyfin libraries).
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>True if the path is safe, false otherwise.</returns>
    bool IsPathSafe(string path);
}
