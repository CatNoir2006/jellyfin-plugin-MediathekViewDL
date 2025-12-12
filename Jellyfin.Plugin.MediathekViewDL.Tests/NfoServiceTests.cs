using System.IO;
using System.Xml.Linq;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class NfoServiceTests
    {
        private readonly Mock<ILogger<NfoService>> _loggerMock;
        private readonly NfoService _nfoService;

        public NfoServiceTests()
        {
            _loggerMock = new Mock<ILogger<NfoService>>();
            _nfoService = new NfoService(_loggerMock.Object);
        }

        [Fact]
        public void CreateNfo_ShouldCreateValidXmlFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var videoPath = Path.ChangeExtension(tempFile, ".mp4");
            var nfoPath = Path.ChangeExtension(videoPath, ".nfo");

            try
            {
                var item = new ResultItem
                {
                    Title = "Test Video",
                    Topic = "Test Show",
                    Description = "This is a test description.",
                    Channel = "ZDF",
                    Timestamp = 1672531200, // 2023-01-01 00:00:00 UTC
                    Id = "12345"
                };

                var videoInfo = new VideoInfo
                {
                    Title = "Clean Title",
                    SeasonNumber = 1,
                    EpisodeNumber = 5
                };

                // Act
                _nfoService.CreateNfo(item, videoInfo, videoPath);

                // Assert
                Assert.True(File.Exists(nfoPath));

                var doc = XDocument.Load(nfoPath);
                var root = doc.Root;
                Assert.NotNull(root);
                Assert.Equal("episodedetails", root.Name.LocalName);
                Assert.Equal("Clean Title", root.Element("title")?.Value);
                Assert.Equal("Test Show", root.Element("showtitle")?.Value);
                Assert.Equal("This is a test description.", root.Element("plot")?.Value);
                Assert.Equal("1", root.Element("season")?.Value);
                Assert.Equal("5", root.Element("episode")?.Value);
                Assert.Equal("ZDF", root.Element("studio")?.Value);
                
                // dateadded is generated based on timestamp, verify format broadly or parse
                var dateAdded = root.Element("dateadded")?.Value;
                Assert.NotNull(dateAdded);
                Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", dateAdded);
            }
            finally
            {
                if (File.Exists(nfoPath)) File.Delete(nfoPath);
                if (File.Exists(tempFile)) File.Delete(tempFile); // cleanup the temp file created by GetTempFileName
            }
        }
    }
}
