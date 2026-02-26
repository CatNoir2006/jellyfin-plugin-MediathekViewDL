using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Api.External;

/// <summary>
/// Interface for the MediathekView API client.
/// </summary>
public interface IMediathekViewApiClient
{
    /// <summary>
    /// Searches for media on the MediathekViewWeb API using a specified query.
    /// </summary>
    /// <param name="apiQueryDto">The api query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result.</returns>
    /// <exception cref="Exceptions.ExternalApi.MediathekException">Thrown when an error occurs while calling the API.</exception>
    Task<QueryResultDto> SearchAsync(ApiQueryDto apiQueryDto, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the size of a media stream from a given URL.
    /// </summary>
    /// <param name="streamUrl">The Url to get size of.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fileSize of the MediaItem in Bytes.</returns>
    Task<long> GetStreamSizeAsync(string streamUrl, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the list of available channels from the Zapp API.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of Zapp channels.</returns>
    Task<IReadOnlyCollection<ZappChannelDto>> GetZappChannelsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current show for a specific channel from the Zapp API.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current show, or null if not found.</returns>
    Task<IReadOnlyCollection<ZappShowDto>> GetCurrentZappShowAsync(string channelId, CancellationToken cancellationToken);
}
