using System;
using System.Diagnostics;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Represents a single download subscription based on a search query.
/// </summary>
[DebuggerDisplay("Name={Name}, Enabled={IsEnabled}, Search={Search}")]
public record Subscription
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

    /// <summary>
    /// Gets or sets a value indicating whether to ignore local files when processing this subscription.
    /// </summary>
    public bool IgnoreLocalFiles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore the download history when processing this subscription.
    /// </summary>
    public bool IgnoreHistory { get; set; }
}
