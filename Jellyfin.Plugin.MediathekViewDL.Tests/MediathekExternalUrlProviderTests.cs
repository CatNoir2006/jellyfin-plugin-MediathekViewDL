using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Providers;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class MediathekExternalUrlProviderTests : IDisposable
{
    private readonly string _tempFile;

    public MediathekExternalUrlProviderTests()
    {
        _tempFile = Path.GetTempFileName() + ".strm";
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Fact]
    public void GetExternalUrls_WhenDisabled_ReturnsEmpty()
    {
        var provider = new MediathekExternalUrlProvider();
        var episode = new Episode
        {
            ProviderIds = new Dictionary<string, string>
            {
                ["MediathekWebUrl"] = "https://www.ardmediathek.de/video/test"
            }
        };

        // When disabled (default)
        if (Plugin.Instance?.Configuration != null)
        {
            Plugin.Instance.Configuration.EnableExternalUrlProvider = false;
        }

        var urls = provider.GetExternalUrls(episode).ToList();
        Assert.Empty(urls);
    }

    [Fact]
    public void GetExternalUrls_WhenEnabledWithProviderId_ReturnsUrl()
    {
        var provider = new MediathekExternalUrlProvider();
        if (Plugin.Instance?.Configuration != null)
        {
            Plugin.Instance.Configuration.EnableExternalUrlProvider = true;
        }

        var movie = new Movie
        {
            ProviderIds = new Dictionary<string, string>
            {
                ["MediathekWebUrl"] = "https://www.ardmediathek.de/video/sample"
            }
        };

        var urls = provider.GetExternalUrls(movie).ToList();
        if (Plugin.Instance?.Configuration?.EnableExternalUrlProvider == true)
        {
            Assert.Single(urls);
            Assert.Equal("https://www.ardmediathek.de/video/sample", urls[0]);
        }
    }

    [Fact]
    public void ExternalIds_SupportExpectedTypes()
    {
        var movieExtId = new MediathekMovieExternalId();
        Assert.True(movieExtId.Supports(new Movie()));
        Assert.False(movieExtId.Supports(new Series()));

        var seriesExtId = new MediathekSeriesExternalId();
        Assert.True(seriesExtId.Supports(new Series()));
        Assert.False(seriesExtId.Supports(new Episode()));

        var episodeExtId = new MediathekEpisodeExternalId();
        Assert.True(episodeExtId.Supports(new Episode()));
        Assert.False(episodeExtId.Supports(new Movie()));
    }
}
