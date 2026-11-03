using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;

/// <summary>
/// Manages the download queue and execution.
/// </summary>
public sealed class DownloadQueueManager : IDownloadQueueManager, IDisposable
{
    private readonly ConcurrentDictionary<Guid, ActiveDownload> _activeDownloads = new();
    private readonly Channel<ActiveDownload> _queueChannel;
    private readonly object _semaphoreLock = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DownloadQueueManager> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly Task _queueProcessor;
    private DynamicSemaphore? _semaphore;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadQueueManager"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    public DownloadQueueManager(
        IServiceScopeFactory scopeFactory,
        ILogger<DownloadQueueManager> logger,
        IConfigurationProvider configurationProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configurationProvider = configurationProvider;
        Plugin.ConfigurationSaved += OnConfigurationSaved;

        _queueChannel = Channel.CreateUnbounded<ActiveDownload>();
        _queueProcessor = Task.Run(ProcessQueueAsync);
    }

    /// <summary>
    /// Gets the concurrency semaphore, creating it on first use so the configured
    /// limit is read after the plugin configuration has been applied.
    /// </summary>
    /// <returns>The dynamic concurrency semaphore.</returns>
    private DynamicSemaphore GetSemaphore()
    {
        lock (_semaphoreLock)
        {
            if (_semaphore is null)
            {
                var limit = GetParallelDownloadLimit();
                _semaphore = new DynamicSemaphore(limit);
                _logger.LogInformation(
                    "Download queue initialized with a concurrency limit of {Limit} parallel download(s).",
                    limit);
            }

            return _semaphore;
        }
    }

    /// <inheritdoc />
    public void QueueJob(DownloadJob job, Guid? subscriptionId = null)
    {
        CleanupOldDownloads();

        var activeDownload = new ActiveDownload { Job = job, Status = DownloadStatus.Queued, SubscriptionId = subscriptionId };

        if (_activeDownloads.TryAdd(activeDownload.Id, activeDownload))
        {
            if (_queueChannel.Writer.TryWrite(activeDownload))
            {
                _logger.LogInformation("Queued download job '{Title}' (ID: {Id}).", job.Title, activeDownload.Id);
            }
            else
            {
                _logger.LogError("Failed to write download job '{Title}' (ID: {Id}) to channel.", job.Title, activeDownload.Id);
                activeDownload.Status = DownloadStatus.Failed;
                activeDownload.ErrorMessage = "Internal error: Queue full or closed.";
            }
        }
    }

    private void CleanupOldDownloads()
    {
        // Remove downloads that are finished/failed/cancelled and older than 24 hours
        var cutoff = DateTimeOffset.UtcNow.AddHours(-24);
        var keysToRemove = _activeDownloads
            .Where(kvp => (kvp.Value.Status == DownloadStatus.Finished ||
                           kvp.Value.Status == DownloadStatus.Failed ||
                           kvp.Value.Status == DownloadStatus.Cancelled) &&
                          kvp.Value.CreatedAt < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _activeDownloads.TryRemove(key, out _);
        }
    }

    /// <inheritdoc />
    public void CancelJob(Guid id)
    {
        if (_activeDownloads.TryGetValue(id, out var download))
        {
            if (download.Status == DownloadStatus.Finished || download.Status == DownloadStatus.Failed || download.Status == DownloadStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot cancel a download that is already in state '{download.Status}'.");
            }

            download.Cts.Cancel();
            download.Status = DownloadStatus.Cancelled;
            _logger.LogInformation("Cancelled download job '{Title}' (ID: {Id}).", download.Job.Title, id);
        }
        else
        {
            throw new KeyNotFoundException($"Download job with ID '{id}' not found.");
        }
    }

    /// <inheritdoc />
    public void CancelAllJobs()
    {
        _logger.LogInformation("Cancellation of all download jobs requested.");
        foreach (var download in _activeDownloads.Values)
        {
            if (download.Status == DownloadStatus.Queued ||
                download.Status == DownloadStatus.Downloading ||
                download.Status == DownloadStatus.Processing)
            {
                try
                {
                    download.Cts.Cancel();
                    download.Status = DownloadStatus.Cancelled;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling job '{Title}' (ID: {Id}).", download.Job.Title, download.Id);
                }
            }
        }
    }

    /// <inheritdoc />
    public void ClearInactiveJobs()
    {
        _logger.LogInformation("Clearing all inactive download jobs from list.");
        var keysToRemove = _activeDownloads
            .Where(kvp => kvp.Value.Status == DownloadStatus.Finished ||
                           kvp.Value.Status == DownloadStatus.Failed ||
                           kvp.Value.Status == DownloadStatus.Cancelled)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _activeDownloads.TryRemove(key, out _);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ActiveDownload> GetActiveDownloads()
    {
        return _activeDownloads.Values.OrderByDescending(d => d.CreatedAt);
    }

    /// <summary>
    /// Disposes the manager.
    /// </summary>
    public void Dispose()
    {
        Plugin.ConfigurationSaved -= OnConfigurationSaved;
        _shutdownCts.Cancel();
        _shutdownCts.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the limit of simultaneous downloads from the plugin configuration, clamped to the range 1..10.
    /// </summary>
    /// <returns>The configured download concurrency limit.</returns>
    private int GetParallelDownloadLimit()
    {
        const int AbsoluteMaximum = 10;

        var configured = _configurationProvider.ConfigurationOrNull?.Download.MaxParallelDownloads ?? 1;
        var limit = Math.Clamp(configured, 1, AbsoluteMaximum);

        if (limit != configured)
        {
            _logger.LogWarning(
                "Configured MaxParallelDownloads value {Configured} is outside the supported range 1..{Maximum}. Using {Limit} instead.",
                configured,
                AbsoluteMaximum,
                limit);
        }

        return limit;
    }

    /// <summary>
    /// Handles plugin configuration updates by adjusting the concurrency limit without a restart.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="eventArgs">The event arguments.</param>
    private void OnConfigurationSaved(object? sender, EventArgs eventArgs)
    {
        try
        {
            var newLimit = GetParallelDownloadLimit();
            lock (_semaphoreLock)
            {
                if (_semaphore is null)
                {
                    // The semaphore is created lazily on first use and will pick up the
                    // current configuration at that point.
                    return;
                }

                var oldLimit = _semaphore.CurrentLimit;
                if (oldLimit == newLimit)
                {
                    return;
                }

                _semaphore.SetLimit(newLimit);
                _logger.LogInformation(
                    "Adjusted the parallel download limit to {NewLimit} (previous limit: {OldLimit}).",
                    newLimit,
                    oldLimit);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply the updated download concurrency limit.");
        }
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            while (await _queueChannel.Reader.WaitToReadAsync(_shutdownCts.Token).ConfigureAwait(false))
            {
                while (_queueChannel.Reader.TryRead(out var download))
                {
                    if (download.Status == DownloadStatus.Cancelled)
                    {
                        continue;
                    }

                    await GetSemaphore().WaitAsync(_shutdownCts.Token).ConfigureAwait(false);

                    if (download.Status == DownloadStatus.Cancelled)
                    {
                        GetSemaphore().Release();
                        continue;
                    }

                    _ = Task.Run(
                        async () =>
                        {
                            try
                            {
                                await ExecuteDownloadAsync(download).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error executing download job '{Title}' (ID: {Id}).", download.Job.Title, download.Id);
                            }
                            finally
                            {
                                GetSemaphore().Release();
                            }
                        },
                        _shutdownCts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in download queue loop.");
        }
    }

    private async Task ExecuteDownloadAsync(ActiveDownload download)
    {
        download.Status = DownloadStatus.Downloading;
        _logger.LogInformation("Starting execution of download job '{Title}' (ID: {Id}).", download.Job.Title, download.Id);

        using var scope = _scopeFactory.CreateScope();
        var downloadManager = scope.ServiceProvider.GetRequiredService<IDownloadManager>();
        var historyRepository = scope.ServiceProvider.GetRequiredService<IDownloadHistoryRepository>();
        var libraryManager = scope.ServiceProvider.GetRequiredService<ILibraryManager>();

        var progress = new Progress<double>(p =>
        {
            download.Progress = p;
            if (p > 90 && download.Status == DownloadStatus.Downloading)
            {
                download.Status = DownloadStatus.Processing;
            }
        });

        try
        {
            var result = await downloadManager.ExecuteJobAsync(download.Job, progress, download.Cts.Token).ConfigureAwait(false);

            if (result.Success)
            {
                download.Status = DownloadStatus.Finished;
                download.Progress = 100;
                download.ItemResults = result.ItemResults;

                // Save every item in the job to history
                foreach (var item in download.Job.DownloadItems)
                {
                    await historyRepository.AddAsync(
                        item.SourceUrl,
                        download.Job.ItemId,
                        download.SubscriptionId ?? Guid.Empty,
                        item.DestinationPath,
                        download.Job.Title,
                        download.Job.ItemInfo.Language).ConfigureAwait(false);
                }

                // Only trigger a scan when no downloads are queued or still running.
                if (_configurationProvider.ConfigurationOrNull?.Download.ScanLibraryAfterDownload == true
                    && _activeDownloads.Values.All(d => d.Status != DownloadStatus.Queued
                        && d.Status != DownloadStatus.Downloading
                        && d.Status != DownloadStatus.Processing))
                {
                    _logger.LogInformation("Triggering library scan (all downloads finished).");
                    libraryManager.QueueLibraryScan();
                }
            }
            else if (download.Status != DownloadStatus.Cancelled)
            {
                download.Status = DownloadStatus.Failed;
                download.ErrorMessage = "Download fehlgeschlagen (Details siehe unten).";
                download.ItemResults = result.ItemResults;
            }
        }
        catch (OperationCanceledException)
        {
            download.Status = DownloadStatus.Cancelled;
        }
        catch (Exception ex)
        {
            download.Status = DownloadStatus.Failed;
            download.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Exception during download job '{Title}' (ID: {Id}).", download.Job.Title, download.Id);
        }
    }
}
