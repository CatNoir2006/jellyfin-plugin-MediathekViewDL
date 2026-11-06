using System;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediathekView.Api.External;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class MediathekViewApiClientOptionsMonitorTests
{
    private readonly Mock<IConfigurationProvider> _configProviderMock;

    public MediathekViewApiClientOptionsMonitorTests()
    {
        _configProviderMock = new Mock<IConfigurationProvider>();
    }

    private MediathekViewApiClientOptionsMonitor CreateMonitor()
    {
        return new MediathekViewApiClientOptionsMonitor(_configProviderMock.Object);
    }

    [Fact]
    public void Get_ShouldReturnConfiguredApiBaseUrl_WhenSet()
    {
        // Arrange
        var config = new PluginConfiguration
        {
            Network = new Configuration.Groups.NetworkOptions { ApiBaseUrl = "http://localhost:8000" }
        };
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
        var monitor = CreateMonitor();

        // Act
        var options = monitor.Get(null);

        // Assert
        Assert.Equal("http://localhost:8000", options.ApiBaseUrl);
    }

    [Fact]
    public void Get_ShouldReturnDefaultApiBaseUrl_WhenConfiguredValueIsEmpty()
    {
        // Arrange
        var config = new PluginConfiguration
        {
            Network = new Configuration.Groups.NetworkOptions { ApiBaseUrl = string.Empty }
        };
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
        var monitor = CreateMonitor();

        // Act
        var options = monitor.Get(null);

        // Assert
        Assert.Equal(MediathekViewApiClientOptions.DefaultApiBaseUrl, options.ApiBaseUrl);
    }

    [Fact]
    public void Get_ShouldReturnDefaultApiBaseUrl_WhenConfigurationIsNull()
    {
        // Arrange
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns((PluginConfiguration?)null);
        var monitor = CreateMonitor();

        // Act
        var options = monitor.Get(null);

        // Assert
        Assert.Equal(MediathekViewApiClientOptions.DefaultApiBaseUrl, options.ApiBaseUrl);
    }

    [Fact]
    public void Get_ShouldAlwaysReturnNewOptionsInstance_WhenCalledRepeatedly()
    {
        // Arrange
        var config = new PluginConfiguration();
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
        var monitor = CreateMonitor();

        // Act
        var first = monitor.Get(null);
        var second = monitor.CurrentValue;

        // Assert
        Assert.NotSame(first, second);
        Assert.Equal(first.ApiBaseUrl, second.ApiBaseUrl);
    }

    [Fact]
    public void OnChange_ShouldReturnDisposable()
    {
        // Arrange
        var config = new PluginConfiguration();
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
        var monitor = CreateMonitor();

        // Act
        var disposable = monitor.OnChange((_, _) => { });

        // Assert
        Assert.NotNull(disposable);
    }
}
