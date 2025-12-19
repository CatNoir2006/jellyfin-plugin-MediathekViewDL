using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Data;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Manages the download queue and execution.
/// </summary>
public sealed class DownloadQueueManager : IDownloadQueueManager, IDisposable
{
    private readonly ConcurrentDictionary<Guid, ActiveDownload> _activeDownloads = new();
    private readonly ConcurrentQueue<ActiveDownload> _queue = new();
    private readonly SemaphoreSlim _concurrencySemaphore = new(2); // Limit to 2 concurrent downloads
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DownloadQueueManager> _logger;
    private readonly CancellationTokenSource _shutdownCts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadQueueManager"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger.</param>
    public DownloadQueueManager(
        IServiceScopeFactory scopeFactory,
        ILogger<DownloadQueueManager> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public void QueueJob(DownloadJob job, Guid? subscriptionId = null)
    {
        CleanupOldDownloads();

        var activeDownload = new ActiveDownload { Job = job, Status = DownloadStatus.Queued, SubscriptionId = subscriptionId };

        if (_activeDownloads.TryAdd(activeDownload.Id, activeDownload))
        {
            _queue.Enqueue(activeDownload);
            _logger.LogInformation("Queued download job '{Title}' (ID: {Id}).", job.Title, activeDownload.Id);
            _ = ProcessQueueAsync();
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
    public IEnumerable<ActiveDownload> GetActiveDownloads()
    {
        return _activeDownloads.Values.OrderByDescending(d => d.CreatedAt);
    }

    /// <summary>
    /// Disposes the manager.
    /// </summary>
    public void Dispose()
    {
        _shutdownCts.Cancel();
        _shutdownCts.Dispose();
        _concurrencySemaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task ProcessQueueAsync()
    {
        while (!_shutdownCts.IsCancellationRequested && _queue.TryPeek(out _))
        {
            await _concurrencySemaphore.WaitAsync(_shutdownCts.Token).ConfigureAwait(false);

            if (_queue.TryDequeue(out var download))
            {
                // Verify it hasn't been cancelled while in queue
                if (download.Status == DownloadStatus.Cancelled)
                {
                    _concurrencySemaphore.Release();
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
                            _concurrencySemaphore.Release();
                            // Trigger next check
                            _ = ProcessQueueAsync();
                        }
                    },
                    _shutdownCts.Token);
            }
            else
            {
                _concurrencySemaphore.Release();
            }
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
            var success = await downloadManager.ExecuteJobAsync(download.Job, progress, download.Cts.Token).ConfigureAwait(false);

            if (success)
            {
                download.Status = DownloadStatus.Finished;
                download.Progress = 100;

                // Save every item in the job to history
                foreach (var item in download.Job.DownloadItems)
                {
                    await historyRepository.AddAsync(
                        item.SourceUrl,
                        download.Job.ItemId,
                        download.SubscriptionId ?? Guid.Empty,
                        item.DestinationPath,
                        download.Job.Title,
                        download.Job.AudioLanguage).ConfigureAwait(false);
                }

                if (Plugin.Instance?.Configuration?.ScanLibraryAfterDownload == true)
                {
                    _logger.LogInformation("Triggering library scan (download finished).");
                    libraryManager.QueueLibraryScan();
                }
            }
            else if (download.Status != DownloadStatus.Cancelled)
            {
                download.Status = DownloadStatus.Failed;
                download.ErrorMessage = "Download failed (check logs).";
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
