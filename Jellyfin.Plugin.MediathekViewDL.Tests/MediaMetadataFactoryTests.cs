using System.Collections.Generic;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
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
            Topic = "Original Topic",
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
        Assert.Equal(item.Topic, metadata.OriginalTopic);
        Assert.Equal(item.Description, metadata.Description);
        Assert.Equal(new[] { item.VideoUrls[0].Url, item.VideoUrls[1].Url }, metadata.VideoUrls);

        // Without a VideoInfo, season / episode / absoluteEpisode stay null
        Assert.Null(metadata.SeasonNumber);
        Assert.Null(metadata.EpisodeNumber);
        Assert.Null(metadata.AbsoluteEpisodeNumber);
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

    [Fact]
    public void Create_ShouldPopulateSeasonAndEpisode_FromVideoInfo()
    {
        // Arrange
        var item = BuildItem();
        var videoInfo = new VideoInfo
        {
            Title = "Cleaned Title",
            Topic = "Original Topic",
            SeasonNumber = 3,
            EpisodeNumber = 7,
            AbsoluteEpisodeNumber = 42,
        };

        // Act
        var metadata = MediaMetadataFactory.Create(item, "https://example.com/video-1080.mp4", null, videoInfo);

        // Assert
        Assert.Equal(3, metadata.SeasonNumber);
        Assert.Equal(7, metadata.EpisodeNumber);
        Assert.Equal(42, metadata.AbsoluteEpisodeNumber);
    }

    [Fact]
    public void Create_ShouldPopulateOnlyAbsoluteEpisodeNumber_WhenSeasonAndEpisodeMissing()
    {
        // Arrange
        var item = BuildItem();
        var videoInfo = new VideoInfo
        {
            Title = "Folge 5",
            Topic = "Original Topic",
            AbsoluteEpisodeNumber = 5,
        };

        // Act
        var metadata = MediaMetadataFactory.Create(item, "https://example.com/video-1080.mp4", null, videoInfo);

        // Assert
        Assert.Null(metadata.SeasonNumber);
        Assert.Null(metadata.EpisodeNumber);
        Assert.Equal(5, metadata.AbsoluteEpisodeNumber);
    }

    [Fact]
    public void Create_ShouldPreserveOriginalTopic_EvenWhenSubscriptionNameDiffers()
    {
        // Arrange — verify the original topic is always the API topic, regardless of
        // whatever name the user gave the subscription (handled by the caller, not by
        // this factory).
        var item = BuildItem();
        const string downloadUrl = "https://example.com/video-1080.mp4";

        // Act
        var metadata = MediaMetadataFactory.Create(item, downloadUrl);

        // Assert
        Assert.Equal("Original Topic", metadata.OriginalTopic);
    }
}
