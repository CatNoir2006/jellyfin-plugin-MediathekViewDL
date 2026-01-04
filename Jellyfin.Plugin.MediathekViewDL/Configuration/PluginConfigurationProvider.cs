using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Implementation of <see cref="IConfigurationProvider"/> that retrieves the configuration from the static <see cref="Plugin"/> instance.
/// </summary>
public class PluginConfigurationProvider : IConfigurationProvider
{
    /// <inheritdoc />
    public PluginConfiguration Configuration
    {
        get
        {
            if (Plugin.Instance == null)
            {
                throw new InvalidOperationException("Plugin instance is not initialized.");
            }

            return Plugin.Instance.Configuration;
        }
    }

    /// <inheritdoc />
    public PluginConfiguration? ConfigurationOrNull => Plugin.Instance?.Configuration;
}
