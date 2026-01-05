using System;
using System.IO;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Helpers;

internal static class TempFileHelper
{
    public static string GetTempFilePath(
        string? destinationPath,
        string? extension,
        IConfigurationProvider configProvider,
        IServerApplicationPaths appPaths,
        ILogger logger)
    {
        var config = configProvider.ConfigurationOrNull;
        var tempDir = config?.TempDownloadPath;

        // 1. If TempDownloadPath is configured, use it.
        if (!string.IsNullOrWhiteSpace(tempDir))
        {
            if (!Directory.Exists(tempDir))
            {
                try
                {
                    Directory.CreateDirectory(tempDir);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Could not create configured temp directory '{TempDir}'. Falling back to system temp.", tempDir);
                    tempDir = null; // Fallback
                }
            }
        }

        // 2. If no configured temp dir, and destination provided (implied "empty = use destination" from HTML desc)
        if (string.IsNullOrWhiteSpace(tempDir) && !string.IsNullOrWhiteSpace(destinationPath))
        {
            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                tempDir = destDir;
                if (!Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Could not create temp directory '{TempDir}'. Falling back to system temp.", tempDir);
                        tempDir = null;
                    }
                }
            }
        }

        // 3. Fallback to System Temp (Jellyfin Temp)
        if (string.IsNullOrWhiteSpace(tempDir))
        {
            tempDir = appPaths.TempDirectory;
        }

        // Add custom extension Part to the file so we can detect our temp files Later.
        var tempFileName = $"{Guid.NewGuid()}{extension}.mvdl-tmp";
        return Path.Combine(tempDir, tempFileName);
    }
}
