using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Channels;

/// <summary>
/// Exposes the items of virtual subscriptions as a Jellyfin channel. Items are streamed on demand
/// directly from the Mediathek URL without downloading files or creating STRMs.
/// </summary>
public class MediathekChannel : IChannel, IRequiresMediaInfoCallback
{
    private const string FolderPrefix = "vsub:";
    private const string ItemPrefix = "vitem:";

    private readonly ILogger<MediathekChannel> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ISubscriptionProcessor _subscriptionProcessor;

    // Cache of channel item id -> the API result used to resolve the stream URL on playback.
    private readonly ConcurrentDictionary<string, ApiResultCacheEntry> _itemCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekChannel"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    public MediathekChannel(
        ILogger<MediathekChannel> logger,
        IConfigurationProvider configurationProvider,
        ISubscriptionProcessor subscriptionProcessor)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _subscriptionProcessor = subscriptionProcessor;
    }

    /// <inheritdoc />
    public string Name => "Mediathek (Virtual)";

    /// <inheritdoc />
    public string Description => "Sendungen der virtuellen Abos – on demand direkt aus der Mediathek streamen, ohne Download.";

    /// <inheritdoc />
    public string DataVersion
    {
        get
        {
            var config = _configurationProvider.ConfigurationOrNull;
            if (config == null)
            {
                return "1.0";
            }

            var ids = string.Join(
                ",",
                config.Subscriptions
                    .Where(s => s.IsEnabled && s.IsVirtual)
                    .Select(s => s.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture)));

            return "v1:" + ids;
        }
    }

    /// <inheritdoc />
    public string HomePageUrl => "https://github.com/CatNoir2006/jellyfin-plugin-MediathekViewDL";

    /// <inheritdoc />
    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    /// <inheritdoc />
    public bool IsEnabledFor(string userId) => true;

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedChannelImages() => [];

    /// <inheritdoc />
    public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
    {
        return Task.FromResult(new DynamicImageResponse { HasImage = false });
    }

    /// <inheritdoc />
    public InternalChannelFeatures GetChannelFeatures()
    {
        return new InternalChannelFeatures
        {
            MediaTypes = new List<ChannelMediaType> { ChannelMediaType.Video },
            ContentTypes = new List<ChannelMediaContentType> { ChannelMediaContentType.Clip, ChannelMediaContentType.Episode },
            DefaultSortFields = new List<ChannelItemSortField> { ChannelItemSortField.PremiereDate, ChannelItemSortField.Name },
            SupportsSortOrderToggle = true,
            SupportsContentDownloading = false,
        };
    }

    /// <inheritdoc />
    public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return new ChannelItemResult();
        }

        var virtualSubscriptions = config.Subscriptions
            .Where(s => s.IsEnabled && s.IsVirtual)
            .ToList();

        if (string.IsNullOrEmpty(query.FolderId))
        {
            return BuildFolderListing(virtualSubscriptions);
        }

        if (query.FolderId.StartsWith(FolderPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var subscriptionId = query.FolderId.Substring(FolderPrefix.Length);
            var subscription = virtualSubscriptions.FirstOrDefault(s => s.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture) == subscriptionId);
            if (subscription == null)
            {
                return new ChannelItemResult();
            }

            return await BuildItemListingAsync(subscription, cancellationToken).ConfigureAwait(false);
        }

        return new ChannelItemResult();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
    {
        if (!_itemCache.TryGetValue(id, out var entry))
        {
            _logger.LogWarning("No cached item found for channel item '{Id}'.", id);
            return [];
        }

        var url = await _subscriptionProcessor.GetStreamUrlAsync(entry.Subscription, entry.Item, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Could not resolve a stream URL for channel item '{Id}'.", id);
            return [];
        }

        var mediaSource = new MediaSourceInfo
        {
            Protocol = MediaProtocol.Http,
            Id = CreateStableGuid(entry.Item.Id).ToString("N", System.Globalization.CultureInfo.InvariantCulture),
            Path = url,
            IsRemote = true,
            Name = entry.Item.Title,
            RunTimeTicks = entry.Item.Duration.Ticks,
            SupportsTranscoding = true,
            SupportsDirectStream = true,
            SupportsDirectPlay = true,
        };

        return new List<MediaSourceInfo> { mediaSource };
    }

    private ChannelItemResult BuildFolderListing(IReadOnlyCollection<Subscription> virtualSubscriptions)
    {
        var items = virtualSubscriptions.Select(subscription => new ChannelItemInfo
        {
            Id = FolderPrefix + subscription.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture),
            Name = subscription.Name,
            Type = ChannelItemType.Folder,
            FolderType = ChannelFolderType.Container,
            MediaType = ChannelMediaType.Video,
            DateCreated = subscription.LastDownloadedTimestamp,
        }).ToList();

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count,
        };
    }

    private async Task<ChannelItemResult> BuildItemListingAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var items = new List<ChannelItemInfo>();

        await foreach (var (item, videoInfo) in _subscriptionProcessor.GetChannelItemsAsync(subscription, cancellationToken).ConfigureAwait(false))
        {
            var itemId = ItemPrefix + subscription.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + "-" + item.Id;
            _itemCache[itemId] = new ApiResultCacheEntry(subscription, item);

            var isEpisode = videoInfo.IsShow;
            var channelItem = new ChannelItemInfo
            {
                Id = itemId,
                Name = videoInfo.Title,
                Overview = item.Description,
                Type = ChannelItemType.Media,
                MediaType = ChannelMediaType.Video,
                ContentType = isEpisode ? ChannelMediaContentType.Episode : ChannelMediaContentType.Clip,
                PremiereDate = item.Timestamp.DateTime,
                ProductionYear = item.Timestamp.Year,
                RunTimeTicks = item.Duration.Ticks,
                DateCreated = item.Timestamp.DateTime,
                IndexNumber = videoInfo.EpisodeNumber,
                ParentIndexNumber = videoInfo.SeasonNumber,
            };

            if (!string.IsNullOrWhiteSpace(item.Channel))
            {
                channelItem.Studios = new List<string> { item.Channel };
            }

            if (!string.IsNullOrWhiteSpace(item.Topic))
            {
                channelItem.SeriesName = item.Topic;
            }

            items.Add(channelItem);
        }

        return new ChannelItemResult
        {
            Items = items,
            TotalRecordCount = items.Count,
        };
    }

    /// <summary>
    /// Creates a stable <see cref="Guid"/> from an arbitrary string.
    /// Jellyfin's streaming pipeline Guid.Parse's the MediaSourceId (e.g. for trickplay),
    /// so channel media sources must expose a valid Guid instead of the raw external id.
    /// </summary>
    /// <param name="value">The external identifier to hash.</param>
    /// <returns>A deterministic Guid derived from <paramref name="value"/>.</returns>
    private static Guid CreateStableGuid(string value)
    {
        // Only the first 16 bytes are required to construct a Guid; SHA256 avoids the
        // deprecated MD5 algorithm while remaining deterministic across restarts.
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private readonly record struct ApiResultCacheEntry(Subscription Subscription, Api.Models.ResultItemDto Item);
}
