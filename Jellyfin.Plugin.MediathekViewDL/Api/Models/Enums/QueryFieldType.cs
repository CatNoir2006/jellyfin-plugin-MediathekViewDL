using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

/// <summary>
/// Defines the fields to search in.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueryFieldType
{
    /// <summary>
    /// Search in title.
    /// </summary>
    Title = 1 << 0,

    /// <summary>
    /// Search in topic (Sendung).
    /// </summary>
    Topic = 1 << 1,

    /// <summary>
    /// Search in description.
    /// </summary>
    Description = 1 << 2,

    /// <summary>
    /// Search in channel.
    /// </summary>
    Channel = 1 << 3
}
