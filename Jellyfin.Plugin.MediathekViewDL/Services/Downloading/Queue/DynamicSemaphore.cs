using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;

/// <summary>
/// A semaphore whose maximum concurrency limit can be adjusted at runtime.
/// Admission happens while the number of holders is below the limit; otherwise
/// waiting requests are admitted in FIFO order whenever a holder is released
/// or the limit is raised.
/// </summary>
public sealed class DynamicSemaphore
{
    private readonly object _lock = new();
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private int _activeCount;
    private int _maxLimit;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicSemaphore"/> class.
    /// </summary>
    /// <param name="initialLimit">The initial limit of concurrent holders. Values below 1 are treated as 1.</param>
    public DynamicSemaphore(int initialLimit)
    {
        _maxLimit = Math.Max(1, initialLimit);
    }

    /// <summary>
    /// Gets the current maximum number of concurrent holders.
    /// </summary>
    public int CurrentLimit
    {
        get
        {
            lock (_lock)
            {
                return _maxLimit;
            }
        }
    }

    /// <summary>
    /// Gets the number of holders that currently occupy a slot, including promoted waiters.
    /// </summary>
    public int ActiveCount
    {
        get
        {
            lock (_lock)
            {
                return _activeCount;
            }
        }
    }

    /// <summary>
    /// Waits for a slot; returns immediately when the active count is below the current limit.
    /// The task completes once a slot has been assigned to the caller.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task representing the slot acquisition.</returns>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> waitTaskSource;

        lock (_lock)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_activeCount < _maxLimit)
            {
                _activeCount++;
                return;
            }

            waitTaskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(waitTaskSource);
        }

        // Cancelled waiters are removed on the next promotion instead of inside this callback,
        // so the cancellation handler stays free of queue modifications beyond the task state itself.
        await using (cancellationToken.Register(() =>
        {
            lock (_lock)
            {
                waitTaskSource.TrySetCanceled(cancellationToken);
            }
        }))
        {
            await waitTaskSource.Task.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Releases one slot and admits waiting requests as far as the limit allows.
    /// </summary>
    public void Release()
    {
        lock (_lock)
        {
            if (_activeCount > 0)
            {
                _activeCount--;
            }

            PromoteWaiters();
        }
    }

    /// <summary>
    /// Adjusts the maximum number of concurrent holders and admits waiting requests
    /// when the limit is raised beyond the active count.
    /// </summary>
    /// <param name="newLimit">The new limit. Values below 1 are treated as 1.</param>
    public void SetLimit(int newLimit)
    {
        lock (_lock)
        {
            _maxLimit = Math.Max(1, newLimit);
            PromoteWaiters();
        }
    }

    private void PromoteWaiters()
    {
        while (_activeCount < _maxLimit && _waiters.Count > 0)
        {
            var waiter = _waiters.Dequeue();
            if (waiter.TrySetResult(true))
            {
                // Only count the waiter when the task was still pending.
                // Cancelled waiters return false and are skipped instead.
                _activeCount++;
            }
        }
    }
}
