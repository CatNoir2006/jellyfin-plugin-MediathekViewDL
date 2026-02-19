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

    /// <summary>
    /// Updates the plugin configuration and saves it.
    /// </summary>
    /// <param name="config">The new configuration.</param>
    void Update(PluginConfiguration config);

    /// <summary>
    /// Tries to update the plugin configuration and save it.
    /// </summary>
    /// <param name="config">The new configuration.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    bool TryUpdate(PluginConfiguration config);

    /// <summary>
    /// Saves the current plugin configuration.
    /// </summary>
    void Save();

    /// <summary>
    /// Tries to save the current plugin configuration.
    /// </summary>
    /// <returns>True if the save was successful, false otherwise.</returns>
    bool TrySave();
}
