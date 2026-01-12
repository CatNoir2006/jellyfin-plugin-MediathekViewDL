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
}
