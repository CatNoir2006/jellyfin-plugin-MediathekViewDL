using System;
using System.IO;
using System.Xml.Linq;
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
        public void CreateNfo_Episode_ShouldCreateValidXmlFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var videoPath = Path.ChangeExtension(tempFile, ".mp4");
            var nfoPath = Path.ChangeExtension(videoPath, ".nfo");

            try
            {
                var dto = new NfoDTO
                {
                    FilePath = nfoPath,
                    Title = "Clean Title",
                    Show = "Test Show",
                    Description = "This is a test description.",
                    Season = 1,
                    Episode = 5,
                    Studio = "ZDF",
                    Id = "12345",
                    AirDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                };

                // Act
                _nfoService.CreateNfo(dto);

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
                
                // Studio should NOT be present in the new implementation
                Assert.Null(root.Element("studio"));
                
                // DateAdded
                var dateAdded = root.Element("dateadded")?.Value;
                Assert.NotNull(dateAdded);
                Assert.Equal("2023-01-01 00:00:00", dateAdded);
                
                // UniqueID
                var uniqueId = root.Element("uniqueid");
                Assert.NotNull(uniqueId);
                Assert.Equal("12345", uniqueId.Value);
                Assert.Equal("MediathekView", uniqueId.Attribute("type")?.Value);
            }
            finally
            {
                if (File.Exists(nfoPath)) File.Delete(nfoPath);
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void CreateNfo_Movie_ShouldCreateValidXmlFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var videoPath = Path.ChangeExtension(tempFile, ".mp4");
            var nfoPath = Path.ChangeExtension(videoPath, ".nfo");

            try
            {
                var dto = new NfoDTO
                {
                    FilePath = nfoPath,
                    Title = "Movie Title",
                    Description = "Movie description.",
                    Season = null, // No season -> Movie
                    Episode = null,
                    Studio = "ARD",
                    Id = "67890",
                    AirDate = new DateTime(2023, 5, 20, 20, 15, 0, DateTimeKind.Utc)
                };

                // Act
                _nfoService.CreateNfo(dto);

                // Assert
                Assert.True(File.Exists(nfoPath));

                var doc = XDocument.Load(nfoPath);
                var root = doc.Root;
                Assert.NotNull(root);
                Assert.Equal("movie", root.Name.LocalName);
                Assert.Equal("Movie Title", root.Element("title")?.Value);
                Assert.Equal("Movie description.", root.Element("plot")?.Value);
                Assert.Null(root.Element("season"));
                Assert.Null(root.Element("episode"));
                
                // Studio should NOT be present
                Assert.Null(root.Element("studio"));

                 // DateAdded
                var dateAdded = root.Element("dateadded")?.Value;
                Assert.NotNull(dateAdded);
                Assert.Equal("2023-05-20 20:15:00", dateAdded);
            }
            finally
            {
                if (File.Exists(nfoPath)) File.Delete(nfoPath);
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}