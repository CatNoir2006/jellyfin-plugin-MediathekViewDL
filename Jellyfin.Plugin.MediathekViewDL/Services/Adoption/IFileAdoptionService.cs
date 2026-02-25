using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Service for adopting local files that were not downloaded by the plugin or are missing history entries.
/// </summary>
public interface IFileAdoptionService
{
    /// <summary>
    /// Loads local files for a subscription, grouped by video and its related files (e.g., subtitles).
    /// Each group includes matching information to an API result.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="AdoptionInfo"/> object containing candidates and API results.</returns>
    Task<AdoptionInfo> GetAdoptionCandidatesAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the API ID for a specific local file group.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription.</param>
    /// <param name="candidateId">The identifier of the candidate group.</param>
    /// <param name="apiId">The identifier of the matched item in the external API.</param>
    /// <param name="videoUrl">The original video URL, if available.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetApiIdAsync(Guid subscriptionId, string candidateId, string apiId, string? videoUrl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple mappings between local file groups and API IDs.
    /// </summary>
    /// <param name="subscriptionId">The unique identifier of the subscription.</param>
    /// <param name="mappings">The list of mappings to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetMappingsAsync(Guid subscriptionId, IReadOnlyList<FileAdoptionMapping> mappings, CancellationToken cancellationToken = default);
}
