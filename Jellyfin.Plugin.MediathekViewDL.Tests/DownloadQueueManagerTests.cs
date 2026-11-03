using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Queue;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class DownloadQueueManagerTests : IDisposable
{
    private readonly Mock<IConfigurationProvider> _configurationProviderMock = new();
    private readonly Mock<IDownloadManager> _downloadManagerMock = new();
    private readonly Mock<IDownloadHistoryRepository> _historyRepositoryMock = new();
    private readonly Mock<ILibraryManager> _libraryManagerMock = new();
    private readonly PluginConfiguration _configuration = new();
    private DownloadQueueManager? _manager;

    public void Dispose()
    {
        _manager?.Dispose();
    }

    private DownloadQueueManager CreateManager(int maxParallelDownloads, bool scanLibraryAfterDownload = true)
    {
        _configuration.Download = new DownloadOptions
        {
            MaxParallelDownloads = maxParallelDownloads,
            ScanLibraryAfterDownload = scanLibraryAfterDownload
        };

        _configurationProviderMock.SetupGet(p => p.ConfigurationOrNull).Returns(_configuration);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(s => s.GetService(typeof(IDownloadManager))).Returns(_downloadManagerMock.Object);
        serviceProviderMock.Setup(s => s.GetService(typeof(IDownloadHistoryRepository))).Returns(_historyRepositoryMock.Object);
        serviceProviderMock.Setup(s => s.GetService(typeof(ILibraryManager))).Returns(_libraryManagerMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

        _manager = new DownloadQueueManager(
            serviceScopeFactoryMock.Object,
            new Mock<ILogger<DownloadQueueManager>>().Object,
            _configurationProviderMock.Object);

        return _manager;
    }

    /// <summary>
    /// Sets the download manager mock up so that jobs titled "A" and "B" run until they are
    /// released via <see cref="BlockingJobsHelper.FinishA"/> / <see cref="BlockingJobsHelper.FinishB"/>.
    /// Tracks current and maximum observed concurrency.
    /// </summary>
    private BlockingJobsHelper SetupBlockingJobs()
    {
        var jobs = new BlockingJobsHelper();

        _downloadManagerMock.Setup(m => m.ExecuteJobAsync(
                It.IsAny<DownloadJob>(),
                It.IsAny<IProgress<double>>(),
                It.IsAny<CancellationToken>()))
            .Returns((DownloadJob job, IProgress<double> progress, CancellationToken ct) =>
            {
                var isJobA = job.Title == "A";
                var started = isJobA ? jobs.StartedA : jobs.StartedB;
                var finish = isJobA ? jobs.FinishA : jobs.FinishB;

                var newCount = Interlocked.Increment(ref jobs.CurrentCount);
                var observedMax = Volatile.Read(ref jobs.MaxCount);
                while (newCount > observedMax && Interlocked.CompareExchange(ref jobs.MaxCount, newCount, observedMax) != observedMax)
                {
                    observedMax = Volatile.Read(ref jobs.MaxCount);
                }

                started.SetResult();

                return finish.Task.ContinueWith(
                    _ =>
                    {
                        Interlocked.Decrement(ref jobs.CurrentCount);
                        return new DownloadJobResult { Success = true };
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            });

        return jobs;
    }

    private sealed class BlockingJobsHelper
    {
        public TaskCompletionSource StartedA { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource StartedB { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource FinishA { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource FinishB { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int CurrentCount;
        public int MaxCount;
    }

    private static DownloadJob CreateJob(string title)
    {
        return new DownloadJob
        {
            ItemId = "test-item",
            ItemInfo = new VideoInfo { Title = title },
            Title = title
        };
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, TimeSpan timeout, string description)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < timeout)
        {
            await Task.Delay(10);
        }

        if (!condition())
        {
            throw new TimeoutException($"Condition was not met within {timeout.TotalSeconds}s: {description}.");
        }
    }

    [Fact]
    public async Task QueueJob_ShouldRunDownloadsInParallel_WhenMaxParallelDownloadsIsGreaterThanOne()
    {
        // Arrange
        var manager = CreateManager(maxParallelDownloads: 2);
        var jobs = SetupBlockingJobs();

        manager.QueueJob(CreateJob("A"));
        manager.QueueJob(CreateJob("B"));

        var activeDownloads = manager.GetActiveDownloads().ToDictionary(d => d.Job.Title);

        // Act — wait until both jobs are running at the same time.
        await Task.WhenAll(jobs.StartedA.Task, jobs.StartedB.Task).WaitAsync(TimeSpan.FromSeconds(10));

        // Assert — due to limit 2 both jobs run concurrently and neither has finished yet.
        Assert.Equal(2, Volatile.Read(ref jobs.CurrentCount));
        Assert.Equal(DownloadStatus.Downloading, activeDownloads["A"].Status);
        Assert.Equal(DownloadStatus.Downloading, activeDownloads["B"].Status);

        // Act — release both jobs and wait until they finished.
        jobs.FinishA.SetResult();
        jobs.FinishB.SetResult();

        await WaitForConditionAsync(
            () => activeDownloads.Values.All(d => d.Status == DownloadStatus.Finished),
            TimeSpan.FromSeconds(10),
            "both downloads finished");

        // Assert
        Assert.Equal(2, Volatile.Read(ref jobs.MaxCount));
    }

    [Fact]
    public async Task QueueJob_ShouldRunDownloadsSequentially_WhenConfiguredValueIsClampedToOne()
    {
        // Arrange — value 0 is outside the supported range and must be clamped to 1.
        var manager = CreateManager(maxParallelDownloads: 0);
        var jobs = SetupBlockingJobs();

        manager.QueueJob(CreateJob("A"));
        manager.QueueJob(CreateJob("B"));

        var activeDownloads = manager.GetActiveDownloads().ToDictionary(d => d.Job.Title);

        // Act — wait until the first job is running.
        await jobs.StartedA.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await WaitForConditionAsync(
            () => activeDownloads["A"].Status == DownloadStatus.Downloading,
            TimeSpan.FromSeconds(10),
            "download A started");

        // Assert — download B stays queued while download A is running.
        Assert.Equal(1, Volatile.Read(ref jobs.CurrentCount));
        await Task.Delay(200);
        Assert.False(jobs.StartedB.Task.IsCompleted);
        Assert.Equal(1, Volatile.Read(ref jobs.CurrentCount));

        // Act — finish download A so download B can start.
        jobs.FinishA.SetResult();
        await jobs.StartedB.Task.WaitAsync(TimeSpan.FromSeconds(10));
        jobs.FinishB.SetResult();

        await WaitForConditionAsync(
            () => activeDownloads.Values.All(d => d.Status == DownloadStatus.Finished),
            TimeSpan.FromSeconds(10),
            "both downloads finished");

        // Assert — no more than one download ever ran at the same time.
        Assert.Equal(1, Volatile.Read(ref jobs.MaxCount));
    }

    [Fact]
    public async Task ExecuteDownloadAsync_ScanLibraryAfterDownload_ShouldTriggerScanAfterLastRunningDownload()
    {
        // Arrange
        var manager = CreateManager(maxParallelDownloads: 2);
        var jobs = SetupBlockingJobs();

        manager.QueueJob(CreateJob("A"));
        manager.QueueJob(CreateJob("B"));

        var activeDownloads = manager.GetActiveDownloads().ToDictionary(d => d.Job.Title);

        await Task.WhenAll(jobs.StartedA.Task, jobs.StartedB.Task).WaitAsync(TimeSpan.FromSeconds(10));

        // Act — finish only download A while download B is still running.
        jobs.FinishA.SetResult();
        await WaitForConditionAsync(
            () => activeDownloads["A"].Status == DownloadStatus.Finished,
            TimeSpan.FromSeconds(10),
            "download A finished");

        // Assert — the scan must not be triggered while another download is still running.
        _libraryManagerMock.Verify(l => l.QueueLibraryScan(), Times.Never);

        // Act — finish download B so the queue is drained.
        jobs.FinishB.SetResult();
        await WaitForConditionAsync(
            () => activeDownloads["B"].Status == DownloadStatus.Finished,
            TimeSpan.FromSeconds(10),
            "download B finished");

        // Assert — the scan is triggered exactly once, after all running downloads finished.
        _libraryManagerMock.Verify(l => l.QueueLibraryScan(), Times.Once);
    }

    [Fact]
    public async Task QueueJob_ShouldApplyRaisedLimitDynamically_WithoutRestart()
    {
        // Arrange — the manager is created while the configuration limit is still 1.
        var manager = CreateManager(maxParallelDownloads: 1);
        var jobs = SetupBlockingJobs();

        manager.QueueJob(CreateJob("A"));
        var activeDownloads = manager.GetActiveDownloads().ToDictionary(d => d.Job.Title);

        await jobs.StartedA.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(DownloadStatus.Downloading, activeDownloads["A"].Status);

        // Act — queue a second job while download A is still running.
        manager.QueueJob(CreateJob("B"));

        // The queued job waits for a free slot.

        // Act — simulate saving a raised configuration (like the settings UI would):
        // change the configuration and notify the subscribers.
        _configuration.Download = new DownloadOptions { MaxParallelDownloads = 2 };
        Plugin.RaiseConfigurationSaved();

        // Assert — download B starts while A is still running, without a restart.
        await jobs.StartedB.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(2, Volatile.Read(ref jobs.CurrentCount));
    }
}
