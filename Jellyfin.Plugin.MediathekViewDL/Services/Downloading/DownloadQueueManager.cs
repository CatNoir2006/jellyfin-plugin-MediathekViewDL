using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
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
    private readonly Channel<ActiveDownload> _queueChannel;
    private readonly SemaphoreSlim _concurrencySemaphore = new(1, 1); // Limit to 1 concurrent download
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DownloadQueueManager> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly Task _queueProcessor;

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
        _queueChannel = Channel.CreateUnbounded<ActiveDownload>();
        _queueProcessor = Task.Run(ProcessQueueAsync);
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

                    await _concurrencySemaphore.WaitAsync(_shutdownCts.Token).ConfigureAwait(false);

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
                        download.Job.ItemInfo.Language).ConfigureAwait(false);
                }

                if (_configurationProvider.ConfigurationOrNull?.ScanLibraryAfterDownload == true && _activeDownloads.Values.All(d => d.Status != DownloadStatus.Queued))
                {
                    _logger.LogInformation("Triggering library scan (all downloads finished).");
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
