using System.Collections.Generic;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Contains all information needed for the adoption UI, including local candidates and all available API results.
/// </summary>
public record AdoptionInfo
{
    /// <summary>
    /// Gets the list of local file groups (candidates) and their matches.
    /// </summary>
    public required IReadOnlyList<AdoptionCandidate> Candidates { get; init; }

    /// <summary>
    /// Gets the list of all API results found for the subscription.
    /// </summary>
    public required IReadOnlyList<ApiResultWithInfo> ApiResults { get; init; }
}
