using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediathekView.Api.External;
using MediathekView.Api.External.Models;
using MediathekView.Api.Models;
using MediathekView.Api.Models.Enums;
using MediathekView.Api.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class MediathekViewApiClientTests
    {
        [Fact]
        public async Task SearchAsync_ShouldRetry_WhenApiReturns500()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var sequence = mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );

            // Fail twice with 500 Internal Server Error
            sequence.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            sequence.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Then succeed with 200 OK and valid JSON
            var validResponse = new ApiResult
            {
                Result = new ResultChannels
                {
                    Results = new Collection<ResultItem>(),
                    QueryInfo = new QueryInfo()
                }
            };
            var json = JsonSerializer.Serialize(validResponse);
            sequence.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.SearchAsync(new ApiQueryDto(), CancellationToken.None);

            // Assert
            // Verify that SendAsync was called 3 times (2 failures + 1 success)
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SearchAsync_ShouldPostJsonToQueryEndpoint_WhenCalled()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            HttpRequestMessage? capturedRequest = null;
            string? capturedBody = null;

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    if (req.Content != null)
                    {
                        capturedBody = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    var validResponse = new ApiResult
                    {
                        Result = new ResultChannels
                        {
                            Results = new Collection<ResultItem>(),
                            QueryInfo = new QueryInfo()
                        }
                    };
                    var json = JsonSerializer.Serialize(validResponse);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json)
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.SearchAsync(new ApiQueryDto(), CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
            Assert.Equal("https://mediathekviewweb.de/api/query", capturedRequest.RequestUri!.ToString());
            Assert.NotNull(capturedBody);
            using var parsed = JsonDocument.Parse(capturedBody!);
            Assert.True(parsed.RootElement.TryGetProperty("size", out _));
            Assert.True(parsed.RootElement.TryGetProperty("offset", out _));
        }

        [Fact]
        public async Task SearchAsync_ShouldThrowMediathekApiException_WhenApiReturns400()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.SearchAsync(new ApiQueryDto(), CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task SearchAsync_ShouldThrowMediathekApiException_WhenApiReturns404()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.SearchAsync(new ApiQueryDto(), CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task SearchAsync_ShouldThrowMediathekApiException_WhenAllRetriesFailWith500()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.SearchAsync(new ApiQueryDto(), CancellationToken.None).ConfigureAwait(false));
            Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        }

        [Fact]
        public async Task SearchAsync_ShouldThrowMediathekParsingException_WhenApiReturnsInvalidJson()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{not valid json")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekParsingException>(
                async () => await client.SearchAsync(new ApiQueryDto(), CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task SearchAsync_ShouldThrowMediathekParsingException_WhenResultIsNull()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"result\": null }")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekParsingException>(
                async () => await client.SearchAsync(new ApiQueryDto(), CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task SearchAsync_ShouldSendConfiguredSize_WhenApiQueryHasSize()
        {
            // Arrange
            int? capturedSize = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    if (req.Content != null)
                    {
                        var body = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
                        using var parsed = JsonDocument.Parse(body);
                        if (parsed.RootElement.TryGetProperty("size", out var sizeElement))
                        {
                            capturedSize = sizeElement.GetInt32();
                        }
                    }

                    var validResponse = new ApiResult
                    {
                        Result = new ResultChannels
                        {
                            Results = new Collection<ResultItem>(),
                            QueryInfo = new QueryInfo { TotalResults = 0 }
                        }
                    };
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(validResponse))
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions { MaxSearchPages = 5 });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.SearchAsync(new ApiQueryDto { Size = 42 }, CancellationToken.None);

            // Assert
            Assert.Equal(42, capturedSize);
        }

        [Fact]
        public async Task SearchAsync_ShouldUseConfiguredApiBaseUrl_WhenSet()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    var validResponse = new ApiResult
                    {
                        Result = new ResultChannels
                        {
                            Results = new Collection<ResultItem>(),
                            QueryInfo = new QueryInfo()
                        }
                    };
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(validResponse))
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.SearchAsync(new ApiQueryDto(), CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("http://localhost:8000/api/query", capturedRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task SearchAsync_ShouldUseDefaultApiBaseUrl_WhenConfiguredValueIsEmpty()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    var validResponse = new ApiResult
                    {
                        Result = new ResultChannels
                        {
                            Results = new Collection<ResultItem>(),
                            QueryInfo = new QueryInfo()
                        }
                    };
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(validResponse))
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "   ",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.SearchAsync(new ApiQueryDto(), CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("https://mediathekviewweb.de/api/query", capturedRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task SearchAsync_ShouldThrowMediathekApiException_WhenApiBaseUrlIsInvalid()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "not a valid url",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.SearchAsync(new ApiQueryDto(), CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task SearchAsync_ShouldTrimTrailingSlash_WhenApiBaseUrlEndsWithSlash()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    var validResponse = new ApiResult
                    {
                        Result = new ResultChannels
                        {
                            Results = new Collection<ResultItem>(),
                            QueryInfo = new QueryInfo()
                        }
                    };
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(validResponse))
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000/",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.SearchAsync(new ApiQueryDto(), CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("http://localhost:8000/api/query", capturedRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldCallStatsEndpoint_WhenServerRespondsCorrectly()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(MediathekViewApiClientOptions.ExpectedStatsResponse)
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.CheckConnectionAsync(null, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("http://localhost:8000/stats", capturedRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldAcceptTrailingPeriod_WhenServerHttpBodyContainsOne()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(MediathekViewApiClientOptions.ExpectedStatsResponse + ".")
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.CheckConnectionAsync(null, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("http://localhost:8000/stats", capturedRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldUseBaseUrlOverride_WhenProvided()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Returns(async (HttpRequestMessage req, CancellationToken _) =>
                {
                    capturedRequest = req;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(MediathekViewApiClientOptions.ExpectedStatsResponse)
                    };
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "https://mediathekviewweb.de",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act
            await client.CheckConnectionAsync("http://localhost:8000", CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("http://localhost:8000/stats", capturedRequest!.RequestUri!.ToString());
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldThrowMediathekApiException_WhenResponseBodyIsUnexpected()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Hello world")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.CheckConnectionAsync(null, CancellationToken.None).ConfigureAwait(false));
            Assert.Contains("Server is up and running", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldThrowMediathekApiException_WhenServerRespondsWithErrorStatusCode()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.CheckConnectionAsync(null, CancellationToken.None).ConfigureAwait(false));
            Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldThrowMediathekConnectionException_WhenServerIsNotReachable()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "http://localhost:8000",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekConnectionException>(
                async () => await client.CheckConnectionAsync(null, CancellationToken.None).ConfigureAwait(false));
        }

        [Fact]
        public async Task CheckConnectionAsync_ShouldThrowMediathekApiException_WhenBaseUrlIsInvalid()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var optionsMonitor = new TestOptionsMonitor(new MediathekViewApiClientOptions
            {
                ApiBaseUrl = "not a valid url",
                MaxSearchPages = 5
            });

            var client = new MediathekViewApiClient(httpClient, mockLogger.Object, optionsMonitor);

            // Act + Assert
            await Assert.ThrowsAsync<MediathekApiException>(
                async () => await client.CheckConnectionAsync(null, CancellationToken.None).ConfigureAwait(false));
        }

        private sealed class TestOptionsMonitor : IOptionsMonitor<MediathekViewApiClientOptions>
        {
            private readonly MediathekViewApiClientOptions _options;

            public TestOptionsMonitor(MediathekViewApiClientOptions options)
            {
                _options = options;
            }

            public MediathekViewApiClientOptions CurrentValue => _options;

            public MediathekViewApiClientOptions Get(string? name) => _options;

            public IDisposable? OnChange(Action<MediathekViewApiClientOptions, string?> listener) => null;
        }
    }
}
