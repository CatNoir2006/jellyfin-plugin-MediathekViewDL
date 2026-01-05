using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <summary>
/// Interface for handling specific download types.
/// </summary>
public interface IDownloadHandler
{
    /// <summary>
    /// Determines whether this handler can handle the specified download type.
    /// </summary>
    /// <param name="downloadType">The type of the download.</param>
    /// <returns><c>true</c> if this handler can handle the specified download type; otherwise, <c>false</c>.</returns>
    bool CanHandle(DownloadType downloadType);

    /// <summary>
    /// Executes the download.
    /// </summary>
    /// <param name="item">The download item.</param>
    /// <param name="job">The parent download job.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The result contains true if successful, otherwise false.</returns>
    Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken);
}
