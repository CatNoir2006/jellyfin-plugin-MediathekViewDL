using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Represents a canonical media item from MediathekViewWeb.
/// </summary>
[Index(nameof(MvwItemId), IsUnique = true)]
public class MediaItem
{
    /// <summary>
    /// Gets or sets the unique internal identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the item identifier from the provider.
    /// </summary>
    [Required]
    public string MvwItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original title of the item.
    /// </summary>
    public string OriginalTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parsed title of the item.
    /// </summary>
    public string ParsedTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the item.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the item.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the duration of the item in seconds.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets the language code of the item.
    /// </summary>
    public string Language { get; set; } = "deu";

    /// <summary>
    /// Gets the collection of media item URLs.
    /// </summary>
    public ICollection<MediaItemUrl> MediaItemUrls { get; } = new List<MediaItemUrl>();
}
