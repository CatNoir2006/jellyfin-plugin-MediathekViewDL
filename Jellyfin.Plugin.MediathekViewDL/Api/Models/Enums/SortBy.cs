namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

/// <summary>
/// Defines the field to sort by.
/// </summary>
public enum SortBy
{
    /// <summary>
    /// Sort by timestamp (broadcast date).
    /// </summary>
    Timestamp,

    /// <summary>
    /// Sort by duration.
    /// </summary>
    Duration,

    /// <summary>
    /// Sort by channel.
    /// </summary>
    Channel
}
