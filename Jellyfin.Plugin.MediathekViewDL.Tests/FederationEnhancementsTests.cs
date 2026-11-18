using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Providers;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Integration;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class FederationEnhancementsTests : IDisposable
{
    private readonly string _tempStrm;

    public FederationEnhancementsTests()
    {
        _tempStrm = Path.Combine(Path.GetTempPath(), "FederationTest_" + Guid.NewGuid().ToString("N") + ".strm");
    }

    public void Dispose()
    {
        if (File.Exists(_tempStrm))
        {
            File.Delete(_tempStrm);
        }
    }

    [Fact]
    public async Task MediathekMediaSourceProvider_WhenDisabled_ReturnsEmpty()
    {
        var provider = new MediathekMediaSourceProvider(NullLogger<MediathekMediaSourceProvider>.Instance);
        var movie = new Movie { Path = _tempStrm };

        if (Plugin.Instance?.Configuration != null)
        {
            Plugin.Instance.Configuration.Federation.EnableFastMediaSourceResolution = false;
        }

        var sources = await provider.GetMediaSources(movie, CancellationToken.None);
        Assert.Empty(sources);
    }

    [Fact]
    public async Task MediathekMediaSourceProvider_WhenEnabledWithValidStrm_ReturnsMediaSource()
    {
        await File.WriteAllTextAsync(_tempStrm, "https://rodlzdf-a.akamaihd.net/test_video.mp4");

        if (Plugin.Instance?.Configuration != null)
        {
            Plugin.Instance.Configuration.Federation.EnableFastMediaSourceResolution = true;
        }

        var provider = new MediathekMediaSourceProvider(NullLogger<MediathekMediaSourceProvider>.Instance);
        var episode = new Episode
        {
            Path = _tempStrm,
            Name = "S01E01 - Pilot (Zweikanalton)"
        };

        var sources = (await provider.GetMediaSources(episode, CancellationToken.None)).ToList();
        if (Plugin.Instance?.Configuration?.Federation.EnableFastMediaSourceResolution == true)
        {
            Assert.Single(sources);
            Assert.Equal("mp4", sources[0].Container);
            Assert.Contains(sources[0].MediaStreams, s => s.Language == "deu");
            Assert.Contains(sources[0].MediaStreams, s => s.Language == "eng");
        }
    }

    [Fact]
    public async Task SeerrNotificationService_SendsPostRequestWhenEnabled()
    {
        var config = new PluginConfiguration();
        config.Federation.AutoTriggerSeerrScan = true;
        config.Federation.SeerrUrl = "http://seerr.test:5055";
        config.Federation.SeerrApiKey = "test_api_key";

        var configProviderMock = new Mock<IConfigurationProvider>();
        configProviderMock.Setup(c => c.ConfigurationOrNull).Returns(config);

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handlerMock.Object);
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        httpFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new SeerrNotificationService(
            NullLogger<SeerrNotificationService>.Instance,
            configProviderMock.Object,
            httpFactoryMock.Object);

        var result = await service.TriggerRecentlyAddedScanAsync(CancellationToken.None);
        Assert.True(result);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("/api/v1/settings/jobs/jellyfin-recently-added-scan/run")),
            ItExpr.IsAny<CancellationToken>());
    }
}
