using System.Collections.Generic;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Data transfer object for query results including metadata.
/// </summary>
public record QueryResultDto
{
    /// <summary>
    /// Gets the query information.
    /// </summary>
    public required QueryInfoDto QueryInfo { get; init; }

    /// <summary>
    /// Gets the list of result items.
    /// </summary>
    public required IReadOnlyList<ResultItemDto> Results { get; init; }
}
