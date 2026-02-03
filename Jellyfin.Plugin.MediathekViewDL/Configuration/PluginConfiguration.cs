using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
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

    /// <summary>
    /// Gets or sets the version of the configuration.
    /// Used for migrations.
    /// </summary>
    public int ConfigVersion { get; set; }

    #pragma warning disable SA1124
    // ToDo: Remove obsolete properties on 1.0.0.0 release
    #region Obsolete Properties
    #pragma warning restore SA1124
    /// <summary>
    /// Gets or sets the default path where completed downloads are stored.
    /// DO NOT USE. Use Paths.DefaultDownloadPath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultDownloadPath")]
    [JsonIgnore]
    public string DeprecatedDefaultDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for show downloads in subscriptions.
    /// DO NOT USE. Use Paths.DefaultSubscriptionShowPath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultSubscriptionShowPath")]
    [JsonIgnore]
    public string DeprecatedDefaultSubscriptionShowPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for movie downloads in subscriptions.
    /// DO NOT USE. Use Paths.DefaultSubscriptionMoviePath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultSubscriptionMoviePath")]
    [JsonIgnore]
    public string DeprecatedDefaultSubscriptionMoviePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for manual show downloads.
    /// DO NOT USE. Use Paths.DefaultManualShowPath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultManualShowPath")]
    [JsonIgnore]
    public string DeprecatedDefaultManualShowPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for manual movie downloads.
    /// DO NOT USE. Use Paths.DefaultManualMoviePath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DefaultManualMoviePath")]
    [JsonIgnore]
    public string DeprecatedDefaultManualMoviePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temporary path where files are stored during download.
    /// DO NOT USE. Use Paths.TempDownloadPath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("TempDownloadPath")]
    [JsonIgnore]
    public string DeprecatedTempDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether Paths for movies should contain the 'Topic' of the Movie.
    /// DO NOT USE. Use Paths.UseTopicForMoviePath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("UseTopicForMoviePath")]
    [JsonIgnore]
    public bool DeprecatedUseTopicForMoviePath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether subtitles should be downloaded if available.
    /// DO NOT USE. Use Download.DownloadSubtitles instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DownloadSubtitles")]
    [JsonIgnore]
    public bool DeprecatedDownloadSubtitles { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether direct audio extraction from URL is enabled.
    /// DO NOT USE. Use Download.EnableDirectAudioExtraction instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("EnableDirectAudioExtraction")]
    [JsonIgnore]
    public bool DeprecatedEnableDirectAudioExtraction { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum free disk space in bytes required to start a new download.
    /// DO NOT USE. Use Download.MinFreeDiskSpaceBytes instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("MinFreeDiskSpaceBytes")]
    [JsonIgnore]
    public long DeprecatedMinFreeDiskSpaceBytes { get; set; } = (long)(1.5 * 1024 * 1024 * 1024);

    /// <summary>
    /// Gets or sets a value indicating whether downloads should be allowed if the available disk space cannot be determined.
    /// DO NOT USE. Use Maintenance.AllowDownloadOnUnknownDiskSpace instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowDownloadOnUnknownDiskSpace")]
    [JsonIgnore]
    public bool DeprecatedAllowDownloadOnUnknownDiskSpace { get; set; }

    /// <summary>
    /// Gets or sets the maximum download bandwidth in MBit/s.
    /// DO NOT USE. Use Download.MaxBandwidthMBits instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("MaxBandwidthMBits")]
    [JsonIgnore]
    public int DeprecatedMaxBandwidthMBits { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether downloads from unknown domains are allowed.
    /// DO NOT USE. Use Network.AllowUnknownDomains instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowUnknownDomains")]
    [JsonIgnore]
    public bool DeprecatedAllowUnknownDomains { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether http is allowed for download URLs.
    /// DO NOT USE. Use Network.AllowHttp instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowHttp")]
    [JsonIgnore]
    public bool DeprecatedAllowHttp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a library scan should be triggered after a download finishes.
    /// DO NOT USE. Use Download.ScanLibraryAfterDownload instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("ScanLibraryAfterDownload")]
    [JsonIgnore]
    public bool DeprecatedScanLibraryAfterDownload { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the automated cleanup of invalid .strm files.
    /// DO NOT USE. Use Maintenance.EnableStrmCleanup instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("EnableStrmCleanup")]
    [JsonIgnore]
    public bool DeprecatedEnableStrmCleanup { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to fetch the stream size for search results.
    /// DO NOT USE. Use Search.FetchStreamSizes instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("FetchStreamSizes")]
    [JsonIgnore]
    public bool DeprecatedFetchStreamSizes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to search in future broadcasts when performing searches.
    /// DO NOT USE. Use Search.SearchInFutureBroadcasts instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("SearchInFutureBroadcasts")]
    [JsonIgnore]
    public bool DeprecatedSearchInFutureBroadcasts { get; set; } = true;
    #endregion

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
        "zdf.de"
    };
}
