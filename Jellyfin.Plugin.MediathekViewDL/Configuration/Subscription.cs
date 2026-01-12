using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Represents a single download subscription based on a search query.
/// </summary>
public class Subscription
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Subscription"/> class.
    /// </summary>
    public Subscription()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        DownloadPath = string.Empty;
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
    /// Gets or sets the language code (3-letter ISO) to use when the language cannot be detected or is detected as "und" (e.g. for OV content).
    /// </summary>
    public string? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets the search query for the MediathekViewWeb API.
    /// </summary>
    [Obsolete("Use Criteria instead. This property will be removed in future versions. It is kept for migration purposes.")]
    [JsonIgnore] // This property should no longer be serialized/deserialized
    public Collection<QueryFields> Queries { get; init; } = new();

    /// <summary>
    /// Gets the search criteria for the MediathekViewWeb API.
    /// </summary>
    public Collection<QueryFieldsDto> Criteria { get; init; } = new();

    /// <summary>
    /// Gets or sets the specific download path for this subscription. Overrides the default path if set.
    /// </summary>
    public string DownloadPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this subscription is enabled.
    /// If false, it will be skipped during scheduled downloads.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to treat videos that are not recognized as episodes as extras.
    /// This Option is ignored if EnforceSeriesParsing is true.
    /// </summary>
    public bool TreatNonEpisodesAsExtras { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether trailers should be saved (only if TreatNonEpisodesAsExtras is true).
    /// </summary>
    public bool SaveTrailers { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether interviews should be saved (only if TreatNonEpisodesAsExtras is true).
    /// </summary>
    public bool SaveInterviews { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether generic extras (not trailers/interviews) should be saved (only if TreatNonEpisodesAsExtras is true).
    /// </summary>
    public bool SaveGenericExtras { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether extras should be saved as .strm files.
    /// </summary>
    public bool SaveExtrasAsStrm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to only download content where season and episode can be parsed from the title.
    /// </summary>
    public bool EnforceSeriesParsing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create a local .nfo file with metadata (Episode number, description, etc.).
    /// </summary>
    public bool CreateNfo { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading content with absolute episode numbering.
    /// This is ignored if EnforceSeriesParsing is false.
    /// </summary>
    public bool AllowAbsoluteEpisodeNumbering { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading versions with audio descriptions.
    /// </summary>
    public bool AllowAudioDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading versions with sign language (Gebärdensprache).
    /// </summary>
    public bool AllowSignLanguage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable enhanced duplicate detection.
    /// If enabled, the target directory is scanned for existing files matching the season/episode pattern.
    /// </summary>
    public bool EnhancedDuplicateDetection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically upgrade to a higher quality version if available.
    /// </summary>
    public bool AutoUpgradeToHigherQuality { get; set; }

    /// <summary>
    /// Gets or sets the minimum duration in minutes for search results.
    /// </summary>
    public int? MinDurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration in minutes for search results.
    /// </summary>
    public int? MaxDurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use streaming URL files (.strm) instead of downloading the actual video files.
    /// </summary>
    public bool UseStreamingUrlFiles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to download the full video for secondary audio languages.
    /// If false, only the audio track will be extracted for secondary languages.
    /// </summary>
    public bool DownloadFullVideoForSecondaryAudio { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow falling back to lower quality versions
    /// if HD version is not available.
    /// </summary>
    public bool AllowFallbackToLowerQuality { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to check if the URL retrieved from MediathekViewWeb API is valid.
    /// If not it will try with the next lower quality available.
    /// This can slow down the Scan. Especially if thers a lot of unavailable videos.
    /// </summary>
    public bool QualityCheckWithUrl { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful download for this subscription.
    /// This is purely for debugging and informational purposes.
    /// </summary>
    public DateTime? LastDownloadedTimestamp { get; set; }
}
