using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Represents a cached quality entry for a video URL.
/// </summary>
[Index(nameof(UrlHash))]
public class QualityCacheEntry
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the hash of the URL.
    /// </summary>
    [Required]
    public string UrlHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width of the video.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the video.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the last updated timestamp.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }
}
