using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services.Adoption;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class FileAdoptionServiceTests
    {
        private readonly Mock<ILogger<FileAdoptionService>> _loggerMock;
        private readonly Mock<IConfigurationProvider> _configProviderMock;
        private readonly Mock<ILocalMediaScanner> _localMediaScannerMock;
        private readonly Mock<IDownloadHistoryRepository> _historyRepositoryMock;
        private readonly Mock<ISubscriptionProcessor> _subscriptionProcessorMock;
        private readonly Mock<IFileNameBuilderService> _fileNameBuilderMock;
        private readonly Mock<ITempMetadataCache> _tempMetadataCacheMock;
        private readonly FileAdoptionService _service;

        public FileAdoptionServiceTests()
        {
            _loggerMock = new Mock<ILogger<FileAdoptionService>>();
            _configProviderMock = new Mock<IConfigurationProvider>();
            _localMediaScannerMock = new Mock<ILocalMediaScanner>();
            _historyRepositoryMock = new Mock<IDownloadHistoryRepository>();
            _subscriptionProcessorMock = new Mock<ISubscriptionProcessor>();
            _fileNameBuilderMock = new Mock<IFileNameBuilderService>();
            _tempMetadataCacheMock = new Mock<ITempMetadataCache>();

            _service = new FileAdoptionService(
                _loggerMock.Object,
                _configProviderMock.Object,
                _localMediaScannerMock.Object,
                _historyRepositoryMock.Object,
                _subscriptionProcessorMock.Object,
                _fileNameBuilderMock.Object,
                _tempMetadataCacheMock.Object);
        }

        [Fact]
        public void CalculateMatchScoreWithSource_ExactUrlMatch_ReturnsHighestScore()
        {
            // Arrange
            var localInfo = new VideoInfo { Title = "Other Title" };
            var apiItem = new ResultItemDto
            {
                Id = "123",
                Title = "API Title",
                Topic = "Topic",
                Channel = "Channel",
                Description = "Description",
                VideoUrls = new List<VideoUrlDto> { new VideoUrlDto { Url = "http://exact-match.com/video.mp4", Quality = 3 } },
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };

            var apiResult = new ApiResultWithInfo(apiItem, new VideoInfo { Title = "API Title" });
            string urlFromInfo = "http://exact-match.com/video.mp4";

            // Act
            var result = InvokeCalculateMatchScoreWithSource(localInfo, apiResult, urlFromInfo);

            // Assert
            Assert.Equal(10.0, result.Score);
            Assert.Equal(AdoptionMatchSource.Url, result.Source);
        }

        [Fact]
        public void CalculateMatchScoreWithSource_SeasonEpisodeMatch_BoostsScore()
        {
            // Arrange
            var localInfo = new VideoInfo
            {
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 5
            };
            var apiInfo = new VideoInfo
            {
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 5
            };
            var apiItem = new ResultItemDto
            {
                Id = "123",
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                Channel = "ARD",
                Description = "Description",
                VideoUrls = new List<VideoUrlDto>(),
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };
            var apiResult = new ApiResultWithInfo(apiItem, apiInfo);

            // Act
            var result = InvokeCalculateMatchScoreWithSource(localInfo, apiResult, null);

            // Assert
            // With title and topic match, score should be 1.0. With 1.4 multiplier, it should be 1.4.
            Assert.Equal(1.4, result.Score, 2);
            Assert.Equal(AdoptionMatchSource.SeriesNumbering, result.Source);
        }

        [Fact]
        public void CalculateMatchScoreWithSource_MismatchedSeasonEpisode_PenalizesScore()
        {
            // Arrange
            var localInfo = new VideoInfo
            {
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 5
            };
            var apiInfo = new VideoInfo
            {
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 6 // Mismatch
            };
            var apiItem = new ResultItemDto
            {
                Id = "123",
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                Channel = "ARD",
                Description = "Description",
                VideoUrls = new List<VideoUrlDto>(),
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };
            var apiResult = new ApiResultWithInfo(apiItem, apiInfo);

            // Act
            var result = InvokeCalculateMatchScoreWithSource(localInfo, apiResult, null);

            // Assert
            // Base score for title and topic match is 1.0. With 0.05 penalty, it should be 1.0 * 0.05 = 0.05.
            Assert.InRange(result.Score, 0.049, 0.051);
        }

        [Fact]
        public void CalculateMatchScoreWithSource_MultipleMismatches_CorrectlyReducesScore()
        {
            // Arrange
            var localInfo = new VideoInfo
            {
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 5,
                AbsoluteEpisodeNumber = 100
            };
            var apiInfo = new VideoInfo
            {
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 6, // Mismatch
                AbsoluteEpisodeNumber = 101 // Mismatch
            };
            var apiItem = new ResultItemDto
            {
                Id = "123",
                Title = "Sendung mit der Maus",
                Topic = "Kindersendung",
                Channel = "ARD",
                Description = "Description",
                VideoUrls = new List<VideoUrlDto>(),
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };
            var apiResult = new ApiResultWithInfo(apiItem, apiInfo);

            // Act
            var result = InvokeCalculateMatchScoreWithSource(localInfo, apiResult, null);

            // Assert
            // Base score 1.0. Multiplied by 0.05 AND 0.05. Should be 1.0 * 0.05 * 0.05 = 0.0025.
            Assert.InRange(result.Score, 0.0024, 0.0026);
        }

        [Fact]
        public void CalculateMatchScoreWithSource_FuzzyTitleMatch_ReturnsCorrectSource()
        {
            // Arrange
            var localInfo = new VideoInfo { Title = "Tatort - Mord im Norden", Topic = "Krimi" };
            var apiInfo = new VideoInfo { Title = "Tatort: Mord im Norden", Topic = "Krimi" }; // Slight difference
            var apiItem = new ResultItemDto
            {
                Id = "123",
                Title = "Tatort: Mord im Norden",
                Topic = "Krimi",
                Channel = "ARD",
                Description = "Description",
                VideoUrls = new List<VideoUrlDto>(),
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };
            var apiResult = new ApiResultWithInfo(apiItem, apiInfo);

            // Act
            var result = InvokeCalculateMatchScoreWithSource(localInfo, apiResult, null);

            // Assert
            Assert.True(result.Score > 0.8);
            Assert.Equal(AdoptionMatchSource.Fuzzy, result.Source);
        }

        [Fact]
        public async Task GetAdoptionCandidatesAsync_MultipleFilesAndApiResults_ReturnsCorrectCandidates()
        {
            // Arrange
            var subId = Guid.NewGuid();
            var subscription = new Subscription { Id = subId, Name = "TestSub" };
            var config = new PluginConfiguration();
            config.Subscriptions.Add(subscription);
            _configProviderMock.Setup(x => x.Configuration).Returns(config);

            var apiItem1 = new ResultItemDto
            {
                Id = "api1",
                Title = "Sendung A",
                Topic = "Topic",
                Channel = "ARD",
                Description = "Desc",
                VideoUrls = new List<VideoUrlDto>(),
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };
            var apiItem2 = new ResultItemDto
            {
                Id = "api2",
                Title = "Sendung B",
                Topic = "Topic",
                Channel = "ZDF",
                Description = "Desc",
                VideoUrls = new List<VideoUrlDto>(),
                SubtitleUrls = new List<SubtitleUrlDto>(),
                ExternalIds = new List<ExternalId>()
            };

            var apiResults = new List<(ResultItemDto, VideoInfo)> { (apiItem1, new VideoInfo { Title = "Sendung A", Topic = "Topic" }), (apiItem2, new VideoInfo { Title = "Sendung B", Topic = "Topic" }) };

            _subscriptionProcessorMock.Setup(x => x.GetEligibleItemsAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
                .Returns(apiResults.ToAsyncEnumerable());

            _fileNameBuilderMock.Setup(x => x.GetSubscriptionBaseDirectories(It.IsAny<Subscription>(), It.IsAny<DownloadContext>()))
                .Returns(new List<string> { "/downloads/TestSub" });

            var scannedFiles = new List<ScannedFile> { new ScannedFile { FilePath = "/downloads/TestSub/Sendung A.mkv", Type = FileType.Video, VideoInfo = new VideoInfo { Title = "Sendung A", Topic = "Topic" } }, new ScannedFile { FilePath = "/downloads/TestSub/Sendung B.mkv", Type = FileType.Video, VideoInfo = new VideoInfo { Title = "Sendung B", Topic = "Topic" } } };

            _localMediaScannerMock.Setup(x => x.ScanSubscriptionDirectory(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new LocalScanResult { Files = new Collection<ScannedFile>(scannedFiles) });

            _historyRepositoryMock.Setup(x => x.GetBySubscriptionIdAsync(subId))
                .ReturnsAsync(new List<DownloadHistoryEntry>());

            // Act
            var result = await _service.GetAdoptionCandidatesAsync(subId);

            // Assert
            Assert.Equal(2, result.Candidates.Count);
            Assert.Equal(2, result.ApiResults.Count);

            var candidateA = result.Candidates.FirstOrDefault(c => c.Id == "/downloads/TestSub/Sendung A.mkv");
            Assert.NotNull(candidateA);
            Assert.Contains(candidateA.Matches, m => m.ApiId == "api1" && m.Confidence > 90);

            var candidateB = result.Candidates.FirstOrDefault(c => c.Id == "/downloads/TestSub/Sendung B.mkv");
            Assert.NotNull(candidateB);
            Assert.Contains(candidateB.Matches, m => m.ApiId == "api2" && m.Confidence > 90);
        }

        private (double Score, AdoptionMatchSource Source) InvokeCalculateMatchScoreWithSource(VideoInfo localInfo, ApiResultWithInfo apiResult, string? urlFromInfo)
        {
            var method = typeof(FileAdoptionService).GetMethod("CalculateMatchScoreWithSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.Invoke(_service, new object[] { localInfo, apiResult, urlFromInfo });

            // Handle the ValueTuple return type from Reflection
            var type = result.GetType();
            var score = (double)type.GetField("Item1").GetValue(result);
            var source = (AdoptionMatchSource)type.GetField("Item2").GetValue(result);

            return (score, source);
        }
    }

    public static class AsyncTestExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return await Task.FromResult(item).ConfigureAwait(false);
            }
        }
    }
}
