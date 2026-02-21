using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Represents a single download subscription based on a search query.
/// </summary>
[DebuggerDisplay("Name={Name}, Enabled={IsEnabled}, Search={Search}")]
public class Subscription
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Subscription"/> class.
    /// </summary>
    public Subscription()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the subscription.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user-defined name for the subscription. Used for the series folder name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this subscription is enabled.
    /// If false, it will be skipped during scheduled downloads.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the search settings for this subscription.
    /// </summary>
    public SearchSettings Search { get; set; } = new();

    /// <summary>
    /// Gets or sets the download settings for this subscription.
    /// </summary>
    public DownloadSettings Download { get; set; } = new();

    /// <summary>
    /// Gets or sets the series settings for this subscription.
    /// </summary>
    public SeriesSettings Series { get; set; } = new();

    /// <summary>
    /// Gets or sets the metadata settings for this subscription.
    /// </summary>
    public MetadataSettings Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the accessibility settings for this subscription.
    /// </summary>
    public AccessibilitySettings Accessibility { get; set; } = new();

    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful download for this subscription.
    /// This is purely for debugging and informational purposes.
    /// </summary>
    public DateTime? LastDownloadedTimestamp { get; set; }

    #pragma warning disable SA1124
    // ToDo: Remove obsolete properties on 1.0.0.0 release
    #region Obsolete Properties
    #pragma warning restore SA1124

    /// <summary>
    /// Gets or sets the language code (3-letter ISO).
    /// DO NOT USE. Use Metadata.OriginalLanguage instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("OriginalLanguage")]
    [JsonIgnore]
    public string? DeprecatedOriginalLanguage { get; set; }

    /// <summary>
    /// Gets the search query for the MediathekViewWeb API.
    /// This is obsolete and will be removed.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlArray("Queries")]
    [JsonIgnore]
    public Collection<QueryFields> DeprecatedQueries { get; init; } = new();

    /// <summary>
    /// Gets the search criteria for the MediathekViewWeb API.
    /// DO NOT USE. Use Search.Criteria instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlArray("Criteria")]
    [JsonIgnore]
    public Collection<QueryFieldsDto> DeprecatedCriteria { get; init; } = new();

    /// <summary>
    /// Gets or sets the specific download path for this subscription.
    /// DO NOT USE. Use Download.DownloadPath instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DownloadPath")]
    [JsonIgnore]
    public string DeprecatedDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to treat videos that are not recognized as episodes as extras.
    /// DO NOT USE. Use Series.TreatNonEpisodesAsExtras instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("TreatNonEpisodesAsExtras")]
    [JsonIgnore]
    public bool DeprecatedTreatNonEpisodesAsExtras { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether trailers should be saved.
    /// DO NOT USE. Use Series.SaveTrailers instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("SaveTrailers")]
    [JsonIgnore]
    public bool DeprecatedSaveTrailers { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether interviews should be saved.
    /// DO NOT USE. Use Series.SaveInterviews instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("SaveInterviews")]
    [JsonIgnore]
    public bool DeprecatedSaveInterviews { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether generic extras should be saved.
    /// DO NOT USE. Use Series.SaveGenericExtras instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("SaveGenericExtras")]
    [JsonIgnore]
    public bool DeprecatedSaveGenericExtras { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether extras should be saved as .strm files.
    /// DO NOT USE. Use Series.SaveExtrasAsStrm instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("SaveExtrasAsStrm")]
    [JsonIgnore]
    public bool DeprecatedSaveExtrasAsStrm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to only download content where season and episode can be parsed.
    /// DO NOT USE. Use Series.EnforceSeriesParsing instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("EnforceSeriesParsing")]
    [JsonIgnore]
    public bool DeprecatedEnforceSeriesParsing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create a local .nfo file.
    /// DO NOT USE. Use Metadata.CreateNfo instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("CreateNfo")]
    [JsonIgnore]
    public bool DeprecatedCreateNfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading content with absolute episode numbering.
    /// DO NOT USE. Use Series.AllowAbsoluteEpisodeNumbering instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowAbsoluteEpisodeNumbering")]
    [JsonIgnore]
    public bool DeprecatedAllowAbsoluteEpisodeNumbering { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading versions with audio descriptions.
    /// DO NOT USE. Use Accessibility.AllowAudioDescription instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowAudioDescription")]
    [JsonIgnore]
    public bool DeprecatedAllowAudioDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading versions with sign language.
    /// DO NOT USE. Use Accessibility.AllowSignLanguage instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowSignLanguage")]
    [JsonIgnore]
    public bool DeprecatedAllowSignLanguage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable enhanced duplicate detection.
    /// DO NOT USE. Use Download.EnhancedDuplicateDetection instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("EnhancedDuplicateDetection")]
    [JsonIgnore]
    public bool DeprecatedEnhancedDuplicateDetection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically upgrade to a higher quality version.
    /// DO NOT USE. Use Download.AutoUpgradeToHigherQuality instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AutoUpgradeToHigherQuality")]
    [JsonIgnore]
    public bool DeprecatedAutoUpgradeToHigherQuality { get; set; }

    /// <summary>
    /// Gets or sets the minimum duration in minutes for search results.
    /// DO NOT USE. Use Search.MinDurationMinutes instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("MinDurationMinutes")]
    [JsonIgnore]
    public int? DeprecatedMinDurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration in minutes for search results.
    /// DO NOT USE. Use Search.MaxDurationMinutes instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("MaxDurationMinutes")]
    [JsonIgnore]
    public int? DeprecatedMaxDurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the minimum broadcast date for search results.
    /// DO NOT USE. Use Search.MinBroadcastDate instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("MinBroadcastDate")]
    [JsonIgnore]
    public DateTimeOffset? DeprecatedMinBroadcastDate { get; set; }

    /// <summary>
    /// Gets or sets the maximum broadcast date for search results.
    /// DO NOT USE. Use Search.MaxBroadcastDate instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("MaxBroadcastDate")]
    [JsonIgnore]
    public DateTimeOffset? DeprecatedMaxBroadcastDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use streaming URL files (.strm).
    /// DO NOT USE. Use Download.UseStreamingUrlFiles instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("UseStreamingUrlFiles")]
    [JsonIgnore]
    public bool DeprecatedUseStreamingUrlFiles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to download the full video for secondary audio languages.
    /// DO NOT USE. Use Download.DownloadFullVideoForSecondaryAudio instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("DownloadFullVideoForSecondaryAudio")]
    [JsonIgnore]
    public bool DeprecatedDownloadFullVideoForSecondaryAudio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow falling back to lower quality versions.
    /// DO NOT USE. Use Download.AllowFallbackToLowerQuality instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AllowFallbackToLowerQuality")]
    [JsonIgnore]
    public bool DeprecatedAllowFallbackToLowerQuality { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to check if the URL retrieved from MediathekViewWeb API is valid.
    /// DO NOT USE. Use Download.QualityCheckWithUrl instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("QualityCheckWithUrl")]
    [JsonIgnore]
    public bool DeprecatedQualityCheckWithUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to append the broadcast date to the title.
    /// DO NOT USE. Use Metadata.AppendDateToTitle instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AppendDateToTitle")]
    [JsonIgnore]
    public bool DeprecatedAppendDateToTitle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to append the broadcast time to the title.
    /// DO NOT USE. Use Metadata.AppendTimeToTitle instead.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlElement("AppendTimeToTitle")]
    [JsonIgnore]
    public bool DeprecatedAppendTimeToTitle { get; set; }

    #endregion
}
