using System;

namespace Jellyfin.Plugin.MediathekViewDL.Exceptions;

/// <summary>
/// Exception thrown when the configuration version is not supported by the plugin.
/// </summary>
public class UnsupportedConfigurationVersionException(int currentVersion, int minSupportedVersion, int targetSupportedVersion) :
    Exception($"Configuration version {currentVersion} is not supported. Minimum supported version is {minSupportedVersion} and target supported version is {targetSupportedVersion}.")
{
    /// <summary>
    /// Gets the current configuration version that is not supported.
    /// </summary>
    public int CurrentVersion { get; } = currentVersion;

    /// <summary>
    /// Gets the target configuration version that is supported by the plugin.
    /// </summary>
    public int TargetSupportedVersion { get; } = targetSupportedVersion;

    /// <summary>
    /// Gets the minimum configuration version that is supported by the plugin for migration purposes.
    /// </summary>
    public int MinimumSupportedVersion { get; } = minSupportedVersion;
}
