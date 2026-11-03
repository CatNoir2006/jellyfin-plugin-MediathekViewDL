using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediathekView.Api.Models.ResourceItem;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class VideoQualitySelectorTests
    {
        private readonly Mock<IFFmpegService> _ffmpegServiceMock;
        private readonly VideoQualitySelector _selector;

        public VideoQualitySelectorTests()
        {
            _ffmpegServiceMock = new Mock<IFFmpegService>();
            _selector = new VideoQualitySelector(
                new Mock<ILogger<VideoQualitySelector>>().Object,
                _ffmpegServiceMock.Object);
        }

        private static List<VideoUrlDto> CreateStreams() => new()
        {
            new VideoUrlDto { Url = "http://hd.mp4", Quality = 3 },
            new VideoUrlDto { Url = "http://sd.mp4", Quality = 2 },
            new VideoUrlDto { Url = "http://low.mp4", Quality = 1 }
        };

        private static LocalMediaInfo CreateMediaInfo(int? height)
        {
            return new LocalMediaInfo { FilePath = "probe", Height = height };
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldOrderByProbedHeightDescending_WithoutHeightRange()
        {
            // Arrange
            var settings = new BaseDownloadSettings { SelectionOrder = QualitySelectionOrder.HighestToLowest };
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://hd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(1080));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://sd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(720));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://low.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(480));

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "http://hd.mp4", "http://sd.mp4", "http://low.mp4" }, result.Select(v => v.Url));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldOrderByProbedHeightAscending_WhenLowestToHighest()
        {
            // Arrange
            var settings = new BaseDownloadSettings { SelectionOrder = QualitySelectionOrder.LowestToHighest };
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://hd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(1080));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://sd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(720));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://low.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(480));

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "http://low.mp4", "http://sd.mp4", "http://hd.mp4" }, result.Select(v => v.Url));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldOrderByQualityIndex_AndProbe_WhenProbesFail_AndNotStrict()
        {
            // Arrange
            // All probes fail (e.g. invalid URL or unsupported stream): the quality index estimate
            // (3=1080p, 2=720p, 1=480p) preserves the expected ranking.
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LocalMediaInfo?)null);
            var settings = new BaseDownloadSettings { SelectionOrder = QualitySelectionOrder.HighestToLowest };

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "http://hd.mp4", "http://sd.mp4", "http://low.mp4" }, result.Select(v => v.Url));
            _ffmpegServiceMock.Verify(x => x.GetMediaInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_WithMaxHeight_ShouldFilterByProbedHeight()
        {
            // Arrange
            var settings = new BaseDownloadSettings { MaxHeight = 720 };
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://hd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(1080));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://sd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(720));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://low.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(480));

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "http://sd.mp4", "http://low.mp4" }, result.Select(v => v.Url));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_WithMinHeight_ShouldFilterByProbedHeight()
        {
            // Arrange
            var settings = new BaseDownloadSettings { MinHeight = 720 };
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://hd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(1080));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://sd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(720));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://low.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(480));

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "http://hd.mp4", "http://sd.mp4" }, result.Select(v => v.Url));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldUseFallbackEstimate_WhenProbeFails_AndNotStrict()
        {
            // Arrange
            // No probe setup: GetMediaInfoAsync default returns null -> estimate 3=1080, 2=720, 1=480.
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((LocalMediaInfo?)null);
            var settings = new BaseDownloadSettings { MaxHeight = 720 };

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            // The HD stream is estimated at 1080p and therefore filtered out.
            Assert.Equal(new[] { "http://sd.mp4", "http://low.mp4" }, result.Select(v => v.Url));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldExcludeStream_WhenProbeFails_AndStrictResolution()
        {
            // Arrange
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://hd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync((LocalMediaInfo?)null);
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://sd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(720));
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync("http://low.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(480));
            var settings = new BaseDownloadSettings { MaxHeight = 1080, StrictResolution = true };

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            // The unmeasurable HD stream must not fall back to the quality index estimate (which would be 1080 and match the range).
            Assert.Equal(new[] { "http://sd.mp4", "http://low.mp4" }, result.Select(v => v.Url));
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldReturnEmpty_WhenNoStreamMatchesRange()
        {
            // Arrange
            _ffmpegServiceMock
                .Setup(x => x.GetMediaInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMediaInfo(1080));
            var settings = new BaseDownloadSettings { MaxHeight = 240 };

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(CreateStreams(), settings, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldDeduplicateByUrl()
        {
            // Arrange
            var streams = new List<VideoUrlDto>
            {
                new VideoUrlDto { Url = "http://hd.mp4", Quality = 3 },
                new VideoUrlDto { Url = "http://hd.mp4", Quality = 3 }
            };
            var settings = new BaseDownloadSettings();

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(streams, settings, CancellationToken.None);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetOrderedCandidatesAsync_ShouldIgnoreStreamsWithEmptyUrl()
        {
            // Arrange
            var streams = new List<VideoUrlDto>
            {
                new VideoUrlDto { Url = string.Empty, Quality = 3 },
                new VideoUrlDto { Url = "http://sd.mp4", Quality = 2 }
            };
            var settings = new BaseDownloadSettings();

            // Act
            var result = await _selector.GetOrderedCandidatesAsync(streams, settings, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("http://sd.mp4", result[0].Url);
        }
    }
}
