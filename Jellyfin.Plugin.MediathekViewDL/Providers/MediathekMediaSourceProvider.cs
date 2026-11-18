using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Providers;

/// <summary>
/// Dynamic MediaSource provider for Mediathek .strm items.
/// Supplies pre-populated Video, Audio, and Subtitle streams for instant playback without FFmpeg probing delays.
/// </summary>
public class MediathekMediaSourceProvider : IMediaSourceProvider, IHasOrder
{
    private readonly ILogger<MediathekMediaSourceProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekMediaSourceProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MediathekMediaSourceProvider(ILogger<MediathekMediaSourceProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public int Order => -100;

    /// <inheritdoc />
    public async Task<IEnumerable<MediaSourceInfo>> GetMediaSources(BaseItem item, CancellationToken cancellationToken)
    {
        if (item is not Movie && item is not Episode)
        {
            return Enumerable.Empty<MediaSourceInfo>();
        }

        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.Federation.EnableFastMediaSourceResolution)
        {
            return Enumerable.Empty<MediaSourceInfo>();
        }

        string? streamUrl = null;

        // 1. From .strm file
        if (!string.IsNullOrWhiteSpace(item.Path) && item.Path.EndsWith(".strm", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (File.Exists(item.Path))
                {
                    var content = (await File.ReadAllTextAsync(item.Path, cancellationToken).ConfigureAwait(false)).Trim();
                    if (config.AllowedDomains.Any(d => content.Contains(d, StringComparison.OrdinalIgnoreCase)))
                    {
                        streamUrl = content;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to read .strm file at {Path}", item.Path);
            }
        }

        // 2. From ProviderIds
        if (string.IsNullOrWhiteSpace(streamUrl) && item.ProviderIds != null)
        {
            if (item.ProviderIds.TryGetValue("MediathekStreamUrl", out var pUrl) && !string.IsNullOrWhiteSpace(pUrl))
            {
                streamUrl = pUrl;
            }
        }

        if (string.IsNullOrWhiteSpace(streamUrl))
        {
            return Enumerable.Empty<MediaSourceInfo>();
        }

        var isDualAudio = item.Name.Contains("Zweikanal", StringComparison.OrdinalIgnoreCase) ||
                          item.Name.Contains("OV", StringComparison.OrdinalIgnoreCase) ||
                          (item.Overview != null && (item.Overview.Contains("Originalton", StringComparison.OrdinalIgnoreCase) || item.Overview.Contains("Zweikanalton", StringComparison.OrdinalIgnoreCase)));

        var streams = new List<MediaStream>
        {
            new MediaStream
            {
                Type = MediaStreamType.Video,
                Index = 0,
                Codec = "h264",
                Width = 1920,
                Height = 1080,
                BitRate = 4500000,
                AverageFrameRate = 50,
                RealFrameRate = 50,
                AspectRatio = "16:9",
                IsDefault = true,
                Title = "1080p H.264 (Mediathek HD)"
            },
            new MediaStream
            {
                Type = MediaStreamType.Audio,
                Index = 1,
                Codec = "aac",
                Language = "deu",
                Channels = 2,
                ChannelLayout = "stereo",
                BitRate = 192000,
                IsDefault = true,
                Title = "Deutsch (Stereo)"
            }
        };

        if (isDualAudio)
        {
            streams.Add(new MediaStream
            {
                Type = MediaStreamType.Audio,
                Index = 2,
                Codec = "aac",
                Language = "eng",
                Channels = 2,
                ChannelLayout = "stereo",
                BitRate = 192000,
                IsDefault = false,
                Title = "Originalton (Englisch)"
            });
        }

        var mediaSource = new MediaSourceInfo
        {
            Id = item.Id.ToString("N"),
            Path = streamUrl,
            Protocol = MediaProtocol.Http,
            Container = "mp4",
            IsRemote = true,
            ReadAtNativeFramerate = false,
            SupportsDirectPlay = true,
            SupportsDirectStream = true,
            SupportsTranscoding = true,
            MediaStreams = streams
        };

        return new[] { mediaSource };
    }

    /// <inheritdoc />
    public Task<ILiveStream> OpenMediaSource(string openToken, List<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
    {
        return Task.FromResult<ILiveStream>(null!);
    }
}
