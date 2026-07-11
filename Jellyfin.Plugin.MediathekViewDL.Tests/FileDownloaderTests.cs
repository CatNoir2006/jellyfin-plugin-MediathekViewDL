using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class FileDownloaderTests : IDisposable
{
    private const string TestUrl = "https://ard.de/video.mp4";
    private readonly Mock<ILogger<FileDownloader>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IConfigurationProvider> _configProviderMock;
    private readonly FileDownloader _downloader;
    private readonly string _tempDir;

    public FileDownloaderTests()
    {
        _loggerMock = new Mock<ILogger<FileDownloader>>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _configProviderMock = new Mock<IConfigurationProvider>();

        var config = new PluginConfiguration
        {
            Maintenance = new Configuration.Groups.MaintenanceOptions { AllowDownloadOnUnknownDiskSpace = true }
        };
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);

        var client = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

        _downloader = new FileDownloader(_loggerMock.Object, _httpClientFactoryMock.Object, _configProviderMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"MediathekViewDL_Tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); }
            catch { /* cleanup best-effort */ }
        }
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, byte[]? content = null)
    {
        var stream = content != null ? new MemoryStream(content) : Stream.Null;
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StreamContent(stream)
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == new Uri(TestUrl)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task DownloadFileAsync_SuccessfulDownload_ReturnsTrueAndCreatesFile()
    {
        // Arrange
        var content = "test file content"u8.ToArray();
        var path = Path.Combine(_tempDir, "video.mp4");
        SetupHttpResponse(HttpStatusCode.OK, content);

        // Act
        var result = await _downloader.DownloadFileAsync(TestUrl, path, null, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(path));
        Assert.Equal(content, await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task DownloadFileAsync_NotFound_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "video.mp4");
        SetupHttpResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _downloader.DownloadFileAsync(TestUrl, path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DownloadFileAsync_Forbidden_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "video.mp4");
        SetupHttpResponse(HttpStatusCode.Forbidden);

        // Act
        var result = await _downloader.DownloadFileAsync(TestUrl, path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DownloadFileAsync_ServerError_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "video.mp4");
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        var result = await _downloader.DownloadFileAsync(TestUrl, path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DownloadFileAsync_EmptyUrl_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "video.mp4");

        // Act
        var result = await _downloader.DownloadFileAsync(string.Empty, path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DownloadFileAsync_NullConfig_ReturnsFalse()
    {
        // Arrange
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns((PluginConfiguration?)null);
        var path = Path.Combine(_tempDir, "video.mp4");

        // Act
        var result = await _downloader.DownloadFileAsync(TestUrl, path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DownloadFileAsync_DisallowedDomain_ReturnsFalse()
    {
        // Arrange
        var config = new PluginConfiguration
        {
            Network = new Configuration.Groups.NetworkOptions { AllowUnknownDomains = false }
        };
        _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
        var path = Path.Combine(_tempDir, "video.mp4");

        // Act
        var result = await _downloader.DownloadFileAsync("https://malicious-site.com/video.mp4", path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DownloadFileAsync_HttpRequestException_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "video.mp4");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _downloader.DownloadFileAsync(TestUrl, path, null, CancellationToken.None);

        // Assert
        Assert.False(result);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task GenerateStreamingUrlFileAsync_SuccessfulCreation_ReturnsTrueAndCreatesFile()
    {
        // Arrange
        var path = Path.Combine(_tempDir, "video.strm");

        // Act
        var result = await _downloader.GenerateStreamingUrlFileAsync(TestUrl, path, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(path));
        Assert.Equal(TestUrl, await File.ReadAllTextAsync(path));
    }
}
