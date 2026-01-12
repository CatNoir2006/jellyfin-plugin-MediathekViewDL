using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

/// <summary>
/// Defines the fields to search in.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueryFieldType
{
    /// <summary>
    /// Search in title.
    /// </summary>
    Title,

    /// <summary>
    /// Search in topic (Sendung).
    /// </summary>
    Topic,

    /// <summary>
    /// Search in description.
    /// </summary>
    Description,

    /// <summary>
    /// Search in channel.
    /// </summary>
    Channel
}
