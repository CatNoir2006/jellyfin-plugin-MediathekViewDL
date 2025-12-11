using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Interface for the FileDownloader service.
/// </summary>
public interface IFileDownloader
{
    /// <summary>
    /// Downloads a file from a URL to a specified destination path.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to download.</param>
    /// <param name="destinationPath">The full path where the file should be saved.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the download was successful, otherwise false.</returns>
    Task<bool> DownloadFileAsync(string fileUrl, string destinationPath, IProgress<double>? progress, CancellationToken cancellationToken);

    /// <summary>
    /// Generates a streaming URL file (.strm) at the specified destination path.
    /// </summary>
    /// <param name="fileUrl">The URL to be written into the streaming URL file.</param>
    /// <param name="destinationPath">The file path where the streaming URL file will be created.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>True if the streaming URL file was successfully created, otherwise false.</returns>
    Task<bool> GenerateStreamingUrlFileAsync(string fileUrl, string destinationPath, CancellationToken cancellationToken);
}
