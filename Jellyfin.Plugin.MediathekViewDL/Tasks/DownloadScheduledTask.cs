using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to process download subscriptions.
/// </summary>
public class DownloadScheduledTask : IScheduledTask
{
    private readonly ILogger<DownloadScheduledTask> _logger;
    private readonly ISubscriptionProcessor _subscriptionProcessor;
    private readonly IDownloadQueueManager _downloadQueueManager;
    private readonly IConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadScheduledTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    /// <param name="downloadQueueManager">The download queue manager.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    public DownloadScheduledTask(
        ILogger<DownloadScheduledTask> logger,
        ISubscriptionProcessor subscriptionProcessor,
        IDownloadQueueManager downloadQueueManager,
        IConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _subscriptionProcessor = subscriptionProcessor;
        _downloadQueueManager = downloadQueueManager;
        _configurationProvider = configurationProvider;
    }

    /// <inheritdoc />
    public string Name => "Mediathek Abo-Downloader";

    /// <inheritdoc />
    public string Key => "MediathekAboDownloader";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Sucht nach neuen Inhalten für Abonnements und fügt sie der Download-Warteschlange hinzu.";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Run every 6 hours
        yield return new TaskTriggerInfo { Type = TaskTriggerInfoType.IntervalTrigger, IntervalTicks = TimeSpan.FromHours(6).Ticks };
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Mediathek subscription download task.");
        progress.Report(0);

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null || config.Subscriptions.Count == 0)
        {
            _logger.LogInformation("No subscriptions configured. Task finished.");
            return;
        }

        var newLastRun = DateTime.UtcNow;
        var subscriptions = config.Subscriptions.ToList();
        var subscriptionProgressShare = subscriptions.Count > 0 ? 100.0 / subscriptions.Count : 0;

        for (int i = 0; i < subscriptions.Count; i++)
        {
            var subscription = subscriptions[i];

            if (!subscription.IsEnabled)
            {
                _logger.LogDebug("Skipping disabled subscription '{SubscriptionName}'.", subscription.Name);
                progress.Report((double)(i + 1) * subscriptionProgressShare);
                continue;
            }

            var baseProgressForSubscription = (double)i * subscriptionProgressShare;
            progress.Report(baseProgressForSubscription);

            _logger.LogInformation("Processing subscription: {SubscriptionName}", subscription.Name);

            // Step 1: Find jobs
            var jobs = await _subscriptionProcessor.GetJobsForSubscriptionAsync(
                subscription,
                config.DownloadSubtitles,
                cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Found {Count} new items for '{SubscriptionName}'.", jobs.Count, subscription.Name);

            var numJobs = jobs.Count;
            if (numJobs == 0)
            {
                progress.Report(baseProgressForSubscription + subscriptionProgressShare);
                continue;
            }

            // Step 2: Queue jobs
            foreach (var job in jobs)
            {
                _downloadQueueManager.QueueJob(job, subscription.Id);
            }

            subscription.LastDownloadedTimestamp = DateTime.UtcNow;

            progress.Report(baseProgressForSubscription + subscriptionProgressShare);
        }

        // Save the new timestamp
        config.LastRun = newLastRun;
        _configurationProvider.TryUpdate(config);

        progress.Report(100);
        _logger.LogInformation("Mediathek subscription discovery task finished. Jobs are in the download queue.");
    }
}
