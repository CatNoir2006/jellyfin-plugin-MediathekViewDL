using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Represents the type of media URL.
/// </summary>
public enum UrlType
{
    /// <summary>
    /// Video.
    /// </summary>
    Video,

    /// <summary>
    /// VideoLow.
    /// </summary>
    VideoLow,

    /// <summary>
    /// VideoHd.
    /// </summary>
    VideoHd,

    /// <summary>
    /// Subtitle.
    /// </summary>
    Subtitle
}

/// <summary>
/// Represents a unique media URL.
/// </summary>
[Index(nameof(Url), IsUnique = true)]
public class MediaUrl
{
    /// <summary>
    /// Gets or sets the unique internal identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the URL string.
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the URL.
    /// </summary>
    public UrlType Type { get; set; }

    /// <summary>
    /// Gets the collection of media item URLs.
    /// </summary>
    public ICollection<MediaItemUrl> MediaItemUrls { get; } = new List<MediaItemUrl>();
}
