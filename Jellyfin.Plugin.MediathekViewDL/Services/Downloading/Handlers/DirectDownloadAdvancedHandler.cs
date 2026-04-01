using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Downloader;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Helpers;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;
using DownloadStatus = Downloader.DownloadStatus;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <inheritdoc />
public class DirectDownloadAdvancedHandler : BaseDownloadHandler
{
    private readonly ILogger<DirectDownloadHandler> _logger;
    private readonly IConfigurationProvider _configProvider;
    private readonly IServerApplicationPaths _appPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectDownloadAdvancedHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="appPaths">The application paths.</param>
    public DirectDownloadAdvancedHandler(ILogger<DirectDownloadHandler> logger, IConfigurationProvider configProvider, IServerApplicationPaths appPaths)
    {
        _logger = logger;
        _configProvider = configProvider;
        _appPaths = appPaths;
    }

    /// <inheritdoc />
    protected override DownloadType SupportedDownloadType { get => DownloadType.DirectDownload; }

    /// <inheritdoc />
    public override async Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var tempPath = TempFileHelper.GetTempFilePath(item.DestinationPath, ".mkv", _configProvider, _appPaths, _logger);
        var mbitToBytesFactor = (1000 * 1000) / 8; // Ergibt 125.000
        var dlConfig = new DownloadConfiguration()
        {
            MaximumBytesPerSecond = _configProvider.Configuration.Download.MaxBandwidthMBits * mbitToBytesFactor,
            FileExistPolicy = FileExistPolicy.Exception,
        };
        var downloader = new DownloadService(dlConfig);
        try
        {
            await downloader.DownloadFileTaskAsync(item.SourceUrl, tempPath,  cancellationToken).ConfigureAwait(false);
            if (downloader.Status != DownloadStatus.Completed)
            {
                return false;
            }

            File.Move(tempPath, item.DestinationPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract or move audio file for {DestinationPath}", item.DestinationPath);
            return false;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary audio file {TempPath}", tempPath);
                }
            }
        }
    }
}
