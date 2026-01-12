using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Defines a filter for the search.
/// </summary>
public record QueryFieldsDto
{
    /// <summary>
    /// Gets or sets the fields to search in.
    /// </summary>
    public QueryFieldType Fields { get; set; }

    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    public string Query { get; set; } = string.Empty;
}
