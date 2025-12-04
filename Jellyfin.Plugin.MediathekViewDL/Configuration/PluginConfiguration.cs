using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        Subscriptions = new Collection<Subscription>();
        DefaultDownloadPath = string.Empty;
        DownloadSubtitles = true;
    }

    /// <summary>
    /// Gets or sets the default path where completed downloads are stored.
    /// Can be overridden by a subscription.
    /// </summary>
    public string DefaultDownloadPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether subtitles should be downloaded if available.
    /// </summary>
    public bool DownloadSubtitles { get; set; }

    /// <summary>
    /// Gets the list of download subscriptions.
    /// </summary>
    public Collection<Subscription> Subscriptions { get; init; }

    /// <summary>
    /// Gets or sets the timestamp of the last job run.
    /// </summary>
    public DateTime LastRun { get; set; }
}
