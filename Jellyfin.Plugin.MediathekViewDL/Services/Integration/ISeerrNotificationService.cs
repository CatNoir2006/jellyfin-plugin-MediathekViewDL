using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Integration;

/// <summary>
/// Service to notify Seerr / Jellyseerr when new media or streams are provisioned.
/// </summary>
public interface ISeerrNotificationService
{
    /// <summary>
    /// Triggers a recently-added library scan in Jellyseerr / Seerr.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if successfully triggered or ignored, false if error occurred.</returns>
    Task<bool> TriggerRecentlyAddedScanAsync(CancellationToken cancellationToken);
}
