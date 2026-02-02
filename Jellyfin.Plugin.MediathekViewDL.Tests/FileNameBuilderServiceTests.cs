using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class FileNameBuilderServiceTests
    {
        private readonly Mock<ILogger<FileNameBuilderService>> _loggerMock;
        private readonly Mock<IConfigurationProvider> _configProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public FileNameBuilderServiceTests()
        {
            _loggerMock = new Mock<ILogger<FileNameBuilderService>>();
            _configProviderMock = new Mock<IConfigurationProvider>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _libraryManagerMock.Setup(x => x.GetVirtualFolders(It.IsAny<bool>())).Returns(new List<VirtualFolderInfo>());
        }

        [Fact]
        public void SanitizeFileName_ShouldRemoveInvalidCharacters()
        {
            // Arrange
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);
            string unsafeName = "File/Name:With*Invalid?Chars";

            // Act
            string sanitized = service.SanitizeFileName(unsafeName);

            // Assert
            Assert.DoesNotContain("/", sanitized);
            Assert.DoesNotContain(":", sanitized);
            Assert.DoesNotContain("*", sanitized);
            Assert.DoesNotContain("?", sanitized);
        }

        [Fact]
        public void SanitizeDirectoryName_ShouldRemoveInvalidCharacters()
        {
            // Arrange
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);
            string unsafeName = "Folder/Name:With*Invalid?Chars";

            // Act
            string sanitized = service.SanitizeDirectoryName(unsafeName);

            // Assert
            Assert.DoesNotContain("/", sanitized);
            Assert.DoesNotContain(":", sanitized);
            Assert.DoesNotContain("*", sanitized);
            Assert.DoesNotContain("?", sanitized);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldReturnValidPaths_ForSimpleVideo()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo
            {
                Title = "TestVideo",
                Language = "deu"
            };

            var subscription = new Subscription { Name = "TestSub" };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            Assert.True(paths.IsValid);
            Assert.Equal(Path.Combine("/tmp/downloads", "TestVideo"), paths.DirectoryPath);
            Assert.EndsWith("TestVideo.mkv", paths.MainFilePath);
            Assert.EndsWith("TestVideo.deu.ttml", paths.SubtitleFilePath);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldIncludeTopicFolder_ForMovie_WhenEnabled()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            config.Paths.UseTopicForMoviePath = true;

            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo
            {
                Title = "TestMovie",
                IsShow = false,
                Language = "deu"
            };

            var subscription = new Subscription { Name = "TestTopic" };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            Assert.Equal(Path.Combine("/tmp/downloads", "TestTopic", "TestMovie"), paths.DirectoryPath);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldHandleSeasonEpisodeNumbering()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo
            {
                Title = "EpisodeTitle",
                IsShow = true,
                SeasonNumber = 1,
                EpisodeNumber = 5,
                Language = "deu"
            };

            var subscription = new Subscription { Name = "MySeries" };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            // Expected folder: /tmp/downloads/MySeries/Staffel 1
            var expectedDir = Path.Combine("/tmp/downloads", "MySeries", "Staffel 1");
            Assert.Equal(expectedDir, paths.DirectoryPath);

            // Expected file: S01E05 - EpisodeTitle.mkv
            Assert.Contains("S01E05 - EpisodeTitle.mkv", paths.MainFilePath);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldUseSubscriptionPath_IfSet()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo { Title = "Test" };
            var subscription = new Subscription 
            { 
                Name = "TestSub", 
                DownloadPath = "/custom/path/MyShow" 
            };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            Assert.Equal(Path.Combine("/custom/path/MyShow", "Test"), paths.DirectoryPath);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldHandleTrailers_WhenTreatNonEpisodesAsExtrasEnabled()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo { Title = "Trailer", IsTrailer = true };
            var subscription = new Subscription 
            { 
                Name = "TestSub", 
                TreatNonEpisodesAsExtras = true 
            };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            Assert.EndsWith("trailers", paths.DirectoryPath);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldAppendLanguage_WhenNotGerman()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo 
            { 
                Title = "EnglishMovie", 
                Language = "eng" 
            };
            var subscription = new Subscription { Name = "TestSub" };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            // Expect: EnglishMovie.eng.mka (audio only by default for non-German unless configured otherwise)
            // Wait, logic says: if (videoInfo.Language == "deu" || subscription.DownloadFullVideoForSecondaryAudio) -> .mkv
            // else -> .mka
            
            Assert.Contains(".eng.mka", paths.MainFilePath);
        }

         [Fact]
        public void GenerateDownloadPaths_ShouldReturnMkv_WhenNotGerman_AndDownloadFullVideoEnabled()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var videoInfo = new VideoInfo 
            { 
                Title = "EnglishMovie", 
                Language = "eng" 
            };
            var subscription = new Subscription 
            { 
                Name = "TestSub",
                DownloadFullVideoForSecondaryAudio = true
            };

            // Act
            var paths = service.GenerateDownloadPaths(videoInfo, subscription, DownloadContext.Subscription);

            // Assert
            Assert.Contains(".eng.mkv", paths.MainFilePath);
        }

        [Fact]
        public void GenerateDownloadPaths_ShouldUseSpecificDefaultPaths_WhenConfigured()
        {
            // Arrange
            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = "/tmp/downloads";
            config.Paths.DefaultSubscriptionShowPath = "/tmp/shows";
            config.Paths.DefaultSubscriptionMoviePath = "/tmp/movies";
            config.Paths.DefaultManualShowPath = "/manual/shows";
            config.Paths.DefaultManualMoviePath = "/manual/movies";

            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            var showInfo = new VideoInfo { Title = "Show", IsShow = true };
            var movieInfo = new VideoInfo { Title = "Movie", IsShow = false };
            var subscription = new Subscription { Name = "Test" };

            // Act
            var subShowPaths = service.GenerateDownloadPaths(showInfo, subscription, DownloadContext.Subscription);
            var subMoviePaths = service.GenerateDownloadPaths(movieInfo, subscription, DownloadContext.Subscription);
            var manualShowPaths = service.GenerateDownloadPaths(showInfo, subscription, DownloadContext.Manual);
            var manualMoviePaths = service.GenerateDownloadPaths(movieInfo, subscription, DownloadContext.Manual);

            // Assert
            Assert.StartsWith("/tmp/shows", subShowPaths.DirectoryPath);
            Assert.StartsWith("/tmp/movies", subMoviePaths.DirectoryPath);
            Assert.StartsWith("/manual/shows", manualShowPaths.DirectoryPath);
            Assert.StartsWith("/manual/movies", manualMoviePaths.DirectoryPath);
        }

        [Fact]
        public void IsPathSafe_ShouldReturnTrue_ForPathsInsideAllowedDirectories()
        {
            // Arrange
            var tempBase = Path.GetTempPath();
            var allowedDir = Path.Combine(tempBase, "Allowed");
            Directory.CreateDirectory(allowedDir);

            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = allowedDir;
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            // Act & Assert
            Assert.True(service.IsPathSafe(Path.Combine(allowedDir, "Subfolder")));
            Assert.True(service.IsPathSafe(allowedDir));
        }

        [Fact]
        public void IsPathSafe_ShouldReturnFalse_ForPathsOutsideAllowedDirectories()
        {
            // Arrange
            var tempBase = Path.GetTempPath();
            var allowedDir = Path.Combine(tempBase, "Allowed");
            var unsafeDir = Path.Combine(tempBase, "Unsafe");
            Directory.CreateDirectory(allowedDir);
            Directory.CreateDirectory(unsafeDir);

            var config = new PluginConfiguration();
            config.Paths.DefaultDownloadPath = allowedDir;
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            // Act & Assert
            Assert.False(service.IsPathSafe(unsafeDir));
            Assert.False(service.IsPathSafe(Path.Combine(allowedDir, "..", "Unsafe")));
        }

        [Fact]
        public void IsPathSafe_ShouldReturnTrue_ForJellyfinLibraryPaths()
        {
            // Arrange
            var tempBase = Path.GetTempPath();
            var libraryDir = Path.Combine(tempBase, "Library");
            Directory.CreateDirectory(libraryDir);

            var config = new PluginConfiguration();
            _configProviderMock.Setup(x => x.ConfigurationOrNull).Returns(config);
            
            _libraryManagerMock.Setup(x => x.GetVirtualFolders(It.IsAny<bool>()))
                .Returns(new List<VirtualFolderInfo> 
                { 
                    new VirtualFolderInfo { Locations = new[] { libraryDir } } 
                });

            var service = new FileNameBuilderService(_loggerMock.Object, _configProviderMock.Object, _libraryManagerMock.Object);

            // Act & Assert
            Assert.True(service.IsPathSafe(Path.Combine(libraryDir, "MyShow")));
        }
    }
}
