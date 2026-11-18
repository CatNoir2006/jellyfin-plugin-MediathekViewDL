using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public sealed class StrmCleanupTaskTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger<StrmCleanupTask>> _loggerMock;
    private readonly Mock<IUrlValidationService> _validationMock;
    private readonly Mock<IConfigurationProvider> _configProviderMock;
    private readonly PluginConfiguration _config;
    private readonly StrmCleanupTask _task;

    public StrmCleanupTaskTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "MediathekViewDL_StrmTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _loggerMock = new Mock<ILogger<StrmCleanupTask>>();
        _validationMock = new Mock<IUrlValidationService>();
        _configProviderMock = new Mock<IConfigurationProvider>();
        _config = new PluginConfiguration();
        _config.Maintenance.EnableStrmCleanup = true;

        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(_config);
        _task = new StrmCleanupTask(_loggerMock.Object, _validationMock.Object, _configProviderMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public void CollectDistinctRootPaths_DeduplicatesAndCollapsesNestedPaths()
    {
        var tvDir = Path.Combine(_tempDir, "tv");
        var tvNestedDir = Path.Combine(tvDir, "The Rookie");
        var moviesDir = Path.Combine(_tempDir, "movies");

        _config.Paths.DefaultSubscriptionShowPath = tvDir;
        _config.Paths.DefaultManualShowPath = tvDir + Path.DirectorySeparatorChar;
        _config.Paths.DefaultSubscriptionMoviePath = moviesDir;
        _config.Subscriptions.Add(new Subscription { IsEnabled = false, Download = new DownloadSettings { DownloadPath = tvNestedDir } });

        var paths = StrmCleanupTask.CollectDistinctRootPaths(_config);

        Assert.Equal(2, paths.Count);
        Assert.Contains(Path.GetFullPath(tvDir), paths);
        Assert.Contains(Path.GetFullPath(moviesDir), paths);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoSubscriptions_StillCleansSplitPathsAndOrphanSidecars()
    {
        var showDir = Path.Combine(_tempDir, "shows", "The Rookie");
        Directory.CreateDirectory(showDir);
        _config.Paths.DefaultSubscriptionShowPath = Path.Combine(_tempDir, "shows");

        var invalidStrm = Path.Combine(showDir, "S03E02.strm");
        var invalidNfo = Path.Combine(showDir, "S03E02.nfo");
        var invalidThumb = Path.Combine(showDir, "S03E02-thumb.jpg");
        await File.WriteAllTextAsync(invalidStrm, "https://zdf.de/expired.mp4");
        await File.WriteAllTextAsync(invalidNfo, "<episodedetails/>");
        await File.WriteAllTextAsync(invalidThumb, "fakejpg");

        var validStrm = Path.Combine(showDir, "S03E03.strm");
        await File.WriteAllTextAsync(validStrm, "https://zdf.de/valid.mp4");

        var trailerStrm = Path.Combine(showDir, "trailer.strm");
        await File.WriteAllTextAsync(trailerStrm, "https://youtube.com/watch?v=123");

        _validationMock.Setup(v => v.ValidateUrlAsync("https://zdf.de/expired.mp4", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _validationMock.Setup(v => v.ValidateUrlAsync("https://zdf.de/valid.mp4", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _validationMock.Setup(v => v.ValidateUrlAsync("https://youtube.com/watch?v=123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Domain not allowed"));

        await _task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

        Assert.False(File.Exists(invalidStrm));
        Assert.False(File.Exists(invalidNfo));
        Assert.False(File.Exists(invalidThumb));
        Assert.True(File.Exists(validStrm));
        Assert.True(File.Exists(trailerStrm));
    }

    [Fact]
    public async Task ExecuteAsync_TransientHttpError_DoesNotDeleteStrm()
    {
        var movieDir = Path.Combine(_tempDir, "movies");
        Directory.CreateDirectory(movieDir);
        _config.Paths.DefaultManualMoviePath = movieDir;

        var strm = Path.Combine(movieDir, "Movie.strm");
        await File.WriteAllTextAsync(strm, "https://ard.de/temp-error.mp4");

        _validationMock.Setup(v => v.ValidateUrlAsync("https://ard.de/temp-error.mp4", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("503 Service Unavailable"));

        await _task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

        Assert.True(File.Exists(strm));
    }
}
