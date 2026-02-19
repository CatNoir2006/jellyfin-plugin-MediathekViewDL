using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Represents a history entry for a downloaded item.
/// </summary>
[Index(nameof(VideoUrlHash))]
[Index(nameof(SubscriptionId))]
public class DownloadHistoryEntry
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the subscription identifier.
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the video URL.
    /// </summary>
    [Required]
    public string VideoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the video URL.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string VideoUrlHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path where the file was downloaded.
    /// </summary>
    public string DownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item identifier from the provider.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the downloaded item.
    /// </summary>
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language code of the downloaded item.
    /// </summary>
    [MaxLength(3)]
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the download was recorded.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
