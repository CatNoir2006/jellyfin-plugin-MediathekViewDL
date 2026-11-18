using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using MediathekView.Api.External;
using MediathekView.Api.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class AccessibilityFilterHelperTests
{
    [Theory]
    [InlineData("Tatort: Mord (Audiodeskription)", true)]
    [InlineData("Bibi & Tina - Tohuwabohu total (AD) (2026)", true)]
    [InlineData("Nord bei Nordwest - Sandy (5) - Audiodeskription (2022)", true)]
    [InlineData("Apocalypse Now [AD]", true)]
    [InlineData("Cash - AD", true)]
    [InlineData("Das Geheimnis (Hörfassung)", true)]
    [InlineData("Film (mit Untertitel)", true)]
    [InlineData("Film (mit UT)", true)]
    [InlineData("Adrenalin", false)]
    [InlineData("The Addams Family", false)]
    [InlineData("Bad Boys", false)]
    [InlineData("Apocalypse Now", false)]
    public void IsAudioDescription_DetectsMarkersCorrectly(string title, bool expected)
    {
        Assert.Equal(expected, AccessibilityFilterHelper.IsAudioDescription(title));
    }

    [Theory]
    [InlineData("Ablass (Gebärdensprache) (2026)", true)]
    [InlineData("Sie sind unter uns (Gebärdensprache) (2026)", true)]
    [InlineData("Film (DGS)", true)]
    [InlineData("Film [DGS]", true)]
    [InlineData("Nachrichten mit Gebärdendolmetscher", true)]
    [InlineData("Film (GS)", true)]
    [InlineData("Normaler Film", false)]
    [InlineData("Dating Game Show", false)]
    public void IsSignLanguage_DetectsMarkersCorrectly(string title, bool expected)
    {
        Assert.Equal(expected, AccessibilityFilterHelper.IsSignLanguage(title));
    }

    [Fact]
    public void ShouldFilterOut_RespectsPermissions()
    {
        var viWithAd = new VideoInfo { HasAudiodescription = true };
        var viClean = new VideoInfo { HasAudiodescription = false };

        // AD not allowed
        Assert.True(AccessibilityFilterHelper.ShouldFilterOut("Clean Title", viWithAd, allowAudioDescription: false));
        Assert.True(AccessibilityFilterHelper.ShouldFilterOut("Title (AD)", viClean, allowAudioDescription: false));

        // AD allowed
        Assert.False(AccessibilityFilterHelper.ShouldFilterOut("Clean Title", viWithAd, allowAudioDescription: true));
        Assert.False(AccessibilityFilterHelper.ShouldFilterOut("Title (AD)", viClean, allowAudioDescription: true));
    }

    [Fact]
    public async Task VirtualLibrarySync_CleansUpLegacyAccessibilityDirectories()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "VirtLibCleanupTest_" + Guid.NewGuid().ToString("N"));
        var moviesDir = Path.Combine(testDir, "movies");
        var legacyAdDir = Path.Combine(moviesDir, "ACHT (Audiodeskription) (2026)");
        var legacySignDir = Path.Combine(moviesDir, "Ablass (Gebärdensprache) (2026)");
        var validMovieDir = Path.Combine(moviesDir, "Inception (2010)");

        Directory.CreateDirectory(legacyAdDir);
        Directory.CreateDirectory(legacySignDir);
        Directory.CreateDirectory(validMovieDir);

        try
        {
            var config = new PluginConfiguration();
            config.VirtualLibrary.EnableVirtualLibrary = true;
            config.VirtualLibrary.VirtualLibraryPath = testDir;

            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

            var apiClientMock = new Mock<IMediathekViewApiClient>();
            apiClientMock.Setup(a => a.SearchAsync(It.IsAny<ApiQueryDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueryResultDto
                {
                    QueryInfo = new QueryInfoDto
                    {
                        MovieListTimestamp = 0,
                        ResultCount = 0,
                        TotalResults = 0,
                        TotalEntries = 0
                    },
                    Results = new List<ResultItemDto>()
                });

            var videoParserMock = new Mock<IVideoParser>();

            var service = new VirtualLibrarySyncService(
                NullLogger<VirtualLibrarySyncService>.Instance,
                configProviderMock.Object,
                apiClientMock.Object,
                videoParserMock.Object);

            await service.SyncVirtualLibraryAsync(CancellationToken.None);

            Assert.False(Directory.Exists(legacyAdDir), "Legacy AD directory should be deleted.");
            Assert.False(Directory.Exists(legacySignDir), "Legacy Sign Language directory should be deleted.");
            Assert.True(Directory.Exists(validMovieDir), "Valid movie directory should remain untouched.");
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}
