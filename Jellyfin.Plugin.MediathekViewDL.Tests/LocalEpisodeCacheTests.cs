using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class LocalEpisodeCacheTests
{
    [Fact]
    public void Contains_SeasonAndEpisode_ReturnsTrue()
    {
        var cache = new LocalEpisodeCache();
        cache.Add(1, 1, null, "deu");

        Assert.True(cache.Contains(1, 1, null, "deu"));
        Assert.True(cache.Contains(1, 1, 999, "deu")); // Should match on S/E regardless of abs
        Assert.False(cache.Contains(1, 1, null, "eng")); // Different language
    }

    [Fact]
    public void Contains_AbsoluteEpisode_ReturnsTrue()
    {
        var cache = new LocalEpisodeCache();
        cache.Add(null, null, 10, "deu");

        Assert.True(cache.Contains(null, null, 10, "deu"));
        Assert.True(cache.Contains(99, 99, 10, "deu"));
        Assert.False(cache.Contains(null, null, 10, "eng")); // Different language
    }

    [Fact]
    public void Contains_MixedData_ReturnsCorrectly()
    {
        var cache = new LocalEpisodeCache();
        // Add S1E1
        cache.Add(1, 1, null, "deu");
        // Add Abs 5
        cache.Add(null, null, 5, "eng");

        // Match S1E1
        Assert.True(cache.Contains(1, 1, null, "deu"));
        
        // Match Abs 5
        Assert.True(cache.Contains(null, null, 5, "eng"));
        
        // Mixed language checks
        Assert.False(cache.Contains(1, 1, null, "eng"));
        Assert.False(cache.Contains(null, null, 5, "deu"));

        // Match S1E1 with wrong Abs (should match because S/E matches)
        Assert.True(cache.Contains(1, 1, 999, "deu"));

        // Match Abs 5 with wrong S/E (should match because Abs matches)
        Assert.True(cache.Contains(99, 99, 5, "eng"));

        // No match
        Assert.False(cache.Contains(1, 2, null, "deu"));
        Assert.False(cache.Contains(null, null, 6, "eng"));
    }
}
