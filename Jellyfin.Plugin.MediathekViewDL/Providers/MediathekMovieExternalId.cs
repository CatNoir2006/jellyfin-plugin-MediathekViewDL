using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.MediathekViewDL.Providers;

/// <summary>
/// Registers the Mediathek ID provider for movies.
/// </summary>
public class MediathekMovieExternalId : IExternalId
{
    /// <summary>
    /// Gets the name of the external ID provider.
    /// </summary>
    public string ProviderName => "Mediathek";

    /// <summary>
    /// Gets the unique key of the external ID.
    /// </summary>
    public string Key => "MediathekId";

    /// <summary>
    /// Gets the media type of the external ID.
    /// </summary>
    public ExternalIdMediaType? Type => ExternalIdMediaType.Movie;

    /// <summary>
    /// Gets the URL format string.
    /// </summary>
    public string? UrlFormatString => null;

    /// <summary>
    /// Determines whether the item is supported by this provider.
    /// </summary>
    /// <param name="item">The item to test.</param>
    /// <returns>True if the item is supported.</returns>
    public bool Supports(IHasProviderIds item) => item is Movie;
}
