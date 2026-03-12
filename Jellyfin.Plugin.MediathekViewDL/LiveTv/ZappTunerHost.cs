using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using MediaBrowser.Common.Configuration;
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
    private readonly IServerConfigurationManager _serverConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZappTunerHost"/> class.
    /// </summary>
    /// <param name="apiClient">The api client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="mediaSourceManager">The media source manager.</param>
    /// <param name="networkManager">The network manager.</param>
    /// <param name="serverConfig">The server configuration manager.</param>
    public ZappTunerHost(
        IMediathekViewApiClient apiClient,
        ILogger<ZappTunerHost> logger,
        IMediaSourceManager mediaSourceManager,
        INetworkManager networkManager,
        IServerConfigurationManager serverConfig)
    {
        _apiClient = apiClient;
        _logger = logger;
        _mediaSourceManager = mediaSourceManager;
        _networkManager = networkManager;
        _serverConfig = serverConfig;
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
        var tunerHostInfo = GetTunerHostInfo();
        if (tunerHostInfo is null)
        {
            _logger.LogWarning("Zapp tuner host is not configured. Please add a tuner host with type 'zapp' in the Live TV settings.");
            return [];
        }

        var channels = await _apiClient.GetZappChannelsAsync(cancellationToken).ConfigureAwait(false);
        return channels.Select(c => new ChannelInfo
        {
            Name = c.Name,
            Id = GetExtChannelId(c.Id),
            Path = c.StreamUrl,
            TunerHostId = tunerHostInfo.Id,
            ChannelType = ChannelType.TV,
            ImageUrl = ZappChannelLogoProvider.GetLogoUrl(c.Id)
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<ILiveStream> GetChannelStream(string channelId, string streamId, IList<ILiveStream> currentLiveStreams, CancellationToken cancellationToken)
    {
        // We throw FileNotFoundException here because Jellyfin's DefaultLiveTvService.cs
        // explicitly catches only FileNotFoundException (and OperationCanceledException)
        // to determine if it should try the next available tuner host.
        // Throwing ArgumentException or others would stop the search and lead to a playback error.
        if (!IsExtChannelId(channelId))
        {
            throw new System.IO.FileNotFoundException("Channel not found");
        }

        var channels = await GetChannels(true, cancellationToken).ConfigureAwait(false);
        var channel = channels.FirstOrDefault(c => string.Equals(c.Id, channelId, StringComparison.OrdinalIgnoreCase));

        if (channel == null)
        {
            throw new System.IO.FileNotFoundException("Channel not found");
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
            MediaStreams = Array.Empty<MediaStream>(),
            RequiresOpening = true,
            RequiresClosing = true,
            Id = channel.Path.GetMD5().ToString("N", CultureInfo.InvariantCulture),
            IsInfiniteStream = true,
            IsRemote = isRemote,
            SupportsDirectPlay = true,
            SupportsDirectStream = true,
            SupportsProbing = true,
            AnalyzeDurationMs = 3000,
            RequiredHttpHeaders = httpHeaders,
        };

        mediaSource.InferTotalBitrate();

        return mediaSource;
    }

    private TunerHostInfo? GetTunerHostInfo()
    {
        var liveTvConfig = _serverConfig.GetConfiguration<LiveTvOptions>("livetv");
        return liveTvConfig.TunerHosts.FirstOrDefault(t => t.Type == "zapp");
    }

    private string GetExtChannelId(string channelId)
    {
        return Constants.ChannelIdPrefix + channelId;
    }

    private string GetInternalChannelId(string extChannelId)
    {
        return IsExtChannelId(extChannelId) ? extChannelId[Constants.ChannelIdPrefix.Length..] : extChannelId;
    }

    private static bool IsExtChannelId(string channelId)
    {
        return channelId.StartsWith(Constants.ChannelIdPrefix, StringComparison.OrdinalIgnoreCase);
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
