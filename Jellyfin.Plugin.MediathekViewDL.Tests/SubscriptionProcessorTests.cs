using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class SubscriptionProcessorTests
    {
        private readonly Mock<ILogger<SubscriptionProcessor>> _loggerMock;
        private readonly Mock<IMediathekViewApiClient> _apiClientMock;
        private readonly Mock<IVideoParser> _videoParserMock;
        private readonly Mock<ILocalMediaScanner> _localMediaScannerMock;
        private readonly Mock<IFileNameBuilderService> _fileNameBuilderServiceMock;
        private readonly Mock<IStrmValidationService> _strmValidationServiceMock;
        private readonly Mock<IFFmpegService> _ffmpegServiceMock;
        private readonly Mock<IQualityCacheRepository> _qualityCacheRepositoryMock;
        private readonly SubscriptionProcessor _processor;

        public SubscriptionProcessorTests()
        {
            _loggerMock = new Mock<ILogger<SubscriptionProcessor>>();
            _apiClientMock = new Mock<IMediathekViewApiClient>();
            _videoParserMock = new Mock<IVideoParser>();
            _localMediaScannerMock = new Mock<ILocalMediaScanner>();
            _fileNameBuilderServiceMock = new Mock<IFileNameBuilderService>();
            _strmValidationServiceMock = new Mock<IStrmValidationService>();
            _ffmpegServiceMock = new Mock<IFFmpegService>();
            _qualityCacheRepositoryMock = new Mock<IQualityCacheRepository>();

            // Default setup: Validation always succeeds
            _strmValidationServiceMock
                .Setup(x => x.ValidateUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _processor = new SubscriptionProcessor(
                _loggerMock.Object,
                _apiClientMock.Object,
                _videoParserMock.Object,
                _localMediaScannerMock.Object,
                _fileNameBuilderServiceMock.Object,
                _strmValidationServiceMock.Object,
                _ffmpegServiceMock.Object,
                _qualityCacheRepositoryMock.Object
            );
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldReturnJob_WhenNewItemFound()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub" };
            var item = new ResultItem
            {
                Id = "123",
                Title = "TestTitle",
                UrlVideo = "http://test.com/video.mp4"
            };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };

            _apiClientMock
                .Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            var videoInfo = new VideoInfo { Title = "TestTitle", Language = "deu" };
            _videoParserMock
                .Setup(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(videoInfo);

            _fileNameBuilderServiceMock
                .Setup(x => x.GenerateDownloadPaths(It.IsAny<VideoInfo>(), It.IsAny<Subscription>()))
                .Returns(new DownloadPaths { DirectoryPath = "/tmp", MainFilePath = "/tmp/video.mp4" });

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, false, CancellationToken.None);

            // Assert
            Assert.Single(jobs);
            var job = jobs[0];
            Assert.Equal("123", job.ItemId);
            Assert.Equal("TestTitle", job.Title);
            Assert.Single(job.DownloadItems);
            Assert.Equal("http://test.com/video.mp4", job.DownloadItems.First().SourceUrl);
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldSkip_IfAlreadyProcessed()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub" };
            subscription.ProcessedItemIds.Add("123"); // Mark as processed

            var item = new ResultItem
            {
                Id = "123",
                Title = "TestTitle"
            };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };

            _apiClientMock
                .Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, false, CancellationToken.None);

            // Assert
            Assert.Empty(jobs);
            // Verify video parser was NOT called because we skipped early
            _videoParserMock.Verify(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldSkip_IfFoundLocally_AndEnhancedDetectionEnabled()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub", EnhancedDuplicateDetection = true };
            var item = new ResultItem { Id = "456", Title = "ExistingTitle" };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };

            _apiClientMock.Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            var videoInfo = new VideoInfo { Title = "ExistingTitle", Language = "deu" };
            _videoParserMock.Setup(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(videoInfo);

            _fileNameBuilderServiceMock.Setup(x => x.GetSubscriptionBaseDirectory(It.IsAny<Subscription>()))
                .Returns("/tmp/TestSub");

            // Simulate local cache containing this item
            var localCache = new LocalEpisodeCache();
            // VideoInfo defaults: SeasonNumber=null, EpisodeNumber=null, AbsoluteEpisodeNumber=null
            // But we can force match by setting absolute number
            videoInfo.AbsoluteEpisodeNumber = 100;
            localCache.Add(null, null, 100, "path/to/file.mp4", "deu");

            _localMediaScannerMock.Setup(x => x.ScanDirectory("/tmp/TestSub", "TestSub"))
               .Returns(localCache);

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, false, CancellationToken.None);

            // Assert
            Assert.Empty(jobs);
            // Verify it was added to processed list
            Assert.Contains("456", subscription.ProcessedItemIds);
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldSkip_AudioDescription_IfDisabled()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub", AllowAudioDescription = false };
            var item = new ResultItem { Id = "123", Title = "AD Content" };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };
            _apiClientMock.Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            var videoInfo = new VideoInfo { Title = "AD Content", HasAudiodescription = true };
            _videoParserMock.Setup(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(videoInfo);

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, false, CancellationToken.None);

            // Assert
            Assert.Empty(jobs);
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldCreateSubtitleJob_WhenEnabled()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub" };
            var item = new ResultItem
            {
                Id = "123",
                UrlVideo = "http://video.mp4",
                UrlSubtitle = "http://subs.ttml"
            };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };
            _apiClientMock.Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            var videoInfo = new VideoInfo { Title = "Test", Language = "deu" };
            _videoParserMock.Setup(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(videoInfo);

            _fileNameBuilderServiceMock.Setup(x => x.GenerateDownloadPaths(It.IsAny<VideoInfo>(), It.IsAny<Subscription>()))
                .Returns(new DownloadPaths { DirectoryPath = "/tmp", MainFilePath = "/tmp/v.mp4", SubtitleFilePath = "/tmp/s.ttml" });

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, true, CancellationToken.None);

            // Assert
            Assert.Single(jobs);
            var job = jobs[0];
            Assert.Equal(2, job.DownloadItems.Count); // Video + Subtitle
            Assert.Contains(job.DownloadItems, d => d.JobType == DownloadType.DirectDownload && d.SourceUrl == "http://subs.ttml");
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldFallback_ToNextQuality_WhenPrimaryFails()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub", QualityCheckWithUrl = true };
            var item = new ResultItem
            {
                Id = "123",
                UrlVideoHd = "http://hd.mp4",
                UrlVideo = "http://sd.mp4",
                UrlVideoLow = "http://low.mp4"
            };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };

            _apiClientMock.Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            var videoInfo = new VideoInfo { Title = "Test", Language = "deu" };
            _videoParserMock.Setup(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(videoInfo);

            _fileNameBuilderServiceMock.Setup(x => x.GenerateDownloadPaths(It.IsAny<VideoInfo>(), It.IsAny<Subscription>()))
                .Returns(new DownloadPaths { DirectoryPath = "/tmp", MainFilePath = "/tmp/v.mp4" });

            // Fail HD, Succeed SD
            _strmValidationServiceMock
                .Setup(x => x.ValidateUrlAsync("http://hd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Fail

            _strmValidationServiceMock
                .Setup(x => x.ValidateUrlAsync("http://sd.mp4", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true); // Success

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, false, CancellationToken.None);

            // Assert
            Assert.Single(jobs);
            var job = jobs[0];
            Assert.Equal("http://sd.mp4", job.DownloadItems.First().SourceUrl);

            // Verify HD was checked first
            _strmValidationServiceMock.Verify(x => x.ValidateUrlAsync("http://hd.mp4", It.IsAny<CancellationToken>()), Times.Once);
            _strmValidationServiceMock.Verify(x => x.ValidateUrlAsync("http://sd.mp4", It.IsAny<CancellationToken>()), Times.Once);
            _strmValidationServiceMock.Verify(x => x.ValidateUrlAsync("http://low.mp4", It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetJobsForSubscriptionAsync_ShouldSkip_WhenAllQualitiesFail()
        {
            // Arrange
            var subscription = new Subscription { Name = "TestSub", QualityCheckWithUrl = true };
            var item = new ResultItem
            {
                Id = "123",
                UrlVideoHd = "http://hd.mp4",
                UrlVideo = "http://sd.mp4"
            };

            var resultChannels = new ResultChannels
            {
                Results = new Collection<ResultItem> { item },
                QueryInfo = new QueryInfo { TotalResults = 1 }
            };

            _apiClientMock.Setup(x => x.SearchAsync(It.IsAny<ApiQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultChannels);

            var videoInfo = new VideoInfo { Title = "Test", Language = "deu" };
            _videoParserMock.Setup(x => x.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(videoInfo);

            _fileNameBuilderServiceMock.Setup(x => x.GenerateDownloadPaths(It.IsAny<VideoInfo>(), It.IsAny<Subscription>()))
                .Returns(new DownloadPaths { DirectoryPath = "/tmp", MainFilePath = "/tmp/v.mp4" });

            // Fail all
            _strmValidationServiceMock
                .Setup(x => x.ValidateUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var jobs = await _processor.GetJobsForSubscriptionAsync(subscription, false, CancellationToken.None);

            // Assert
            Assert.Empty(jobs); // Should not create a job
        }
    }
}
