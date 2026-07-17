using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

/// <summary>
/// Factory for creating <see cref="MediaMetadata"/> instances from API result items.
/// </summary>
public static class MediaMetadataFactory
{
    /// <summary>
    /// Creates a <see cref="MediaMetadata"/> from the given result item and the download URL
    /// that was actually used (or will be used) for the download.
    /// </summary>
    /// <param name="item">The result item from the MediathekView API.</param>
    /// <param name="downloadUrl">The URL that was selected for the download.</param>
    /// <param name="subtitleUrl">The optional URL of the preferred subtitle.</param>
    /// <returns>The populated <see cref="MediaMetadata"/> instance.</returns>
    public static MediaMetadata Create(ResultItemDto item, string downloadUrl, string? subtitleUrl = null)
    {
        var videoUrls = item.VideoUrls
            .Select(v => v.Url)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList();

        return new MediaMetadata
        {
            Id = item.Id,
            DownloadUrl = downloadUrl,
            VideoUrls = videoUrls,
            SubtitleUrl = subtitleUrl,
            OriginalTitle = item.Title,
            Description = item.Description,
        };
    }
}
