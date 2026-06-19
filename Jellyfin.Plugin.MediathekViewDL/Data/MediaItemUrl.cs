namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Join table representing the M:N relationship between MediaItem and MediaUrl.
/// </summary>
public class MediaItemUrl
{
    /// <summary>
    /// Gets or sets the media item identifier.
    /// </summary>
    public int MediaItemId { get; set; }

    /// <summary>
    /// Gets or sets the media item.
    /// </summary>
    public MediaItem MediaItem { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media URL identifier.
    /// </summary>
    public int MediaUrlId { get; set; }

    /// <summary>
    /// Gets or sets the media URL.
    /// </summary>
    public MediaUrl MediaUrl { get; set; } = null!;
}
