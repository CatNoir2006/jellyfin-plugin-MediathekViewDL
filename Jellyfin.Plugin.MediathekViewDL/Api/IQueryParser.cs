using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// Interface for parsing search strings into query fields.
/// </summary>
public interface IQueryParser
{
    /// <summary>
    /// Parses the search parameters into a list of <see cref="QueryFieldsDto"/>.
    /// </summary>
    /// <param name="title">The title search string.</param>
    /// <param name="topic">The topic search string.</param>
    /// <param name="channel">The channel search string.</param>
    /// <param name="combinedSearch">The combined title and topic search string.</param>
    /// <returns>A list of query fields.</returns>
    Collection<QueryFieldsDto> Parse(string? title, string? topic, string? channel, string? combinedSearch);
}
