using System.Collections.ObjectModel;
using MediathekView.Api.Models.Enums;

namespace MediathekView.Api.Models;

/// <summary>
/// Defines a filter for the search.
/// </summary>
public record QueryFieldsDto
{
    /// <summary>
    /// Gets the fields to search in.
    /// </summary>
    public Collection<QueryFieldType> Fields { get; init; } = new();

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is an exclusion filter (NOT).
    /// </summary>
    public bool IsExclude { get; set; }
}
