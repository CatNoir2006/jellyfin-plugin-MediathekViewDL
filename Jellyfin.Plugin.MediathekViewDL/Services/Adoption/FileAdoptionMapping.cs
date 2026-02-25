namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Represents a mapping between a local file group and an API ID.
/// </summary>
public record FileAdoptionMapping
{
    /// <summary>
    /// Gets the identifier of the local candidate group.
    /// </summary>
    public required string CandidateId { get; init; }

    /// <summary>
    /// Gets the identifier of the matched item in the external API.
    /// </summary>
    public required string ApiId { get; init; }

    /// <summary>
    /// Gets the original video URL, if available.
    /// </summary>
    public string? VideoUrl { get; init; }
}
