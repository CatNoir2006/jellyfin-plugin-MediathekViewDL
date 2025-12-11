using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Service for downloading files from a URL.
/// </summary>
public class FileDownloader : IFileDownloader
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
    /// Gets the plugin configuration.
    /// </summary>
    protected virtual PluginConfiguration? Configuration => Plugin.Instance?.Configuration;

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
        var pluginConfig = Configuration;

        // Validate file URL
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            _logger.LogError("File URL cannot be null or empty.");
            return false;
        }

        // Validate destination path
        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            _logger.LogError("Destination path cannot be null or empty for URL: {FileUrl}", fileUrl);
            return false;
        }

        // Validate plugin configuration
        if (pluginConfig is null)
        {
            _logger.LogError("Plugin configuration is not available.");
            return false;
        }

        // Check if the domain is allowed
        var domainAllowed = CheckDomainAllowed(fileUrl, pluginConfig, pluginConfig.AllowUnknownDomains);
        if (!pluginConfig.AllowUnknownDomains && !domainAllowed)
        {
            _logger.LogError("Download from unknown domain is not allowed: {FileUrl}", fileUrl);
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

            var diskSpace = GetDiskSpace(destinationPath);
            // Check if there is enough disk space before starting the download
            if (diskSpace < pluginConfig.MinFreeDiskSpaceBytes)
            {
                _logger.LogError(
                    "Insufficient disk space to download '{FileUrl}' to '{DestinationPath}'. Required: {RequiredBytes} bytes, Available: {AvailableBytes} bytes.",
                    fileUrl,
                    destinationPath,
                    pluginConfig?.MinFreeDiskSpaceBytes,
                    diskSpace);
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            // Check disk space again considering the file size
            if (totalBytes != -1)
            {
                long requiredSpace = totalBytes + pluginConfig.MinFreeDiskSpaceBytes;

                if (diskSpace < requiredSpace)
                {
                    _logger.LogError(
                        "Not enough disk space to download '{FileUrl}' to '{DestinationPath}'. " +
                        "Required: {RequiredBytes} bytes (File: {FileSize} + MinFree: {MinFree}), " +
                        "Available: {AvailableBytes}.",
                        fileUrl,
                        destinationPath,
                        requiredSpace,
                        totalBytes,
                        pluginConfig.MinFreeDiskSpaceBytes,
                        diskSpace);

                    return false;
                }
            }

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

    /// <summary>
    /// Generates a streaming URL file (.strm) at the specified destination path.
    /// </summary>
    /// <param name="fileUrl">The URL to be written into the streaming URL file.</param>
    /// <param name="destinationPath">The file path where the streaming URL file will be created.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>True if the streaming URL file was successfully created, otherwise false.</returns>
    public async Task<bool> GenerateStreamingUrlFileAsync(string fileUrl, string destinationPath, CancellationToken cancellationToken)
    {
        try
        {
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(destinationPath, fileUrl, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Successfully created streaming URL file at '{DestinationPath}'.", destinationPath);
            return true;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "File system error during creation of streaming URL file at '{DestinationPath}': {Message}", destinationPath, ex.Message);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Creation of streaming URL file at '{DestinationPath}' was cancelled.", destinationPath);
            // Clean up partially created file
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during creation of streaming URL file at '{DestinationPath}': {Message}", destinationPath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets the available free disk space in bytes for the drive containing the specified path.
    /// </summary>
    /// <param name="path">The path to check disk space for.</param>
    /// <returns>The available free disk space in bytes.</returns>
    public static long GetDiskSpace(string path)
    {
        string directory = path;

        if (string.IsNullOrWhiteSpace(directory))
        {
            return 0;
        }

#pragma warning disable CA3003 // The path is provieded by the Admin. Also there should be no issue with directory traversal. As we only check disk space, this is acceptable.
        if (!Directory.Exists(directory))
        {
            directory = Path.GetDirectoryName(path)!;
            if (string.IsNullOrWhiteSpace(directory))
            {
                return 0;
            }
        }
#pragma warning restore CA3003

        directory = Path.GetFullPath(directory);

        try
        {
            var drive = new DriveInfo(directory);
            return drive.AvailableFreeSpace;
        }
        catch (ArgumentException)
        {
            // This can happen for UNC paths, etc.
            return 0;
        }
    }

    private bool CheckDomainAllowed(string fileUrl, PluginConfiguration pluginConfig, bool isWarningOnly = false)
    {
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uriResult))
        {
            _logger.Log(isWarningOnly ? LogLevel.Warning : LogLevel.Error, "Invalid URL: {FileUrl}", fileUrl);
            return false;
        }

        var host = uriResult.Host;

        // Extract the top-level domain for validation
        var hostParts = host.Split('.');
        if (hostParts.Length < 2)
        {
            _logger.Log(isWarningOnly ? LogLevel.Warning : LogLevel.Error, "Invalid host in URL: {Host}", host);
            return false;
        }

        var topDomain = string.Join('.', hostParts[^2..]);

        if (!pluginConfig.AllowedDomains.Contains(topDomain))
        {
            _logger.Log(isWarningOnly ? LogLevel.Warning : LogLevel.Error, "Domain '{Domain}' is not allowed.", topDomain);
            return false;
        }

        return true;
    }
}
