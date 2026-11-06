using Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class DownloadOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeMaxParallelDownloadsToDefaultOne()
    {
        // Arrange / Act
        var options = new DownloadOptions();

        // Assert
        Assert.Equal(1, options.MaxParallelDownloads);
    }
}
