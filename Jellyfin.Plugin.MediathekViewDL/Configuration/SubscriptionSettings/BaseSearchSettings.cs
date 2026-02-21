using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Base settings for searching the Mediathek.
/// </summary>
public record BaseSearchSettings
{
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
