using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class DownloadManagerTests
{
    private readonly Mock<ILogger<DownloadManager>> _loggerMock;
    private readonly Mock<INfoService> _nfoServiceMock;
    private readonly Mock<IFileDownloader> _fileDownloaderMock;
    private readonly Mock<IStrmValidationService> _validationServiceMock;
    private readonly DownloadManager _downloadManager;

    public DownloadManagerTests()
    {
        _loggerMock = new Mock<ILogger<DownloadManager>>();
        _nfoServiceMock = new Mock<INfoService>();
        _fileDownloaderMock = new Mock<IFileDownloader>();
        _validationServiceMock = new Mock<IStrmValidationService>();

        var handler = _fileDownloaderMock.As<IDownloadHandler>();
        handler.Setup(h => h.CanHandle(It.IsAny<DownloadType>())).Returns(true);
        handler.Setup(h => h.ExecuteAsync(
                It.IsAny<DownloadItem>(),
                It.IsAny<DownloadJob>(),
                It.IsAny<IProgress<double>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _downloadManager = new DownloadManager(
            _loggerMock.Object,
            _nfoServiceMock.Object,
            new[] { handler.Object },
            _validationServiceMock.Object);
    }

    private static DownloadJob CreateJob(string sourceUrl, string destPath, DownloadType type = DownloadType.SubtitleDownload)
    {
        return new DownloadJob
        {
            ItemId = "test-item",
            ItemInfo = new VideoInfo { Title = "Test Video" },
            Title = "Test Video",
            DownloadItems =
            {
                new DownloadItem
                {
                    SourceUrl = sourceUrl,
                    DestinationPath = destPath,
                    JobType = type
                }
            }
        };
    }

    [Fact]
    public async Task ExecuteJobAsync_ValidationReturnsFalse_SkipsHandlerAndReturnsFalse()
    {
        // Arrange
        var destPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.tmp");
        var job = CreateJob("https://ard.de/deleted.mp4", destPath);

        _validationServiceMock
            .Setup(s => s.ValidateUrlAsync("https://ard.de/deleted.mp4", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _downloadManager.ExecuteJobAsync(job, Mock.Of<IProgress<double>>(), CancellationToken.None);

        // Assert
        Assert.False(result);
        _fileDownloaderMock.As<IDownloadHandler>().Verify(
            h => h.ExecuteAsync(It.IsAny<DownloadItem>(), It.IsAny<DownloadJob>(),
                It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task ExecuteJobAsync_ValidationThrowsException_SkipsHandlerAndReturnsFalse()
    {
        // Arrange
        var destPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.tmp");
        var job = CreateJob("https://ard.de/video.mp4", destPath);

        _validationServiceMock
            .Setup(s => s.ValidateUrlAsync("https://ard.de/video.mp4", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Server returned 404"));

        // Act
        var result = await _downloadManager.ExecuteJobAsync(job, Mock.Of<IProgress<double>>(), CancellationToken.None);

        // Assert
        Assert.False(result);
        _fileDownloaderMock.As<IDownloadHandler>().Verify(
            h => h.ExecuteAsync(It.IsAny<DownloadItem>(), It.IsAny<DownloadJob>(),
                It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task ExecuteJobAsync_ValidationSucceeds_ExecutesHandler()
    {
        // Arrange
        var destPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.tmp");
        var job = CreateJob("https://ard.de/video.mp4", destPath);

        _validationServiceMock
            .Setup(s => s.ValidateUrlAsync("https://ard.de/video.mp4", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _downloadManager.ExecuteJobAsync(job, Mock.Of<IProgress<double>>(), CancellationToken.None);

        // Assert
        Assert.True(result);
        _fileDownloaderMock.As<IDownloadHandler>().Verify(
            h => h.ExecuteAsync(
                It.Is<DownloadItem>(i => i.SourceUrl == "https://ard.de/video.mp4"),
                job,
                It.IsAny<IProgress<double>>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task ExecuteJobAsync_FileAlreadyExists_SkipsDownload()
    {
        // Arrange
        var destPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.tmp");
        File.WriteAllText(destPath, "existing content");
        try
        {
            var job = CreateJob("https://ard.de/video.mp4", destPath);

            _validationServiceMock
                .Setup(s => s.ValidateUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _downloadManager.ExecuteJobAsync(job, Mock.Of<IProgress<double>>(), CancellationToken.None);

            // Assert
            Assert.True(result);
            _fileDownloaderMock.As<IDownloadHandler>().Verify(
                h => h.ExecuteAsync(It.IsAny<DownloadItem>(), It.IsAny<DownloadJob>(),
                    It.IsAny<IProgress<double>>(), It.IsAny<CancellationToken>()),
                Times.Never());
        }
        finally
        {
            File.Delete(destPath);
        }
    }
}
