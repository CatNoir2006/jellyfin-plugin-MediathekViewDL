using System;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Represents a single download subscription based on a search query.
/// </summary>
public class Subscription
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Subscription"/> class.
    /// </summary>
    public Subscription()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        SearchQuery = string.Empty;
        DownloadPath = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the subscription.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user-defined name for the subscription. Used for the series folder name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the search query for the MediathekViewWeb API.
    /// Todo: Replace with Collection of Filters and add min and max length.
    /// </summary>
    public string SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the specific download path for this subscription. Overrides the default path if set.
    /// </summary>
    public string DownloadPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to only download content where season and episode can be parsed from the title.
    /// </summary>
    public bool EnforceSeriesParsing { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to allow downloading versions with audio descriptions.
    /// </summary>
    public bool AllowAudioDescription { get; set; }
}
