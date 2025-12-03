using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service for downloading files from a URL.
/// </summary>
public class FileDownloader
{
    private readonly ILogger<FileDownloader> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownloader"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The http client factory.</param>
    public FileDownloader(ILogger<FileDownloader> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Downloads a file from a URL to a specified destination path.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to download.</param>
    /// <param name="destinationPath">The full path where the file should be saved.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the download was successful, otherwise false.</returns>
    public async Task<bool> DownloadFileAsync(string fileUrl, string destinationPath, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            _logger.LogError("File URL cannot be null or empty.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            _logger.LogError("Destination path cannot be null or empty for URL: {FileUrl}", fileUrl);
            return false;
        }

        _logger.LogInformation("Starting download of '{FileUrl}' to '{DestinationPath}'", fileUrl, destinationPath);

        try
        {
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var receivedBytes = 0L;
            Memory<byte> buffer = new byte[8192];

            #pragma warning disable CA2007 // Aufruf von "ConfigureAwait" für erwarteten Task erwägen
            await using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                #pragma warning disable CA2007 // Aufruf von "ConfigureAwait" für erwarteten Task erwägen
                await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                {
                    while (true)
                    {
                        var read = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                        if (read == 0)
                        {
                            break;
                        }

                        await fileStream.WriteAsync(buffer.Slice(0, read), cancellationToken).ConfigureAwait(false);
                        receivedBytes += read;
                        if (totalBytes != -1)
                        {
                            progress?.Report((double)receivedBytes / totalBytes * 100);
                        }
                    }
                }
                #pragma warning restore CA2007 // Aufruf von "ConfigureAwait" für erwarteten Task erwägen
            }
            #pragma warning restore CA2007 // Aufruf von "ConfigureAwait" für erwarteten Task erwägen

            _logger.LogInformation("Successfully downloaded '{DestinationPath}'.", destinationPath);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed during download of '{FileUrl}': {Message}", fileUrl, ex.Message);
            return false;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File system error during download of '{FileUrl}' to '{DestinationPath}': {Message}", fileUrl, destinationPath, ex.Message);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Download of '{FileUrl}' to '{DestinationPath}' was cancelled.", fileUrl, destinationPath);
            // Clean up partially downloaded file
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during download of '{FileUrl}': {Message}", fileUrl, ex.Message);
            return false;
        }
    }
}
