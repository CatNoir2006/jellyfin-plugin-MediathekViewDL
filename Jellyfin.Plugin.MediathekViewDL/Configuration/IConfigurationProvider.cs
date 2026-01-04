namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Provides access to the plugin configuration.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Gets the current plugin configuration.
    /// </summary>
    PluginConfiguration Configuration { get; }

    /// <summary>
    /// Gets the current plugin configuration or null if not available.
    /// </summary>
    PluginConfiguration? ConfigurationOrNull { get; }
}
