using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class MediaMergeServiceTests : IDisposable
{
    private readonly string _testDir;

    public MediaMergeServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "MediathekMergeTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public async Task MergeMediaInstances_WhenDisabled_ReturnsZero()
    {
        var config = new PluginConfiguration();
        config.Merging.EnableInstanceMerging = false;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var service = new MediaMergeService(
            NullLogger<MediaMergeService>.Instance,
            configProviderMock.Object);

        var count = await service.MergeMediaInstancesAsync(CancellationToken.None);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task ProcessDirectoryMerging_AutoDeleteStrm_DeletesDuplicateStrmWhenPhysicalVideoExists()
    {
        var seasonDir = Path.Combine(_testDir, "Season 01");
        Directory.CreateDirectory(seasonDir);

        // Physical video (e.g. download or Seerr)
        var mkvFile = Path.Combine(seasonDir, "The Rookie - S01E01 - Pilot.mkv");
        await File.WriteAllTextAsync(mkvFile, "video data");

        // Stale .strm stream link
        var strmFile = Path.Combine(seasonDir, "The Rookie - S01E01 - Pilot.strm");
        await File.WriteAllTextAsync(strmFile, "https://stream.url");
        var nfoFile = Path.Combine(seasonDir, "The Rookie - S01E01 - Pilot.nfo");
        await File.WriteAllTextAsync(nfoFile, "<episodedetails></episodedetails>");

        var config = new PluginConfiguration();
        config.Merging.EnableInstanceMerging = true;
        config.Merging.AutoDeleteStrmWhenLocalExists = true;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var service = new MediaMergeService(
            NullLogger<MediaMergeService>.Instance,
            configProviderMock.Object);

        var count = await service.ProcessDirectoryMergingAsync(_testDir, CancellationToken.None);
        Assert.Equal(1, count);

        // .strm and .nfo must be deleted
        Assert.False(File.Exists(strmFile));
        Assert.False(File.Exists(nfoFile));
        // .mkv must still exist
        Assert.True(File.Exists(mkvFile));
    }

    [Fact]
    public async Task ProcessDirectoryMerging_MultiVersionRename_RenamesDuplicateStrm()
    {
        var seasonDir = Path.Combine(_testDir, "Season 01");
        Directory.CreateDirectory(seasonDir);

        // Physical video (e.g. download or Seerr)
        var mkvFile = Path.Combine(seasonDir, "The Rookie - S01E02 - Crash Course.mkv");
        await File.WriteAllTextAsync(mkvFile, "video data");

        // Strm stream link
        var strmFile = Path.Combine(seasonDir, "The Rookie - S01E02 - Crash Course.strm");
        await File.WriteAllTextAsync(strmFile, "https://stream.url");

        var config = new PluginConfiguration();
        config.Merging.EnableInstanceMerging = true;
        config.Merging.AutoDeleteStrmWhenLocalExists = false;
        config.Merging.GroupAsMultiVersion = true;

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var service = new MediaMergeService(
            NullLogger<MediaMergeService>.Instance,
            configProviderMock.Object);

        var count = await service.ProcessDirectoryMergingAsync(_testDir, CancellationToken.None);
        Assert.Equal(1, count);

        var expectedRenamedPath = Path.Combine(seasonDir, "The Rookie - S01E02 - Crash Course - [Mediathek Stream].strm");
        Assert.True(File.Exists(expectedRenamedPath));
    }
}
