using System;
using System.IO;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Service for handling file name building operations.
/// </summary>
public class FileNameBuilderService : IFileNameBuilderService
{
    private readonly ILogger<FileNameBuilderService> _logger;

    // Invalid characters for file names on most file systems
    private readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars().Concat(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }).ToArray();

    private readonly char[] _invalidFolderNameChars = Path.GetInvalidPathChars().Concat(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }).ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNameBuilderService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public FileNameBuilderService(ILogger<FileNameBuilderService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the plugin configuration. Protected virtual to allow overriding in tests.
    /// </summary>
    protected virtual PluginConfiguration? Configuration => Plugin.Instance?.Configuration;

    /// <summary>
    /// Generates all necessary download paths for a given video and subscription.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <returns>A <see cref="DownloadPaths"/> object containing all generated paths.</returns>
    public DownloadPaths GenerateDownloadPaths(VideoInfo videoInfo, Subscription subscription)
    {
        var paths = new DownloadPaths();

        string targetDirectory = BuildDirectoryName(videoInfo, subscription);
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            // Error already logged in BuildDirectoryName
            return paths; // Return empty paths object
        }

        paths.DirectoryPath = targetDirectory;
        paths.MainFilePath = Path.Combine(targetDirectory, BuildFileName(videoInfo, subscription, false));
        paths.SubtitleFilePath = Path.Combine(targetDirectory, BuildFileName(videoInfo, subscription, true));
        paths.StrmFilePath = Path.Combine(targetDirectory, BuildFileName(videoInfo, subscription, false, true));

        return paths;
    }

    /// <summary>
    /// Sanitizes a string to be used as a file name.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>A sanitized file name.</returns>
    public string SanitizeFileName(string fileName)
    {
        return string.Join("_", fileName.Split(_invalidFileNameChars));
    }

    /// <summary>
    /// Sanitizes a string to be used as a directory name.
    /// </summary>
    /// <param name="directoryName">The directory name to sanitize.</param>
    /// <returns>A sanitized directory name.</returns>
    public string SanitizeDirectoryName(string directoryName)
    {
        return string.Join("_", directoryName.Split(_invalidFolderNameChars));
    }

    /// <summary>
    /// Gets the base directory for a subscription.
    /// </summary>
    /// <param name="subscription">The subscription.</param>
    /// <returns>The base directory path.</returns>
    public string GetSubscriptionBaseDirectory(Subscription subscription)
    {
        var config = Configuration;
        string targetPath;

        if (string.IsNullOrWhiteSpace(subscription.DownloadPath))
        {
            string defaultPath = config?.DefaultDownloadPath ?? string.Empty;
            string subscriptionPath = SanitizeDirectoryName(subscription.Name);
            if (string.IsNullOrWhiteSpace(defaultPath))
            {
                // This will be logged later if we try to use it for an actual item, but for scanning it's fine to return empty.
                return string.Empty;
            }

            targetPath = Path.Combine(defaultPath, subscriptionPath);
        }
        else
        {
            targetPath = subscription.DownloadPath;
        }

        return targetPath;
    }

    /// <summary>
    /// Builds a sanitized file name for the given video info and subscription.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <param name="isSubtitle">Whether the file is a subtitle.</param>
    /// <param name="useStreamingUrlFile">Whether to generate a streaming URL file name.</param>
    /// <returns>The sanitized file name.</returns>
    private string BuildFileName(VideoInfo videoInfo, Subscription subscription, bool isSubtitle, bool useStreamingUrlFile = false)
    {
        string fileNamePart;
        if (videoInfo.IsShow && videoInfo.HasSeasonEpisodeNumbering)
        {
            fileNamePart = $"S{videoInfo.SeasonNumber!.Value:D2}E{videoInfo.EpisodeNumber!.Value:D2}";
        }
        else if (videoInfo.IsShow && videoInfo.HasAbsoluteNumbering)
        {
            fileNamePart = $"{videoInfo.AbsoluteEpisodeNumber!.Value:D3}"; // Pad absolute number to 3 digits for consistency
        }
        else
        {
            fileNamePart = string.Empty;
        }

        fileNamePart = string.IsNullOrWhiteSpace(fileNamePart) ? videoInfo.Title : $"{fileNamePart} - {videoInfo.Title}";

        if (!isSubtitle)
        {
            if (videoInfo.HasAudiodescription)
            {
                fileNamePart += " [AD]";
            }

            if (videoInfo.HasSignLanguage)
            {
                fileNamePart += " [DGS]";
            }
        }

        if (videoInfo.Language != "deu" || isSubtitle)
        {
            fileNamePart += $".{videoInfo.Language}";
        }

        if (isSubtitle)
        {
            fileNamePart += ".ttml"; // Subtitle file extension
        }
        else if (useStreamingUrlFile)
        {
            fileNamePart += ".strm"; // Streaming URL file extension
        }
        else if (videoInfo.Language == "deu" || subscription.DownloadFullVideoForSecondaryAudio)
        {
            fileNamePart += ".mkv"; // Main video with German audio or full video download for secondary languages
        }
        else
        {
            fileNamePart += ".mka"; // Audio track only
        }

        string sanitizedTitle = SanitizeFileName(fileNamePart);
        return sanitizedTitle;
    }

    /// <summary>
    /// Builds the target directory name based on video info and subscription settings.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <returns>The target directory name. Returns an empty string if no valid path is configured.</returns>
    private string BuildDirectoryName(VideoInfo videoInfo, Subscription subscription)
    {
        var config = Configuration;
        string targetPath = string.Empty;
        if (string.IsNullOrWhiteSpace(subscription.DownloadPath))
        {
            string defaultPath = config?.DefaultDownloadPath ?? string.Empty;
            string subscriptionPath = SanitizeDirectoryName(subscription.Name);
            if (string.IsNullOrWhiteSpace(defaultPath))
            {
                _logger.LogError("No default download path configured. Cannot build directory name for subscription '{SubscriptionName}' and item '{Title}'.", subscription.Name, videoInfo.Title);
                return string.Empty;
            }

            targetPath = Path.Combine(defaultPath, subscriptionPath);
        }
        else
        {
            targetPath = subscription.DownloadPath;
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            _logger.LogError("No download path configured for subscription '{SubscriptionName}' or globally. Skipping item '{Title}'.", subscription.Name, videoInfo.Title);
            return string.Empty;
        }

        if ((videoInfo.IsShow && videoInfo.HasSeasonEpisodeNumbering) || (subscription.TreatNonEpisodesAsExtras && videoInfo.SeasonNumber.HasValue))
        {
            targetPath = Path.Combine(targetPath, $"Staffel {videoInfo.SeasonNumber!.Value}");
        }

        if (subscription.TreatNonEpisodesAsExtras && !videoInfo.IsShow)
        {
            if (videoInfo.IsTrailer)
            {
                targetPath = Path.Combine(targetPath, "trailers");
            }
            else if (videoInfo.IsInterview)
            {
                targetPath = Path.Combine(targetPath, "interviews");
            }
            else
            {
                targetPath = Path.Combine(targetPath, "extras");
            }
        }

        return targetPath;
    }
}
