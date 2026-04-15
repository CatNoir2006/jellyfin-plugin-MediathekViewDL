using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class StrmValidationServiceTests
{
    private readonly Mock<ILogger<StrmValidationService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IConfigurationProvider> _configProviderMock;
    private readonly StrmValidationService _service;
    private readonly PluginConfiguration _testConfig;

    public StrmValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<StrmValidationService>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _configProviderMock = new Mock<IConfigurationProvider>();
        _testConfig = new PluginConfiguration();

        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(_testConfig);

        var client = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

        _service = new StrmValidationService(_loggerMock.Object, _httpClientFactoryMock.Object, _configProviderMock.Object);
    }

    [Fact]
    public async Task ValidateUrlAsync_ValidUrl_ReturnsTrue()
    {
        // Arrange
        var url = "https://ard.de/video.mp4";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head && req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await _service.ValidateUrlAsync(url, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateUrlAsync_NotFound_ReturnsFalse()
    {
        // Arrange
        var url = "https://ard.de/deleted.mp4";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head && req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var result = await _service.ValidateUrlAsync(url, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateUrlAsync_MethodNotAllowed_RetriesWithGet()
    {
        // Arrange
        var url = "https://ard.de/video.mp4";

        // First HEAD request returns MethodNotAllowed
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head && req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));

        // Second GET request returns OK
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await _service.ValidateUrlAsync(url, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateUrlAsync_InvalidDomain_ThrowsException()
    {
        // Arrange
        var url = "https://malicious-site.com/video.mp4";
        _testConfig.Network.AllowUnknownDomains = false;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ValidateUrlAsync(url, CancellationToken.None));

        // Ensure no HTTP call was made
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ValidateUrlAsync_NotHttps_ThrowsException()
    {
        // Arrange
        var url = "http://ard.de/video.mp4";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateUrlAsync(url, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateUrlAsync_ServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var url = "https://ard.de/video.mp4";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head && req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ValidateUrlAsync(url, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateUrlAsync_EmptyUrl_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateUrlAsync("", CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateUrlAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task ValidateUrlAsync_InvalidUrlFormat_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateUrlAsync("not_a_url", CancellationToken.None));
    }

    [Fact]
    public async Task ValidateUrlAsync_Gone_ReturnsFalse()
    {
        // Arrange
        var url = "https://ard.de/gone.mp4";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head && req.RequestUri == new Uri(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Gone));

        // Act
        var result = await _service.ValidateUrlAsync(url, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
