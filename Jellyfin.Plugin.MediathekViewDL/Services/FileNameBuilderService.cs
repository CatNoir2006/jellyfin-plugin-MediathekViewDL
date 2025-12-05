using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service for handling file name building operations.
/// </summary>
public class FileNameBuilderService
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
    /// Builds a sanitized file name for the given video info and subscription.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="isSubtitle">Whether the file is a subtitle.</param>
    /// <returns>The sanitized file name.</returns>
    public string BuildFileName(VideoInfo videoInfo, bool isSubtitle)
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
        else if (videoInfo.Language == "deu")
        {
            fileNamePart += ".mkv"; // Main video with German audio
        }
        else
        {
            fileNamePart += ".mka"; // Audio track only
        }

        string sanitizedTitle = string.Join("_", fileNamePart.Split(_invalidFileNameChars));
        return sanitizedTitle;
    }

    /// <summary>
    /// Builds the target directory name based on video info and subscription settings.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <returns>The target directory name. Returns an empty string if no valid path is configured.</returns>
    public string BuildDirectoryName(VideoInfo videoInfo, Subscription subscription)
    {
        var config = Plugin.Instance?.Configuration;
        string targetPath = string.Empty;
        if (string.IsNullOrWhiteSpace(subscription.DownloadPath))
        {
            string defaultPath = config?.DefaultDownloadPath ?? string.Empty;
            string subscriptionPath = string.Join("_", subscription.Name.Split(_invalidFolderNameChars));
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

        if (videoInfo.IsShow && videoInfo.HasSeasonEpisodeNumbering)
        {
            targetPath = Path.Combine(targetPath, $"Staffel {videoInfo.SeasonNumber!.Value}");
        }
        else if (subscription.TreatNonEpisodesAsExtras)
        {
            targetPath = Path.Combine(targetPath, "Extras");
        }

        return targetPath;
    }
}
