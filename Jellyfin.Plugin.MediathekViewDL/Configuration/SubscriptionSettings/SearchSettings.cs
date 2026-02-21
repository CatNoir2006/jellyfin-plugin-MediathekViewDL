using System.Collections.ObjectModel;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Settings for searching the Mediathek within a subscription.
/// </summary>
public record SearchSettings : BaseSearchSettings
{
    /// <summary>
    /// Gets the search criteria for the MediathekViewWeb API.
    /// </summary>
    public Collection<QueryFieldsDto> Criteria { get; init; } = new();
}
