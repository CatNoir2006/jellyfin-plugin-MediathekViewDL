namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Data transfer object for Zapp channels.
/// </summary>
public record ZappChannelDto
{
    /// <summary>
    /// Gets the ID of the channel.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name of the channel.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the stream URL of the channel.
    /// </summary>
    public required string StreamUrl { get; init; }
}
