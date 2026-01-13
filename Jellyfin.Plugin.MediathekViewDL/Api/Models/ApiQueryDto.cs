using System.Collections.ObjectModel;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Defines the DTO for the MediathekViewWeb API query.
/// </summary>
public record ApiQueryDto
{
    /// <summary>
    /// Gets the list of queries to filter the search.
    /// </summary>
    public Collection<QueryFieldsDto> Queries { get; init; } = new();

    /// <summary>
    /// Gets or sets the field to sort by.
    /// </summary>
    public SortBy SortBy { get; set; } = SortBy.Timestamp;

    /// <summary>
    /// Gets or sets the sort order.
    /// </summary>
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;

    /// <summary>
    /// Gets or sets a value indicating whether to include future broadcasts.
    /// </summary>
    public bool Future { get; set; }

    /// <summary>
    /// Gets or sets the offset for pagination.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Gets or sets the number of results to return.
    /// </summary>
    public int Size { get; set; } = 25;

    /// <summary>
    /// Gets or sets the minimum duration in seconds for search results.
    /// </summary>
    public int? MinDuration { get; set; }

    /// <summary>
    /// Gets or sets the maximum duration in seconds for search results.
    /// </summary>
    public int? MaxDuration { get; set; }
}
