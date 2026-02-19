using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

#nullable disable

/// <summary>
/// Contains the results of a query.
/// </summary>
public class ResultChannels
{
    /// <summary>
    /// Gets the list of result items.
    /// </summary>
    [JsonPropertyName("results")]
    public Collection<ResultItem> Results { get; init; }

    /// <summary>
    /// Gets or sets the query info.
    /// </summary>
    [JsonPropertyName("queryInfo")]
    public QueryInfo QueryInfo { get; set; }
}
