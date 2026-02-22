using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.MediathekViewDL.LiveTv;

/// <summary>
/// A tuner host for Zapp channels.
/// </summary>
public class ZappTunerHost : ITunerHost, IConfigurableTunerHost
{
    private readonly IMediathekViewApiClient _apiClient;
    private readonly ILogger<ZappTunerHost> _logger;
    private readonly IMediaSourceManager _mediaSourceManager;
    private readonly INetworkManager _networkManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZappTunerHost"/> class.
    /// </summary>
    /// <param name="apiClient">The api client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="mediaSourceManager">The media source manager.</param>
    /// <param name="networkManager">The network manager.</param>
    public ZappTunerHost(
        IMediathekViewApiClient apiClient,
        ILogger<ZappTunerHost> logger,
        IMediaSourceManager mediaSourceManager,
        INetworkManager networkManager)
    {
        _apiClient = apiClient;
        _logger = logger;
        _mediaSourceManager = mediaSourceManager;
        _networkManager = networkManager;
    }

    /// <inheritdoc />
    public string Name => "Zapp";

    /// <inheritdoc />
    public string Type => "zapp";

    /// <inheritdoc />
    public bool IsSupported => true;

    /// <inheritdoc />
    public async Task<List<ChannelInfo>> GetChannels(bool enableCache, CancellationToken cancellationToken)
    {
        var channels = await _apiClient.GetZappChannelsAsync(cancellationToken).ConfigureAwait(false);
        return channels.Select(c => new ChannelInfo
        {
            Name = c.Name,
            Id = c.Id,
            Path = c.StreamUrl,
            TunerHostId = "zapp",
            ChannelType = ChannelType.TV,
            ImageUrl = ZappChannelLogoProvider.GetLogoUrl(c.Id)
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<ILiveStream> GetChannelStream(string channelId, string streamId, IList<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
    {
        var channels = await GetChannels(true, cancellationToken).ConfigureAwait(false);
        var channel = channels.FirstOrDefault(c => string.Equals(c.Id, channelId, StringComparison.OrdinalIgnoreCase));

        if (channel == null)
        {
            throw new ArgumentException("Channel not found", nameof(channelId));
        }

        var mediaSource = CreateMediaSourceInfo(channel);
        return new ZappLiveStream(mediaSource);
    }

    /// <inheritdoc />
    public Task<List<MediaSourceInfo>> GetChannelStreamMediaSources(string channelId, CancellationToken cancellationToken)
    {
        // This is not called if GetChannelStream is implemented correctly and returns the ILiveStream with MediaSource.
        // But for completeness, we can implement it.
        return Task.FromResult(new List<MediaSourceInfo>());
    }

    /// <inheritdoc />
    public Task Validate(TunerHostInfo info)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<TunerHostInfo>> DiscoverDevices(int discoveryDurationMs, CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<TunerHostInfo>());
    }

    private MediaSourceInfo CreateMediaSourceInfo(ChannelInfo channel)
    {
        var path = channel.Path;
        var protocol = _mediaSourceManager.GetPathProtocol(path);

        var isRemote = true;
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            isRemote = !_networkManager.IsInLocalNetwork(uri.Host);
        }

        var httpHeaders = new Dictionary<string, string>();

        if (protocol == MediaProtocol.Http)
        {
            httpHeaders[HeaderNames.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";
        }

        var mediaSource = new MediaSourceInfo
        {
            Path = path,
            Protocol = protocol,
            MediaStreams = new MediaStream[]
            {
                new MediaStream
                {
                    Type = MediaStreamType.Video,
                    Index = -1,
                    IsInterlaced = false
                },
                new MediaStream
                {
                    Type = MediaStreamType.Audio,
                    Index = -1
                }
            },
            RequiresOpening = true,
            RequiresClosing = true,
            Id = channel.Path.GetMD5().ToString("N", CultureInfo.InvariantCulture),
            IsInfiniteStream = true,
            IsRemote = isRemote,
            SupportsDirectPlay = true,
            SupportsDirectStream = true,
            RequiredHttpHeaders = httpHeaders
        };

        mediaSource.InferTotalBitrate();

        return mediaSource;
    }

    private sealed class ZappLiveStream : ILiveStream
    {
        public ZappLiveStream(MediaSourceInfo mediaSource)
        {
            MediaSource = mediaSource;
            UniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            ConsumerCount = 1;
            OriginalStreamId = string.Empty;
        }

        public int ConsumerCount { get; set; }

        public string OriginalStreamId { get; set; }

        public string TunerHostId => "zapp";

        public bool EnableStreamSharing => false;

        public MediaSourceInfo MediaSource { get; set; }

        public string UniqueId { get; }

        public Task Open(CancellationToken openCancellationToken) => Task.CompletedTask;

        public Task Close() => Task.CompletedTask;

        public System.IO.Stream GetStream() => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
