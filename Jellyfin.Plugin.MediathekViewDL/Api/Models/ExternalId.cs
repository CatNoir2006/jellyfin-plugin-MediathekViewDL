namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Represents an external identifier for a media item.
/// </summary>
public record ExternalId
{
    /// <summary>
    /// Gets the source of the external ID.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets the identifier value.
    /// </summary>
    public required string Id { get; init; }
}
