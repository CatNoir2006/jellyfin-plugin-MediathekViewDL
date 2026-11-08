using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests
{
    public class PluginConfigurationTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWizardCompletedAsFalse()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.False(config.WizardCompleted);
        }

        [Fact]
        public void WizardCompleted_ShouldBeSettableAndReadable()
        {
            // Arrange
            var config = new PluginConfiguration();

            // Act
            config.WizardCompleted = true;

            // Assert
            Assert.True(config.WizardCompleted);
        }

        [Fact]
        public void Constructor_ShouldInitializeEmptySubscriptionsCollection()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.NotNull(config.Subscriptions);
            Assert.Empty(config.Subscriptions);
        }

        [Fact]
        public void Constructor_ShouldInitializeSubscriptionDownloadSettingsWithProbingSkippedByDefault()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.True(config.SubscriptionDefaults.DownloadSettings.SkipVideoQualityProbing);
        }

        [Fact]
        public void Constructor_ShouldInitializeEnableExternalUrlProviderAsFalse()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.False(config.EnableExternalUrlProvider);
        }

        [Fact]
        public void Constructor_ShouldInitializeMissingEpisodesSyncAsDisabledByDefault()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.NotNull(config.MissingEpisodes);
            Assert.False(config.MissingEpisodes.EnableMissingEpisodesSync);
            Assert.Equal(Configuration.Groups.MissingEpisodeSyncMode.Download, config.MissingEpisodes.SyncMode);
        }

        [Fact]
        public void Constructor_ShouldInitializeVirtualLibraryAsDisabledByDefault()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.NotNull(config.VirtualLibrary);
            Assert.False(config.VirtualLibrary.EnableVirtualLibrary);
            Assert.Equal(string.Empty, config.VirtualLibrary.VirtualLibraryPath);
        }

        [Fact]
        public void Constructor_ShouldInitializeMediaMergingAsDisabledByDefault()
        {
            // Arrange / Act
            var config = new PluginConfiguration();

            // Assert
            Assert.NotNull(config.Merging);
            Assert.False(config.Merging.EnableInstanceMerging);
            Assert.False(config.Merging.AutoDeleteStrmWhenLocalExists);
            Assert.True(config.Merging.GroupAsMultiVersion);
        }
    }
}