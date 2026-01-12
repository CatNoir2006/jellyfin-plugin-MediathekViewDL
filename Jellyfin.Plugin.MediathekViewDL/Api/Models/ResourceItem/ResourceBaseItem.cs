using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;

/// <summary>
/// Base item for resources.
/// </summary>
public abstract record ResourceBaseItem
{
    /// <summary>
    /// Gets the URL of the resource.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the language of the resource.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the features associated with the item.
    /// </summary>
    public ItemFeatures Flags { get; init; }
}
