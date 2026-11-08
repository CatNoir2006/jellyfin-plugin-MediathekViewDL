using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using MediathekView.Api.External;
using MediathekView.Api.Models;
using MediathekView.Api.Models.ResourceItem;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class VirtualLibrarySyncServiceTests : IDisposable
{
    private readonly string _testDir;

    public VirtualLibrarySyncServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "MediathekVirtLibTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Ignore cleanup failures
        }
    }

    private static ResultItemDto CreateMockResultItem(string id, string topic, string title, string videoUrl, TimeSpan duration)
    {
        return new ResultItemDto
        {
            Id = id,
            Topic = topic,
            Title = title,
            Channel = "ZDF",
            Description = "John Nolan starts his career in LAPD.",
            Timestamp = DateTimeOffset.UtcNow,
            Duration = duration,
            VideoUrls = new List<VideoUrlDto>
            {
                new VideoUrlDto { Url = videoUrl, Quality = 3 }
            },
            SubtitleUrls = new List<SubtitleUrlDto>(),
            ExternalIds = new List<ExternalId>()
        };
    }

    private static QueryResultDto CreateMockQueryResult(List<ResultItemDto> results)
    {
        return new QueryResultDto
        {
            QueryInfo = new QueryInfoDto
            {
                MovieListTimestamp = 0,
                ResultCount = results.Count,
                TotalResults = results.Count,
                TotalEntries = 1000
            },
            Results = results
        };
    }

    [Fact]
    public async Task SyncVirtualLibrary_DisabledInConfig_ReturnsZero()
    {
        var config = new PluginConfiguration();
        config.VirtualLibrary.EnableVirtualLibrary = false;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var service = new VirtualLibrarySyncService(
            NullLogger<VirtualLibrarySyncService>.Instance,
            configProviderMock.Object,
            new Mock<IMediathekViewApiClient>().Object,
            new Mock<IVideoParser>().Object);

        var count = await service.SyncVirtualLibraryAsync(CancellationToken.None);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task SyncTopicAsync_CreatesStrmAndNfoSidecars()
    {
        var config = new PluginConfiguration();
        config.VirtualLibrary.EnableVirtualLibrary = true;
        config.VirtualLibrary.VirtualLibraryPath = _testDir;
        config.VirtualLibrary.MinDurationMinutes = 5;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var apiClientMock = new Mock<IMediathekViewApiClient>();
        var mockResults = new List<ResultItemDto>
        {
            CreateMockResultItem("rookie_ep1", "The Rookie", "S01/E01: Frischfleisch", "https://example.com/stream.mp4", TimeSpan.FromMinutes(40))
        };
        apiClientMock.Setup(a => a.SearchAsync(It.IsAny<ApiQueryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockQueryResult(mockResults));

        var videoParserMock = new Mock<IVideoParser>();
        videoParserMock.Setup(v => v.ParseVideoInfo("The Rookie", "S01/E01: Frischfleisch"))
            .Returns(new VideoInfo
            {
                Title = "Frischfleisch",
                SeasonNumber = 1,
                EpisodeNumber = 1,
                IsShow = true
            });

        var service = new VirtualLibrarySyncService(
            NullLogger<VirtualLibrarySyncService>.Instance,
            configProviderMock.Object,
            apiClientMock.Object,
            videoParserMock.Object);

        var synced = await service.SyncTopicAsync("The Rookie", _testDir, CancellationToken.None);
        Assert.Equal(1, synced);

        var seriesDir = Path.Combine(_testDir, "series", "The Rookie", "Season 01");
        Assert.True(Directory.Exists(seriesDir));

        var strmFiles = Directory.GetFiles(seriesDir, "*.strm");
        Assert.Single(strmFiles);
        var content = await File.ReadAllTextAsync(strmFiles[0]);
        Assert.Equal("https://example.com/stream.mp4", content);

        var nfoFiles = Directory.GetFiles(seriesDir, "*.nfo");
        Assert.Single(nfoFiles);
    }

    [Fact]
    public async Task SyncTopicAsync_CuratedPopularOnly_FiltersOutShortMovies()
    {
        var config = new PluginConfiguration();
        config.VirtualLibrary.EnableVirtualLibrary = true;
        config.VirtualLibrary.VirtualLibraryPath = _testDir;
        config.VirtualLibrary.CuratedPopularOnly = true;
        config.VirtualLibrary.MinMovieDurationMinutes = 75;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var apiClientMock = new Mock<IMediathekViewApiClient>();
        var mockResults = new List<ResultItemDto>
        {
            CreateMockResultItem("short_doc", "Spielfilm", "Kurzfilm", "https://example.com/short.mp4", TimeSpan.FromMinutes(30)),
            CreateMockResultItem("full_movie", "Spielfilm", "Blockbuster Movie", "https://example.com/movie.mp4", TimeSpan.FromMinutes(95))
        };
        apiClientMock.Setup(a => a.SearchAsync(It.IsAny<ApiQueryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockQueryResult(mockResults));

        var videoParserMock = new Mock<IVideoParser>();
        videoParserMock.Setup(v => v.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new VideoInfo { IsShow = false });

        var service = new VirtualLibrarySyncService(
            NullLogger<VirtualLibrarySyncService>.Instance,
            configProviderMock.Object,
            apiClientMock.Object,
            videoParserMock.Object);

        var synced = await service.SyncTopicAsync("Spielfilm", _testDir, CancellationToken.None);
        Assert.Equal(1, synced);

        var moviesDir = Path.Combine(_testDir, "movies");
        var movieSubDirs = Directory.GetDirectories(moviesDir);
        Assert.Single(movieSubDirs);
        Assert.Contains("Blockbuster Movie", movieSubDirs[0]);
    }

    [Fact]
    public async Task SyncTopicAsync_ShowAlreadyPresentInLocalTvLibrary_SkipsVirtualSeries()
    {
        var localTvDir = Path.Combine(Path.GetTempPath(), "LocalTvTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(localTvDir, "The Rookie"));

        try
        {
            var config = new PluginConfiguration();
            config.VirtualLibrary.EnableVirtualLibrary = true;
            config.VirtualLibrary.VirtualLibraryPath = _testDir;
            config.Paths.DefaultSubscriptionShowPath = localTvDir;

            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

            var apiClientMock = new Mock<IMediathekViewApiClient>();
            var mockResults = new List<ResultItemDto>
            {
                CreateMockResultItem("rookie_s01e01", "The Rookie", "Frischfleisch", "https://example.com/stream.mp4", TimeSpan.FromMinutes(45))
            };
            apiClientMock.Setup(a => a.SearchAsync(It.IsAny<ApiQueryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockQueryResult(mockResults));

            var videoParserMock = new Mock<IVideoParser>();
            videoParserMock.Setup(v => v.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new VideoInfo
                {
                    Title = "Frischfleisch",
                    SeasonNumber = 1,
                    EpisodeNumber = 1,
                    IsShow = true
                });

            var service = new VirtualLibrarySyncService(
                NullLogger<VirtualLibrarySyncService>.Instance,
                configProviderMock.Object,
                apiClientMock.Object,
                videoParserMock.Object);

            var synced = await service.SyncTopicAsync("The Rookie", _testDir, CancellationToken.None);
            Assert.Equal(0, synced);

            var seriesDir = Path.Combine(_testDir, "series", "The Rookie");
            Assert.False(Directory.Exists(seriesDir));
        }
        finally
        {
            if (Directory.Exists(localTvDir))
            {
                Directory.Delete(localTvDir, true);
            }
        }
    }

    [Fact]
    public async Task SyncTopicAsync_FiltersOutAccessibilityDuplicates()
    {
        var config = new PluginConfiguration();
        config.VirtualLibrary.EnableVirtualLibrary = true;
        config.VirtualLibrary.VirtualLibraryPath = _testDir;
        config.VirtualLibrary.CuratedPopularOnly = false;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var apiClientMock = new Mock<IMediathekViewApiClient>();
        var mockResults = new List<ResultItemDto>
        {
            CreateMockResultItem("ad_movie", "Filme", "Tatort - Mord (Audiodeskription)", "https://example.com/ad.mp4", TimeSpan.FromMinutes(90)),
            CreateMockResultItem("clean_movie", "Filme", "Tatort - Mord", "https://example.com/clean.mp4", TimeSpan.FromMinutes(90))
        };
        apiClientMock.Setup(a => a.SearchAsync(It.IsAny<ApiQueryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockQueryResult(mockResults));

        var videoParserMock = new Mock<IVideoParser>();
        videoParserMock.Setup(v => v.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new VideoInfo { IsShow = false });

        var service = new VirtualLibrarySyncService(
            NullLogger<VirtualLibrarySyncService>.Instance,
            configProviderMock.Object,
            apiClientMock.Object,
            videoParserMock.Object);

        var synced = await service.SyncTopicAsync("Filme", _testDir, CancellationToken.None);
        Assert.Equal(1, synced);

        var moviesDir = Path.Combine(_testDir, "movies");
        var movieSubDirs = Directory.GetDirectories(moviesDir);
        Assert.Single(movieSubDirs);
        Assert.DoesNotContain("Audiodeskription", movieSubDirs[0]);
    }

    [Fact]
    public async Task SyncTopicAsync_ParsesCleanMovieTitleAndYear()
    {
        var config = new PluginConfiguration();
        config.VirtualLibrary.EnableVirtualLibrary = true;
        config.VirtualLibrary.VirtualLibraryPath = _testDir;
        config.VirtualLibrary.CuratedPopularOnly = false;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var apiClientMock = new Mock<IMediathekViewApiClient>();
        var mockResults = new List<ResultItemDto>
        {
            CreateMockResultItem("doc_broadcaster", "Filme", "Cash - Spielfilm, Deutschland 2020", "https://example.com/cash.mp4", TimeSpan.FromMinutes(90))
        };
        apiClientMock.Setup(a => a.SearchAsync(It.IsAny<ApiQueryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockQueryResult(mockResults));

        var videoParserMock = new Mock<IVideoParser>();
        videoParserMock.Setup(v => v.ParseVideoInfo(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new VideoInfo { IsShow = false });

        var service = new VirtualLibrarySyncService(
            NullLogger<VirtualLibrarySyncService>.Instance,
            configProviderMock.Object,
            apiClientMock.Object,
            videoParserMock.Object);

        var synced = await service.SyncTopicAsync("Filme", _testDir, CancellationToken.None);
        Assert.Equal(1, synced);

        var moviesDir = Path.Combine(_testDir, "movies");
        var movieSubDirs = Directory.GetDirectories(moviesDir);
        Assert.Single(movieSubDirs);
        Assert.Equal("Cash (2020)", Path.GetFileName(movieSubDirs[0]));
    }
}

