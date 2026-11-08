using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to synchronize all available ARD/ZDF Mediathek episodes into the dedicated virtual streaming library.
/// </summary>
public class SyncVirtualMediathekLibraryTask : IScheduledTask
{
    private readonly ILogger<SyncVirtualMediathekLibraryTask> _logger;
    private readonly IVirtualLibrarySyncService _syncService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncVirtualMediathekLibraryTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="syncService">The virtual library sync service.</param>
    public SyncVirtualMediathekLibraryTask(
        ILogger<SyncVirtualMediathekLibraryTask> logger,
        IVirtualLibrarySyncService syncService)
    {
        _logger = logger;
        _syncService = syncService;
    }

    /// <inheritdoc />
    public string Name => "Mediathek Virtuelle Bibliothek Sync (.strm & .nfo)";

    /// <inheritdoc />
    public string Key => "MediathekVirtualLibrarySync";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Erstellt und synchronisiert leichtgewichtige .strm Stream-Links und .nfo Metadaten aller verfügbaren Mediathek-Folgen für eine eigene Jellyfin-Bibliothek.";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.IntervalTrigger,
                IntervalTicks = TimeSpan.FromHours(24).Ticks
            }
        };
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scheduled virtual library synchronization...");
        var count = await _syncService.SyncVirtualLibraryAsync(cancellationToken, progress).ConfigureAwait(false);
        _logger.LogInformation("Completed scheduled virtual library synchronization. Synced {Count} items.", count);
        progress.Report(100);
    }
}
