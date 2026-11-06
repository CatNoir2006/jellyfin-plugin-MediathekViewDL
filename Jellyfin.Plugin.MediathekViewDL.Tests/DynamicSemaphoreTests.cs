using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class DynamicSemaphoreTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task WaitAsync_ShouldAdmitImmediately_UpToCurrentLimit()
    {
        // Arrange
        var semaphore = new DynamicSemaphore(2);

        // Act / Assert
        await semaphore.WaitAsync().WaitAsync(TestTimeout);
        await semaphore.WaitAsync().WaitAsync(TestTimeout);

        // The third request stays a waiter and waits for a release.
        var third = semaphore.WaitAsync();
        await Task.Delay(100);
        Assert.False(third.IsCompleted);
        Assert.Equal(2, semaphore.ActiveCount);

        // Cleanup — complete the test without deadlocks.
        semaphore.Release();
        await third.WaitAsync(TestTimeout);
    }

    [Fact]
    public async Task SetLimit_ShouldAdmitWaitersImmediately_WhenLimitIsRaised()
    {
        // Arrange
        var semaphore = new DynamicSemaphore(1);
        var first = semaphore.WaitAsync();
        await first.WaitAsync(TestTimeout);

        var second = semaphore.WaitAsync();
        var third = semaphore.WaitAsync();
        await Task.Delay(100);
        Assert.False(second.IsCompleted);
        Assert.False(third.IsCompleted);

        // Act — raise the limit like a config change would.
        semaphore.SetLimit(3);

        // Assert — waiters are admitted without any release.
        await Task.WhenAll(second, third).WaitAsync(TestTimeout);
        Assert.Equal(3, semaphore.ActiveCount);
    }

    [Fact]
    public async Task Release_ShouldAdmitWaiter_WhenSlotBecomesFree()
    {
        // Arrange
        var semaphore = new DynamicSemaphore(1);
        await semaphore.WaitAsync().WaitAsync(TestTimeout);

        var waiter = semaphore.WaitAsync();
        await Task.Delay(100);
        Assert.False(waiter.IsCompleted);

        // Act
        semaphore.Release();

        // Assert
        await waiter.WaitAsync(TestTimeout);
        Assert.Equal(1, semaphore.ActiveCount);
    }

    [Fact]
    public async Task SetLimit_ShouldNotAdmitNewWaiters_WhenLimitIsLoweredBelowActiveCount()
    {
        // Arrange
        var semaphore = new DynamicSemaphore(3);
        await semaphore.WaitAsync().WaitAsync(TestTimeout);
        await semaphore.WaitAsync().WaitAsync(TestTimeout);
        await semaphore.WaitAsync().WaitAsync(TestTimeout);
        Assert.Equal(3, semaphore.ActiveCount);

        // Act — lower the limit: running holders keep their slot, new requests must wait.
        semaphore.SetLimit(1);

        var waiter = semaphore.WaitAsync();
        await Task.Delay(100);

        // Assert
        Assert.False(waiter.IsCompleted);
        Assert.Equal(3, semaphore.ActiveCount);

        // Free slots drop the active count; only below the new limit waiters may be admitted.
        semaphore.Release();
        semaphore.Release();
        semaphore.Release();
        await waiter.WaitAsync(TestTimeout);
        Assert.Equal(1, semaphore.ActiveCount);
    }

    [Fact]
    public async Task WaitAsync_ShouldThrowOperationCanceled_WhenTokenIsCancelledWhileWaiting()
    {
        // Arrange
        var semaphore = new DynamicSemaphore(1);
        await semaphore.WaitAsync().WaitAsync(TestTimeout);

        using var cts = new CancellationTokenSource();
        var waiter = semaphore.WaitAsync(cts.Token);
        await Task.Delay(100);
        Assert.False(waiter.IsCompleted);

        // Act
        await cts.CancelAsync();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter.WaitAsync(TestTimeout));
    }
}
