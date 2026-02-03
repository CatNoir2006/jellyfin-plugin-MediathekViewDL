using System;
using System.Collections.ObjectModel;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Settings for searching the Mediathek.
/// </summary>
public record SearchSettings
{
    /// <summary>
    /// Gets the search criteria for the MediathekViewWeb API.
    /// </summary>
    public Collection<QueryFieldsDto> Criteria { get; init; } = new();

    /// <summary>
    /// Gets the minimum duration in minutes for search results.
    /// </summary>
    public int? MinDurationMinutes { get; init; }

    /// <summary>
    /// Gets the maximum duration in minutes for search results.
    /// </summary>
    public int? MaxDurationMinutes { get; init; }

    /// <summary>
    /// Gets the minimum broadcast date for search results.
    /// </summary>
    public DateTimeOffset? MinBroadcastDate { get; init; }

    /// <summary>
    /// Gets the maximum broadcast date for search results.
    /// </summary>
    public DateTimeOffset? MaxBroadcastDate { get; init; }
}
