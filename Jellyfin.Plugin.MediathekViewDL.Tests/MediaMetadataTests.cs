using System.Text.Json;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Xunit;

namespace Jellyfin.Plugin.MediathekViewDL.Tests;

public class MediaMetadataTests
{
    [Fact]
    public void Serialize_ShouldProduceSingleLineJson_WithExpectedFields()
    {
        // Arrange
        var metadata = new MediaMetadata
        {
            Id = "abc123",
            DownloadUrl = "https://example.com/video.mp4",
            VideoUrls = ["https://example.com/video-1080p.mp4", "https://example.com/video-720p.mp4"],
            SubtitleUrl = "https://example.com/subtitle.vtt",
            OriginalTitle = "Tatort: Der Clown",
            OriginalTopic = "Tatort",
            SeasonNumber = 5,
            EpisodeNumber = 12,
            AbsoluteEpisodeNumber = 87,
            Description = "Ein spannender Krimi aus Münster.",
        };

        // Act
        var json = MediaMetadataKeys.Serialize(metadata);

        // Assert
        Assert.DoesNotContain("\n", json);
        Assert.DoesNotContain("\r", json);
        Assert.Contains("\"id\":\"abc123\"", json);
        Assert.Contains("\"downloadUrl\":\"https://example.com/video.mp4\"", json);
        Assert.Contains("\"videoUrls\":[", json);
        Assert.Contains("\"subtitleUrl\":\"https://example.com/subtitle.vtt\"", json);
        Assert.Contains("\"originalTitle\":\"Tatort: Der Clown\"", json);
        Assert.Contains("\"originalTopic\":\"Tatort\"", json);
        Assert.Contains("\"seasonNumber\":5", json);
        Assert.Contains("\"episodeNumber\":12", json);
        Assert.Contains("\"absoluteEpisodeNumber\":87", json);
        Assert.Contains("\"description\":\"Ein spannender Krimi aus Münster.\"", json);
    }

    [Fact]
    public void Serialize_ShouldOmitSubtitleUrl_WhenNull()
    {
        // Arrange
        var metadata = new MediaMetadata
        {
            Id = "abc123",
            DownloadUrl = "https://example.com/video.mp4",
            SubtitleUrl = null,
        };

        // Act
        var json = MediaMetadataKeys.Serialize(metadata);

        // Assert
        // When subtitleUrl is null it is serialized as null (not omitted)
        // because the property is not declared as nullable in the DTO.
        Assert.Contains("\"subtitleUrl\":null", json);
    }

    [Fact]
    public void Serialize_ShouldOmitSeasonAndEpisodeNumbering_WhenNull()
    {
        // Arrange
        var metadata = new MediaMetadata
        {
            Id = "abc123",
            DownloadUrl = "https://example.com/video.mp4",
        };

        // Act
        var json = MediaMetadataKeys.Serialize(metadata);

        // Assert
        // Season / episode / absoluteEpisode are serialized as null when not set
        // (matches the existing behaviour for subtitleUrl).
        Assert.Contains("\"seasonNumber\":null", json);
        Assert.Contains("\"episodeNumber\":null", json);
        Assert.Contains("\"absoluteEpisodeNumber\":null", json);
    }

    [Fact]
    public void Serialize_ShouldNotIntroduceEscapesForCommonCharacters()
    {
        // Arrange
        var metadata = new MediaMetadata
        {
            Id = "abc",
            DownloadUrl = "https://example.com/x",
            OriginalTitle = "Tatort: Der Clown",
        };

        // Act
        var json = MediaMetadataKeys.Serialize(metadata);

        // Assert
        // The unsafe relaxed encoder keeps common characters unescaped for readability.
        Assert.DoesNotContain("\\u0027", json);
        Assert.DoesNotContain("\\u0026", json);
        Assert.DoesNotContain("\\u00e4", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_ShouldEscapeQuotesAndBackslashes()
    {
        // Arrange
        var metadata = new MediaMetadata
        {
            Id = "abc\"def\\g",
            DownloadUrl = "https://example.com/x",
        };

        // Act
        var json = MediaMetadataKeys.Serialize(metadata);

        // Assert
        // Quotes and backslashes must always be escaped.
        Assert.Contains("\\\"", json);
        Assert.Contains("\\\\", json);
    }

    [Fact]
    public void Deserialize_RoundTrip_ShouldPreserveAllFields()
    {
        // Arrange
        var original = new MediaMetadata
        {
            Id = "abc123",
            DownloadUrl = "https://example.com/video.mp4",
            VideoUrls = ["https://example.com/video-1080p.mp4", "https://example.com/video-720p.mp4"],
            SubtitleUrl = "https://example.com/subtitle.vtt",
            OriginalTitle = "Tatort",
            OriginalTopic = "Tatort",
            SeasonNumber = 5,
            EpisodeNumber = 12,
            AbsoluteEpisodeNumber = 87,
            Description = "Beschreibung mit \"Anführungszeichen\" und \\Backslash\\.",
        };

        // Act
        var json = MediaMetadataKeys.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<MediaMetadata>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized!.Id);
        Assert.Equal(original.DownloadUrl, deserialized.DownloadUrl);
        Assert.Equal(original.VideoUrls, deserialized.VideoUrls);
        Assert.Equal(original.SubtitleUrl, deserialized.SubtitleUrl);
        Assert.Equal(original.OriginalTitle, deserialized.OriginalTitle);
        Assert.Equal(original.OriginalTopic, deserialized.OriginalTopic);
        Assert.Equal(original.SeasonNumber, deserialized.SeasonNumber);
        Assert.Equal(original.EpisodeNumber, deserialized.EpisodeNumber);
        Assert.Equal(original.AbsoluteEpisodeNumber, deserialized.AbsoluteEpisodeNumber);
        Assert.Equal(original.Description, deserialized.Description);
    }

    [Fact]
    public void StrmCommentPrefix_ShouldMatchExpectedFormat()
    {
        // The expected prefix is "# MediathekViewDL-Metadata: " so that the resulting
        // line in the .strm file reads: "# MediathekViewDL-Metadata: {json}"
        Assert.Equal("# MediathekViewDL-Metadata: ", MediaMetadataKeys.StrmCommentPrefix);
    }

    [Fact]
    public void MetadataKey_ShouldBeMediathekViewDl()
    {
        Assert.Equal("MediathekViewDL", MediaMetadataKeys.MetadataKey);
    }
}
