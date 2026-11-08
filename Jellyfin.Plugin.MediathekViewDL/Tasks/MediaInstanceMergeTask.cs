using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to merge multi-source media instances and eliminate duplicates.
/// </summary>
public class MediaInstanceMergeTask : IScheduledTask
{
    private readonly ILogger<MediaInstanceMergeTask> _logger;
    private readonly IMediaMergeService _mergeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaInstanceMergeTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mergeService">The media merge service.</param>
    public MediaInstanceMergeTask(
        ILogger<MediaInstanceMergeTask> logger,
        IMediaMergeService mergeService)
    {
        _logger = logger;
        _mergeService = mergeService;
    }

    /// <inheritdoc />
    public string Name => "Mediathek Multi-Source Instanz Merging";

    /// <inheritdoc />
    public string Key => "MediathekMultiSourceMerge";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Prüft auf Duplikate zwischen lokalen Videodateien (Prio 1) und Stream-Verknüpfungen (Prio 2) und führt diese sauber zusammen.";

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
        _logger.LogInformation("Starting scheduled multi-source media merging task...");
        var count = await _mergeService.MergeMediaInstancesAsync(cancellationToken, progress).ConfigureAwait(false);
        _logger.LogInformation("Completed multi-source media merging. Merged/cleaned {Count} items.", count);
        progress.Report(100);
    }
}
