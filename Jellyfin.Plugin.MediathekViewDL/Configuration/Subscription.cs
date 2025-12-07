using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jellyfin.Plugin.MediathekViewDL.Api;

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
    /// Gets the search query for the MediathekViewWeb API.
    /// </summary>
    public Collection<QueryFields> Queries { get; init; } = new();

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
    /// Gets or sets a value indicating whether to only download content where season and episode can be parsed from the title.
    /// </summary>
    public bool EnforceSeriesParsing { get; set; }

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
    public bool DownloadFullVideoForSecondaryAudio { get; set; } = false;

    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful download for this subscription.
    /// This is purely for debugging and informational purposes.
    /// </summary>
    public DateTime? LastDownloadedTimestamp { get; set; }

    /// <summary>
    /// Gets a set of unique identifiers for items that have already been processed for this subscription.
    /// This is used to avoid re-processing or re-downloading content.
    /// </summary>
    public HashSet<string> ProcessedItemIds { get; init; } = new();
}
