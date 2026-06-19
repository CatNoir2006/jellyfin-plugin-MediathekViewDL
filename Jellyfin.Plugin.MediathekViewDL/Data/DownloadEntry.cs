using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Represents a download record in the new normalized database structure.
/// </summary>
[Index(nameof(SubscriptionId))]
public class DownloadEntry
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
    /// Gets or sets the media item identifier.
    /// </summary>
    public int MediaItemId { get; set; }

    /// <summary>
    /// Gets or sets the associated media item.
    /// </summary>
    public MediaItem MediaItem { get; set; } = null!;

    /// <summary>
    /// Gets or sets the path where the file was downloaded.
    /// </summary>
    [Required]
    public string DownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the download was recorded.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}
