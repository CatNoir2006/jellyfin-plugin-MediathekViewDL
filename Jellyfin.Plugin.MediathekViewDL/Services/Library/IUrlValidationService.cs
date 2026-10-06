using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Interface for the UrlValidationService.
/// </summary>
public interface IUrlValidationService
{
    /// <summary>
    /// Validates a streaming URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the URL is valid and accessible, otherwise false.</returns>
    Task<bool> ValidateUrlAsync(string url, CancellationToken cancellationToken);
}
