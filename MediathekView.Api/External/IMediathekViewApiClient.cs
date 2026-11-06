using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediathekView.Api.Models;

namespace MediathekView.Api.External;

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
    /// <exception cref="MediathekView.Api.Exceptions.MediathekException">Thrown when an error occurs while calling the API.</exception>
    Task<QueryResultDto> SearchAsync(ApiQueryDto apiQueryDto, CancellationToken cancellationToken);

    /// <summary>
    /// Validates the given or configured MediathekViewWeb server URL by calling its stats health endpoint.
    /// The server must respond with status code 200 and the body "Server is up and running".
    /// </summary>
    /// <param name="baseUrlOverride">Optional base URL to validate. When null or empty, the configured base URL is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous check.</returns>
    /// <exception cref="MediathekView.Api.Exceptions.MediathekException">Thrown when the server is not reachable or responds unexpectedly.</exception>
    Task CheckConnectionAsync(string? baseUrlOverride, CancellationToken cancellationToken);

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
    /// Gets the list of available channels from the MediathekViewWeb API.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of channel names.</returns>
    Task<IReadOnlyCollection<string>> GetChannelsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the list of available topics from the MediathekViewWeb API.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of topic names.</returns>
    Task<IReadOnlyCollection<string>> GetTopicsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current show for a specific channel from the Zapp API.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current show, or null if not found.</returns>
    Task<IReadOnlyCollection<ZappShowDto>> GetCurrentZappShowAsync(string channelId, CancellationToken cancellationToken);
}
