using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Microsoft.Extensions.Logging;
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
                    Results = new System.Collections.ObjectModel.Collection<ResultItem>() 
                } 
            };
            var json = JsonSerializer.Serialize(validResponse);
            sequence.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockLogger = new Mock<ILogger<MediathekViewApiClient>>();
            var client = new MediathekViewApiClient(httpClient, mockLogger.Object);

            // Act
            await client.SearchAsync(new ApiQuery(), CancellationToken.None);

            // Assert
            // Verify that SendAsync was called 3 times (2 failures + 1 success)
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
