using System;
using System.Collections.Generic;
using Jellyfin.Plugin.MediathekViewDL.Data;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Represents a grouped download history entry.
/// </summary>
public class GroupedDownloadHistoryDto
{
    /// <summary>
    /// Gets or sets the subscription identifier.
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the title of the group.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the group.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider item identifier.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latest timestamp in this group.
    /// </summary>
    public DateTimeOffset LatestTimestamp { get; set; }

    /// <summary>
    /// Gets the list of history entries in this group.
    /// </summary>
    public ICollection<DownloadHistoryEntry> Entries { get; } = new List<DownloadHistoryEntry>();
}
