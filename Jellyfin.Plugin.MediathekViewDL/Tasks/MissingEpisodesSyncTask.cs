using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to scan existing series and sync missing episodes from ARD/ZDF Mediathek.
/// </summary>
public class MissingEpisodesSyncTask : IScheduledTask
{
    private readonly ILogger<MissingEpisodesSyncTask> _logger;
    private readonly IMissingEpisodeService _missingEpisodeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MissingEpisodesSyncTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="missingEpisodeService">The missing episode service.</param>
    public MissingEpisodesSyncTask(
        ILogger<MissingEpisodesSyncTask> logger,
        IMissingEpisodeService missingEpisodeService)
    {
        _logger = logger;
        _missingEpisodeService = missingEpisodeService;
    }

    /// <inheritdoc />
    public string Name => "Mediathek Fehlende Folgen Sync";

    /// <inheritdoc />
    public string Key => "MediathekMissingEpisodesSync";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Sucht in bestehenden Jellyfin-Serien nach fehlenden Episoden und lädt diese automatisch herunter oder verknüpft sie per Stream-Link.";

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
        _logger.LogInformation("Starting scheduled missing episodes scan...");
        var count = await _missingEpisodeService.SyncMissingEpisodesAsync(cancellationToken, progress).ConfigureAwait(false);
        _logger.LogInformation("Completed scheduled missing episodes scan. Synced/enqueued {Count} episodes.", count);
        progress.Report(100);
    }
}
