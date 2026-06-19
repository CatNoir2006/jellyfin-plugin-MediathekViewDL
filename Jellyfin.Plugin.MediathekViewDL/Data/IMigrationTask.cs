using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Interface for a database or configuration migration task.
/// </summary>
public interface IMigrationTask
{
    /// <summary>
    /// Gets the unique name of the migration task.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the migration task.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(MediathekViewDlDbContext context, CancellationToken ct);
}
