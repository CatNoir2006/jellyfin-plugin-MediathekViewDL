using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Media;

/// <summary>
/// Service for handling file name building operations.
/// </summary>
public class FileNameBuilderService : IFileNameBuilderService
{
    private readonly ILogger<FileNameBuilderService> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ILibraryManager _libraryManager;

    // Invalid characters for file names on most file systems
    private readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars().Concat(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }).ToArray();

    private readonly char[] _invalidFolderNameChars = Path.GetInvalidPathChars().Concat(new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }).ToArray();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileNameBuilderService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="libraryManager">The library manager.</param>
    public FileNameBuilderService(
        ILogger<FileNameBuilderService> logger,
        IConfigurationProvider configurationProvider,
        ILibraryManager libraryManager)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _libraryManager = libraryManager;
    }

    /// <inheritdoc />
    public DownloadPaths GenerateDownloadPaths(VideoInfo videoInfo, Subscription subscription, DownloadContext context, FileType? forceType = null)
    {
        var paths = new DownloadPaths();

        string targetDirectory = BuildDirectoryName(videoInfo, subscription, context);
        if (string.IsNullOrWhiteSpace(targetDirectory))
        {
            return paths;
        }

        paths.MainType = forceType ?? GetTargetMainType(videoInfo, subscription);

        paths.DirectoryPath = targetDirectory;
        var mainFile = BuildFileName(videoInfo, subscription, paths.MainType);
        paths.MainFilePath = Path.Combine(paths.DirectoryPath, mainFile);
        paths.SubtitleFilePath = Path.Combine(targetDirectory, BuildFileName(videoInfo, subscription, FileType.Subtitle));
        paths.NfoFilePath = Path.ChangeExtension(paths.MainFilePath, ".nfo");

        return paths;
    }

    /// <inheritdoc />
    public string SanitizeFileName(string fileName)
    {
        return string.Join("_", fileName.Split(_invalidFileNameChars));
    }

    /// <inheritdoc />
    public string SanitizeDirectoryName(string directoryName)
    {
        return string.Join("_", directoryName.Split(_invalidFolderNameChars));
    }

    /// <inheritdoc />
    public string GetSubscriptionBaseDirectory(Subscription subscription, DownloadContext context)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        string targetPath;

        if (string.IsNullOrWhiteSpace(subscription.Download.DownloadPath))
        {
            string defaultPath = GetDefaultPathForContext(config, context, subscription.Series.EnforceSeriesParsing || subscription.Series.AllowAbsoluteEpisodeNumbering);
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
            targetPath = subscription.Download.DownloadPath;
        }

        return targetPath;
    }

    /// <summary>
    /// Builds a sanitized file name for the given video info and subscription.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <param name="targetType">The FileType we want.</param>
    /// <returns>The sanitized file name.</returns>
    private string BuildFileName(VideoInfo videoInfo, Subscription subscription, FileType targetType)
    {
        string fileNamePart = videoInfo switch
        {
            { IsShow: true, HasSeasonEpisodeNumbering: true } => $"S{videoInfo.SeasonNumber!.Value:D2}E{videoInfo.EpisodeNumber!.Value:D2}",
            { IsShow: true, HasAbsoluteNumbering: true } => $"{videoInfo.AbsoluteEpisodeNumber!.Value:D3}",
            _ => string.Empty
        };

        fileNamePart = string.IsNullOrWhiteSpace(fileNamePart) ? videoInfo.Title : $"{fileNamePart} - {videoInfo.Title}";

        if (targetType != FileType.Subtitle)
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

        if (videoInfo.Language != "deu" || targetType != FileType.Video)
        {
            fileNamePart += $".{videoInfo.Language}";
        }

        switch (targetType)
        {
            case FileType.Subtitle:
                fileNamePart += ".ttml";
                break;
            case FileType.Strm:
                fileNamePart += ".strm";
                break;
            case FileType.Video:
                fileNamePart += ".mkv";
                break;
            case FileType.Audio:
                fileNamePart += ".mka";
                break;
            default:
                _logger.LogError("Unknown file type '{TargetType}' for File '{FileName}'.", targetType, videoInfo.Title);
                break;
        }

        string sanitizedTitle = SanitizeFileName(fileNamePart);
        return sanitizedTitle;
    }

    /// <summary>
    /// Builds the target directory name based on video info and subscription settings.
    /// </summary>
    /// <param name="videoInfo">The video information.</param>
    /// <param name="subscription">The subscription settings.</param>
    /// <param name="context">The download context.</param>
    /// <returns>The target directory name. Returns an empty string if no valid path is configured.</returns>
    private string BuildDirectoryName(VideoInfo videoInfo, Subscription subscription, DownloadContext context)
    {
        var config = _configurationProvider.ConfigurationOrNull;

        if (config == null)
        {
            _logger.LogWarning("Plugin configuration not avilable. Cant build config paths.");
            return string.Empty;
        }

        string targetPath;
        if (string.IsNullOrWhiteSpace(subscription.Download.DownloadPath))
        {
            bool useShowDir = videoInfo.IsShow || subscription.Series.TreatNonEpisodesAsExtras;
            string defaultPath = GetDefaultPathForContext(config, context, useShowDir);
            string subscriptionPath = SanitizeDirectoryName(subscription.Name);
            if (string.IsNullOrWhiteSpace(defaultPath))
            {
                _logger.LogError("No default download path configured for {Context} {Type}. Cannot build directory name for subscription '{SubscriptionName}' and item '{Title}'.", context, videoInfo.IsShow ? "Show" : "Movie", subscription.Name, videoInfo.Title);
                return string.Empty;
            }

            if (useShowDir || config.Paths.UseTopicForMoviePath)
            {
                targetPath = Path.Combine(defaultPath, subscriptionPath);
            }
            else
            {
                targetPath = defaultPath;
            }
        }
        else
        {
            targetPath = subscription.Download.DownloadPath;
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            _logger.LogError("No download path configured for subscription '{SubscriptionName}' or globally. Skipping item '{Title}'.", subscription.Name, videoInfo.Title);
            return string.Empty;
        }

        if ((videoInfo.IsShow && videoInfo.HasSeasonEpisodeNumbering) || (subscription.Series.TreatNonEpisodesAsExtras && videoInfo.SeasonNumber.HasValue))
        {
            targetPath = Path.Combine(targetPath, $"Staffel {videoInfo.SeasonNumber!.Value}");
        }

        if (subscription.Series.TreatNonEpisodesAsExtras && !videoInfo.IsShow)
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

        if (!subscription.Series.TreatNonEpisodesAsExtras && !videoInfo.IsShow)
        {
            var sanitizedTitle = SanitizeDirectoryName(videoInfo.Title);
            targetPath = Path.Combine(targetPath, sanitizedTitle);
        }

        return targetPath;
    }

    private string GetDefaultPathForContext(PluginConfiguration? config, DownloadContext context, bool isShow)
    {
        if (config == null)
        {
            return string.Empty;
        }

        string path = context switch
        {
            DownloadContext.Manual => isShow ? config.Paths.DefaultManualShowPath : config.Paths.DefaultManualMoviePath,
            DownloadContext.Subscription => isShow ? config.Paths.DefaultSubscriptionShowPath : config.Paths.DefaultSubscriptionMoviePath,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(path))
        {
            path = config.Paths.DefaultDownloadPath;
        }

        return path;
    }

    private FileType GetTargetMainType(VideoInfo videoInfo, Subscription subscription)
    {
        bool useStrm = subscription.Download.UseStreamingUrlFiles || (subscription.Series.SaveExtrasAsStrm && subscription.Series.TreatNonEpisodesAsExtras && !videoInfo.IsShow);
        if (useStrm)
        {
            return FileType.Strm;
        }

        // Audiodesc. should alwas be Audioonly, SignLang must be Video because else its nonsense
        if ((videoInfo is { Language: "deu", HasAudiodescription: false } or { HasSignLanguage: true }) || subscription.Download.DownloadFullVideoForSecondaryAudio)
        {
            return FileType.Video;
        }

        return FileType.Audio;
    }

    /// <inheritdoc />
    public bool IsPathSafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return false;
        }

        try
        {
            var normalizedPath = Path.GetFullPath(path);
            var normalizedPathWithSeparator = normalizedPath.EndsWith(Path.DirectorySeparatorChar)
                ? normalizedPath
                : normalizedPath + Path.DirectorySeparatorChar;

            var allowedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Add paths from configuration
            void AddIfNotNull(string? p)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {
                    try
                    {
                        var fullPath = Path.GetFullPath(p);
                        if (!fullPath.EndsWith(Path.DirectorySeparatorChar))
                        {
                            fullPath += Path.DirectorySeparatorChar;
                        }

                        allowedPaths.Add(fullPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error adding config path {Path} to allowed paths.", p);
                    }
                }
            }

            AddIfNotNull(config.Paths.DefaultDownloadPath);
            AddIfNotNull(config.Paths.DefaultSubscriptionShowPath);
            AddIfNotNull(config.Paths.DefaultSubscriptionMoviePath);
            AddIfNotNull(config.Paths.DefaultManualShowPath);
            AddIfNotNull(config.Paths.DefaultManualMoviePath);
            AddIfNotNull(config.Paths.TempDownloadPath);

            foreach (var sub in config.Subscriptions)
            {
                AddIfNotNull(sub.Download.DownloadPath);
            }

            // Add paths from Jellyfin libraries
            var virtualFolders = _libraryManager.GetVirtualFolders(false);
            if (virtualFolders != null)
            {
                foreach (var folder in virtualFolders)
                {
                    if (folder?.Locations != null && folder.CollectionType != CollectionTypeOptions.boxsets)
                    {
                        foreach (var folderPath in folder.Locations)
                        {
                            AddIfNotNull(folderPath);
                        }
                    }
                }
            }

            return allowedPaths.Any(allowed => normalizedPathWithSeparator.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating path safety for: {Path}", path);
            return false;
        }
    }
}
