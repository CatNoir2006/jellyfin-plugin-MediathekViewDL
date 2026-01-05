using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class M3U8DownloadHandlerTests
    {
        private readonly Mock<ILogger<M3U8DownloadHandler>> _loggerMock;
        private readonly Mock<IFFmpegService> _ffmpegServiceMock;
        private readonly M3U8DownloadHandler _handler;

        public M3U8DownloadHandlerTests()
        {
            _loggerMock = new Mock<ILogger<M3U8DownloadHandler>>();
            _ffmpegServiceMock = new Mock<IFFmpegService>();
            _handler = new M3U8DownloadHandler(_loggerMock.Object, _ffmpegServiceMock.Object);
        }

        [Fact]
        public void CanHandle_ShouldReturnTrue_ForM3U8Download()
        {
            Assert.True(_handler.CanHandle(DownloadType.M3U8Download));
        }

        [Theory]
        [InlineData(DownloadType.DirectDownload)]
        [InlineData(DownloadType.StreamingUrl)]
        [InlineData(DownloadType.AudioExtraction)]
        [InlineData(DownloadType.QualityUpgrade)]
        public void CanHandle_ShouldReturnFalse_ForOtherTypes(DownloadType type)
        {
            Assert.False(_handler.CanHandle(type));
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCallDownloadM3U8Async()
        {
            // Arrange
            var downloadItem = new DownloadItem
            {
                JobType = DownloadType.M3U8Download,
                SourceUrl = "https://example.com/stream.m3u8",
                DestinationPath = "/tmp/video.mp4"
            };
            var downloadJob = new DownloadJob 
            { 
                Title = "Test Job",
                ItemInfo = new Services.Media.VideoInfo() 
            };
            var progressMock = new Mock<IProgress<double>>();

            _ffmpegServiceMock
                .Setup(x => x.DownloadM3U8Async(downloadItem.SourceUrl, downloadItem.DestinationPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.ExecuteAsync(downloadItem, downloadJob, progressMock.Object, CancellationToken.None);

            // Assert
            Assert.True(result);
            _ffmpegServiceMock.Verify(x => x.DownloadM3U8Async(downloadItem.SourceUrl, downloadItem.DestinationPath, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
