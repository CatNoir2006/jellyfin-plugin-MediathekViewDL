using System.Collections.Generic;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Represents a candidate for file adoption, grouping a video with its related files (e.g., subtitles).
/// </summary>
public record AdoptionCandidate
{
    /// <summary>
    /// Gets the unique identifier for this candidate group (e.g., the primary video file path).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the list of all files belonging to this candidate group.
    /// </summary>
    public required IReadOnlyList<string> FilePaths { get; init; }

    /// <summary>
    /// Gets the list of match information for this candidate, ordered by confidence.
    /// </summary>
    public required IReadOnlyList<AdoptionMatch> Matches { get; init; }
}
