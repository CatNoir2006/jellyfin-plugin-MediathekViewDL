using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;
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
    }

    /// <summary>
    /// Gets or sets the version of the configuration.
    /// Used for migrations.
    /// </summary>
    public int ConfigVersion { get; set; }

    /// <summary>
    /// Gets or sets the configuration paths.
    /// Contains the paths for the different download types.
    /// </summary>
    public ConfigurationPaths Paths { get; set; } = new();

    /// <summary>
    /// Gets or sets the download options.
    /// </summary>
    public DownloadOptions Download { get; set; } = new();

    /// <summary>
    /// Gets or sets the search options.
    /// </summary>
    public SearchOptions Search { get; set; } = new();

    /// <summary>
    /// Gets or sets the network options.
    /// </summary>
    public NetworkOptions Network { get; set; } = new();

    /// <summary>
    /// Gets or sets the maintenance options.
    /// </summary>
    public MaintenanceOptions Maintenance { get; set; } = new();

    /// <summary>
    /// Gets or sets the subscription default values.
    /// </summary>
    public SubscriptionDefaults SubscriptionDefaults { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the setup wizard has been completed.
    /// Used to auto-show the first-run wizard on fresh installations
    /// and to suppress it once the user has finished (or explicitly skipped) the setup.
    /// </summary>
    public bool WizardCompleted { get; set; }

    /// <summary>
    /// Gets the list of download subscriptions.
    /// </summary>
    public Collection<Subscription> Subscriptions { get; init; }

    /// <summary>
    /// Gets or sets the timestamp of the last job run.
    /// </summary>
    public DateTime LastRun { get; set; }

    /// <summary>
    /// Gets the list of allowed download domains.
    /// This covers the known CDNs used by ARD and ZDF.
    /// The list does only contain top-level domains subdomains may be added at some point.
    /// </summary>
    public HashSet<string> AllowedDomains => new(StringComparer.OrdinalIgnoreCase)
    {
        "akamaihd.net",
        "akamaized.net",
        "apa.at",
        "ard-mcdn.de",
        "ard.de",
        "ardmediathek.de",
        "br.de",
        "daserste.de",
        "orf.at",
        "srf.ch",
        "zdf.de",
        "kika.de",
    };
}
