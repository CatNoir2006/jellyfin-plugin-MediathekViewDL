using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class VideoParserDateTests
{
    private readonly Mock<ILogger<VideoParser>> _mockLogger;
    private readonly Mock<LanguageDetectionService> _mockLanguageDetectionService;
    private readonly VideoParser _videoParser;

    public VideoParserDateTests()
    {
        _mockLogger = new Mock<ILogger<VideoParser>>();
        _mockLanguageDetectionService = new Mock<LanguageDetectionService>();

        _videoParser = new VideoParser(_mockLogger.Object, _mockLanguageDetectionService.Object);

        _mockLanguageDetectionService.Setup(s => s.DetectLanguage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string title, string _) =>
            {
                return new LanguageDetectionResult { LanguageCode = "de", CleanedTitle = title };
            });
    }

    [Fact]
    public void ParseVideoInfo_ShouldDetectShow_WhenTitleContainsDate()
    {
        // Arrange
        string topic = "tagesschau";
        string title = "tagesschau 20:00 Uhr, 06.01.2026";

        // Act
        var result = _videoParser.ParseVideoInfo(topic, title);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsShow, "Should be identified as a show due to date in title.");
        Assert.Contains("06.01.2026", result.Title); // Date should be preserved
    }
}
