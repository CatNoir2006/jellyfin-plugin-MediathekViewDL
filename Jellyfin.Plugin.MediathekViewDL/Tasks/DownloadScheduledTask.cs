using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Tasks;

/// <summary>
/// Scheduled task to process download subscriptions.
/// </summary>
public class DownloadScheduledTask : IScheduledTask
{
    private readonly ILogger<DownloadScheduledTask> _logger;
    private readonly ILibraryManager _libraryManager;
    private readonly ISubscriptionProcessor _subscriptionProcessor;
    private readonly IDownloadManager _downloadManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadScheduledTask"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    /// <param name="downloadManager">The download manager.</param>
    public DownloadScheduledTask(
        ILogger<DownloadScheduledTask> logger,
        ILibraryManager libraryManager,
        ISubscriptionProcessor subscriptionProcessor,
        IDownloadManager downloadManager)
    {
        _logger = logger;
        _libraryManager = libraryManager;
        _subscriptionProcessor = subscriptionProcessor;
        _downloadManager = downloadManager;
    }

    /// <inheritdoc />
    public string Name => "Mediathek Abo-Downloader";

    /// <inheritdoc />
    public string Key => "MediathekAboDownloader";

    /// <inheritdoc />
    public string Category => "Mediathek Downloader";

    /// <inheritdoc />
    public string Description => "Sucht nach neuen Inhalten für Abonnements und lädt sie herunter.";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Run every 6 hours
        yield return new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerInterval, IntervalTicks = TimeSpan.FromHours(6).Ticks };
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Mediathek subscription download task.");
        progress.Report(0);

        var config = Plugin.Instance?.Configuration;
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

            var progressPerJob = subscriptionProgressShare / numJobs;
            var hasDownloadedAnyItem = false;

            // Step 2: Execute jobs
            for (int j = 0; j < numJobs; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var job = jobs[j];
                var baseProgressForJob = baseProgressForSubscription + (j * progressPerJob);

                var jobProgress = new Progress<double>(p =>
                {
                    var itemDownloadProgress = p / 100.0 * progressPerJob;
                    progress.Report(baseProgressForJob + itemDownloadProgress);
                });

                if (await _downloadManager.ExecuteJobAsync(job, jobProgress, cancellationToken).ConfigureAwait(false))
                {
                    subscription.ProcessedItemIds.Add(job.ItemId);
                    hasDownloadedAnyItem = true;
                }

                progress.Report(baseProgressForJob + progressPerJob);
            }

            if (hasDownloadedAnyItem)
            {
                subscription.LastDownloadedTimestamp = DateTime.UtcNow;
            }
        }

        // Save the new timestamp
        config.LastRun = newLastRun;
        Plugin.Instance?.UpdateConfiguration(config);

        // Trigger library scans
        if (config.ScanLibraryAfterDownload)
        {
            _logger.LogInformation("Triggering library scan");
            _libraryManager.QueueLibraryScan();
        }
        else
        {
            _logger.LogInformation("Library scan skipped (configured in settings).");
        }

        progress.Report(100);
        _logger.LogInformation("Mediathek subscription download task finished.");
    }
}
