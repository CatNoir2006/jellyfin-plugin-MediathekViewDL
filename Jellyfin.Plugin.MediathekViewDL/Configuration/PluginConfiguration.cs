using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
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

    #pragma warning disable SA1124
    // ToDo: Remove obsolete properties on 1.0.0.0 release
    #region Obsolete Path Properties
    #pragma warning restore SA1124
    /// <summary>
    /// Gets or sets the default path where completed downloads are stored.
    /// Can be overridden by a subscription.
    /// </summary>
    [Obsolete("Use Paths.DefaultDownloadPath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultDownloadPath")]
    public string DefaultDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for show downloads in subscriptions.
    /// </summary>
    [Obsolete("Use Paths.DefaultSubscriptionShowPath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultSubscriptionShowPath")]
    public string DefaultSubscriptionShowPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for movie downloads in subscriptions.
    /// </summary>
    [Obsolete("Use Paths.DefaultSubscriptionMoviePath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultSubscriptionMoviePath")]
    public string DefaultSubscriptionMoviePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for manual show downloads.
    /// </summary>
    [Obsolete("Use Paths.DefaultManualShowPath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultManualShowPath")]
    public string DefaultManualShowPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for manual movie downloads.
    /// </summary>
    [Obsolete("Use Paths.DefaultManualMoviePath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultManualMoviePath")]
    public string DefaultManualMoviePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temporary path where files are stored during download.
    /// If empty, the destination path is used directly.
    /// </summary>
    [Obsolete("Use Paths.TempDownloadPath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("TempDownloadPath")]
    public string TempDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether Paths for movies should contain the 'Topic' of the Movie.
    /// </summary>
    [Obsolete("Use Paths.UseTopicForMoviePath instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("UseTopicForMoviePath")]
    public bool UseTopicForMoviePath { get; set; }
    #endregion

    /// <summary>
    /// Gets or sets the configuration paths.
    /// Contains the paths for the different download types.
    /// </summary>
    public ConfigurationPaths Paths { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether subtitles should be downloaded if available.
    /// </summary>
    public bool DownloadSubtitles { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether direct audio extraction from URL is enabled.
    /// </summary>
    public bool EnableDirectAudioExtraction { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum free disk space in bytes required to start a new download.
    /// </summary>
    public long MinFreeDiskSpaceBytes { get; set; } = (long)(1.5 * 1024 * 1024 * 1024); // Default to 1.5 GiB

    /// <summary>
    /// Gets or sets a value indicating whether downloads should be allowed if the available disk space cannot be determined.
    /// This can happen with network shares or non-standard file systems.
    /// </summary>
    public bool AllowDownloadOnUnknownDiskSpace { get; set; }

    /// <summary>
    /// Gets or sets the maximum download bandwidth in MBit/s.
    /// 0 means unlimited.
    /// </summary>
    public int MaxBandwidthMBits { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether downloads from unknown domains are allowed.
    /// This may be usefull if ARD or ZDF adds new CDNs that are not yet whitelisted.
    /// This may pose a security risk, so use with caution.
    /// </summary>
    public bool AllowUnknownDomains { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether http is allowed for download URLs.
    /// This may be necessary as some URLs do not support https for some reason.
    /// I recommend keeping this off and only turning it on if you encounter problems.
    /// </summary>
    public bool AllowHttp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a library scan should be triggered after a download finishes.
    /// </summary>
    public bool ScanLibraryAfterDownload { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the automated cleanup of invalid .strm files.
    /// </summary>
    public bool EnableStrmCleanup { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to fetch the stream size for search results.
    /// This requires an additional HTTP request per result and may slow down the search.
    /// </summary>
    public bool FetchStreamSizes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to search in future broadcasts when performing searches.
    /// </summary>
    public bool SearchInFutureBroadcasts { get; set; } = true;

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
    public HashSet<string> AllowedDomains => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
        "zdf.de"
    };
}
