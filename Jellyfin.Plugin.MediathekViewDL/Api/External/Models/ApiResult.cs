using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

#nullable disable

/// <summary>
/// Defines the root response object from the MediathekViewWeb API.
/// </summary>
public class ApiResult
{
    /// <summary>
    /// Gets or sets any error details from the API response.
    /// </summary>
    [JsonPropertyName("err")]
    public object Error { get; set; }

    /// <summary>
    /// Gets or sets the main result object from the API response.
    /// </summary>
    [JsonPropertyName("result")]
    public ResultChannels Result { get; set; }
}
