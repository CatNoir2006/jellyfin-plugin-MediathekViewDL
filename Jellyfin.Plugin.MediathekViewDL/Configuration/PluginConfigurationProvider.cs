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

    /// <inheritdoc />
    public void Update(PluginConfiguration config)
    {
        if (Plugin.Instance == null)
        {
            throw new InvalidOperationException("Plugin instance is not initialized.");
        }

        Plugin.Instance.UpdateConfiguration(config);
    }

    /// <inheritdoc />
    public bool TryUpdate(PluginConfiguration config)
    {
        if (Plugin.Instance == null)
        {
            return false;
        }

        Plugin.Instance.UpdateConfiguration(config);
        return true;
    }

    /// <inheritdoc />
    public void Save()
    {
        if (Plugin.Instance == null)
        {
            throw new InvalidOperationException("Plugin instance is not initialized.");
        }

        Plugin.Instance.SaveConfiguration();
    }

    /// <inheritdoc />
    public bool TrySave()
    {
        if (Plugin.Instance == null)
        {
            return false;
        }

        Plugin.Instance.SaveConfiguration();
        return true;
    }
}
