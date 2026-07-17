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
    }
}