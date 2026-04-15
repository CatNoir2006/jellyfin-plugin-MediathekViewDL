using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;

/// <inheritdoc />
public abstract class BaseDownloadHandler : IDownloadHandler
{
    /// <summary>
    /// Gets the SupportedDownloadType the Handler supports.
    /// </summary>
    protected abstract DownloadType SupportedDownloadType { get; }

    /// <summary>
    /// Gets a value indicating whether this handler is enabled.
    /// This only affects CanHandel by default.
    /// </summary>
    protected virtual bool IsEnabled => true;

    /// <inheritdoc />
    public bool CanHandle(DownloadType downloadType)
    {
        return IsEnabled && downloadType == SupportedDownloadType;
    }

    /// <inheritdoc />
    public abstract Task<bool> ExecuteAsync(DownloadItem item, DownloadJob job, IProgress<double> progress, CancellationToken cancellationToken);
}
