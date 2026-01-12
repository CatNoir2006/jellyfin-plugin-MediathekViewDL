namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Defines the relation of the total results count.
/// </summary>
public enum TotalRelation
{
    /// <summary>
    /// Total results are equal to the count (eq).
    /// </summary>
    Equal,

    /// <summary>
    /// Total results are greater than the count (gte).
    /// </summary>
    GreaterThan,
}
