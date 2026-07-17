using System.Collections.Generic;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class MediaMetadataFactoryTests
{
    private static ResultItemDto BuildItem()
    {
        return new ResultItemDto
        {
            Id = "id-123",
            Title = "Original Titel",
            Topic = "Topic",
            Channel = "ARD",
            Description = "Eine Beschreibung.",
            Timestamp = System.DateTimeOffset.UnixEpoch,
            Duration = System.TimeSpan.FromMinutes(45),
            Size = 123456,
            VideoUrls = new List<VideoUrlDto>
            {
                new() { Url = "https://example.com/video-1080.mp4", Quality = 3 },
                new() { Url = "https://example.com/video-720.mp4", Quality = 2 },
            },
            SubtitleUrls = new List<SubtitleUrlDto>
            {
                new() { Url = "https://example.com/sub.vtt", Type = SubtitleType.WEBVTT },
            },
            ExternalIds = new List<ExternalId>(),
        };
    }

    [Fact]
    public void Create_ShouldPopulateAllRequiredFields()
    {
        // Arrange
        var item = BuildItem();
        const string downloadUrl = "https://example.com/video-1080.mp4";
        const string subtitleUrl = "https://example.com/sub.vtt";

        // Act
        var metadata = MediaMetadataFactory.Create(item, downloadUrl, subtitleUrl);

        // Assert
        Assert.Equal(item.Id, metadata.Id);
        Assert.Equal(downloadUrl, metadata.DownloadUrl);
        Assert.Equal(subtitleUrl, metadata.SubtitleUrl);
        Assert.Equal(item.Title, metadata.OriginalTitle);
        Assert.Equal(item.Description, metadata.Description);
        Assert.Equal(new[] { item.VideoUrls[0].Url, item.VideoUrls[1].Url }, metadata.VideoUrls);
    }

    [Fact]
    public void Create_ShouldAllowNullSubtitleUrl()
    {
        // Arrange
        var item = BuildItem();
        const string downloadUrl = "https://example.com/video-1080.mp4";

        // Act
        var metadata = MediaMetadataFactory.Create(item, downloadUrl, null);

        // Assert
        Assert.Null(metadata.SubtitleUrl);
    }

    [Fact]
    public void Create_ShouldIgnoreEmptyVideoUrls()
    {
        // Arrange
        var item = new ResultItemDto
        {
            Id = "id",
            Title = "t",
            Topic = "to",
            Channel = "c",
            Description = "d",
            VideoUrls = new List<VideoUrlDto>
            {
                new() { Url = "https://example.com/a.mp4" },
                new() { Url = string.Empty },
                new() { Url = "   " },
            },
            SubtitleUrls = new List<SubtitleUrlDto>(),
            ExternalIds = new List<ExternalId>(),
        };

        // Act
        var metadata = MediaMetadataFactory.Create(item, "https://example.com/a.mp4");

        // Assert
        Assert.Single(metadata.VideoUrls);
        Assert.Equal("https://example.com/a.mp4", metadata.VideoUrls[0]);
    }
}
