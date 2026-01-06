using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External;

/// <summary>
/// Interface for the MediathekView API client.
/// </summary>
public interface IMediathekViewApiClient
{
    /// <summary>
    /// Searches for media on the MediathekViewWeb API.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="topic">The topic filter.</param>
    /// <param name="channel">The channel filter.</param>
    /// <param name="minDuration">Optional minimum duration in seconds.</param>
    /// <param name="maxDuration">Optional maximum duration in seconds.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of result items.</returns>
    /// <exception cref="Exceptions.ExternalApi.MediathekException">Thrown when an error occurs while calling the API.</exception>
    Task<Collection<ResultItem>> SearchAsync(string? searchQuery, string? topic, string? channel, int? minDuration, int? maxDuration, CancellationToken cancellationToken);

    /// <summary>
    /// Searches for media on the MediathekViewWeb API using a specified query.
    /// </summary>
    /// <param name="apiQuery">The api query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result.</returns>
    /// <exception cref="Exceptions.ExternalApi.MediathekException">Thrown when an error occurs while calling the API.</exception>
    Task<ResultChannels> SearchAsync(ApiQuery apiQuery, CancellationToken cancellationToken);
}
