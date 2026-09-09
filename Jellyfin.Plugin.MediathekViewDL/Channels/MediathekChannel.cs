using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using MediaBrowser.Model.Dlna;
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
    private const int MaxCacheEntries = 512;

    // The channel is a singleton, so the cache needs an entry lifetime and a hard cap to
    // avoid unbounded growth and stale playback entries for removed subscriptions/items.
    private static readonly TimeSpan CacheEntryLifetime = TimeSpan.FromMinutes(30);

    private readonly ILogger<MediathekChannel> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly ISubscriptionProcessor _subscriptionProcessor;
    private readonly IHttpClientFactory _httpClientFactory;

    // Cache of channel item id -> the API result used to resolve the stream URL on playback.
    private readonly ConcurrentDictionary<string, ApiResultCacheEntry> _itemCache = new(StringComparer.OrdinalIgnoreCase);

    // The state (DataVersion) the cache was built from; a mismatch invalidates all entries.
    private string? _cacheStateKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekChannel"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="subscriptionProcessor">The subscription processor.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public MediathekChannel(
        ILogger<MediathekChannel> logger,
        IConfigurationProvider configurationProvider,
        ISubscriptionProcessor subscriptionProcessor,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _subscriptionProcessor = subscriptionProcessor;
        _httpClientFactory = httpClientFactory;
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
                    .Select(s => s.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture))
                    .OrderBy(static id => id, StringComparer.OrdinalIgnoreCase));

            // Jellyfin caches channel listings while DataVersion stays constant. Bucketing by
            // day forces a daily refresh so newly published items appear without config changes.
            // v3: stable GUID for item part of channel id (was raw Mediathek id with '+/=')
            return $"v3:{DateTime.UtcNow:yyyyMMdd}:{ids}";
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

        InvalidateStaleCache();

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
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("No channel item id provided.");
            return [];
        }

        // Jellyfin may pass the id with surrounding quotes or URL-encoding when the original
        // Mediathek id contains '+','/','=' . Normalize before lookup and try stable-guid fallback.
        var normalizedId = id.Trim().Trim('"', '\'');
        if (!string.IsNullOrWhiteSpace(normalizedId) && normalizedId != id)
        {
            _logger.LogDebug("Normalized channel item id from '{Original}' to '{Normalized}'.", id, normalizedId);
            id = normalizedId;
        }

        if (!_itemCache.TryGetValue(id, out var entry) || IsExpired(entry))
        {
            // Fallback: the vitem id suffix may be a stable GUID (new format) while the cache was built
            // with the old raw-id format, or vice-versa. Try to find by stable guid or raw suffix.
            ApiResultCacheEntry? fallback = null;
            foreach (var kv in _itemCache)
            {
                if (IsExpired(kv.Value))
                {
                    continue;
                }

                var rawSuffix = kv.Key.Contains('-', StringComparison.Ordinal) ? kv.Key.Substring(kv.Key.LastIndexOf('-') + 1) : kv.Key;
                var guidSuffix = CreateStableGuid(kv.Value.Item.Id).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
                var requestedSuffix = id.Contains('-', StringComparison.Ordinal) ? id.Substring(id.LastIndexOf('-') + 1) : id;

                if (string.Equals(rawSuffix, requestedSuffix, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(guidSuffix, requestedSuffix, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(kv.Key, id, StringComparison.OrdinalIgnoreCase))
                {
                    fallback = kv.Value;
                    break;
                }
            }

            if (fallback.HasValue)
            {
                entry = fallback.Value;
            }
            else
            {
                // Cache may be empty after a server restart. Try to re-resolve the item by
                // querying the virtual subscriptions for the matching stable GUID / raw id.
                var recovered = await TryRecoverEntryAsync(id, cancellationToken).ConfigureAwait(false);
                if (recovered.HasValue)
                {
                    entry = recovered.Value;
                    // Repopulate cache for next time
                    _itemCache[id] = entry;
                    _logger.LogInformation("Recovered channel item '{Id}' via re-query (cache was empty).", id);
                }
                else
                {
                    _logger.LogWarning("No (longer) cached item found for channel item '{Id}'. Cache contains {Count} entries.", id, _itemCache.Count);
                    return [];
                }
            }
        }

        var url = await _subscriptionProcessor.GetStreamUrlAsync(entry.Subscription, entry.Item, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Could not resolve a stream URL for channel item '{Id}'.", id);
            return [];
        }

        // Domain check – mirror the download-path validation so the channel never streams
        // from an untrusted origin.
        if (!IsDomainAllowed(url))
        {
            _logger.LogWarning(
                "Stream URL for channel item '{Id}' uses a non-allowed domain; refusing to stream. URL: {Url}",
                id,
                url);
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
            MediaStreams = new List<MediaStream>(),
            DefaultSubtitleStreamIndex = -1,
        };

        // Attach subtitles – only WEBVTT is exposed (TTML not supported by Jellyfin).
        // External HTTP VTT via SubtitleEncoder fails with "Cannot access a closed Stream"
        // when using IsExternalUrl=true directly. Workaround: download VTT to a local
        // temp file and expose as IsExternalUrl=false (File) so the server can serve it
        // without HttpClient charset fetch.
        var subtitleStreams = new List<MediaStream>();
        var subtitleIndex = 0;
        foreach (var sub in entry.Item.SubtitleUrls)
        {
            if (string.IsNullOrWhiteSpace(sub.Url))
            {
                continue;
            }

            if (sub.Type != Api.Models.SubtitleType.WEBVTT)
            {
                continue;
            }

            var localPath = await GetOrDownloadSubtitleAsync(sub.Url, entry.Item.Id, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(localPath))
            {
                // Fallback to external URL if download fails – client may still fetch directly
                subtitleStreams.Add(new MediaStream
                {
                    Type = MediaStreamType.Subtitle,
                    Index = subtitleIndex++,
                    IsExternal = true,
                    IsExternalUrl = true,
                    IsDefault = false,
                    IsForced = false,
                    Path = sub.Url,
                    DeliveryUrl = sub.Url,
                    Codec = "webvtt",
                    Language = "de",
                    Title = "Deutsch",
                    SupportsExternalStream = true,
                    DeliveryMethod = SubtitleDeliveryMethod.External,
                });
                continue;
            }

            subtitleStreams.Add(new MediaStream
            {
                Type = MediaStreamType.Subtitle,
                Index = subtitleIndex++,
                IsExternal = true,
                IsExternalUrl = false,
                IsDefault = false,
                IsForced = false,
                Path = localPath,
                Codec = "webvtt",
                Language = "de",
                Title = "Deutsch",
                SupportsExternalStream = true,
            });
        }

        mediaSource.MediaStreams = subtitleStreams;

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
            // Use stable GUID for the item part to avoid '+'/'/'/'=' in the Mediathek id breaking channel ids / URLs.
            var itemId = ItemPrefix + subscription.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture) + "-" + CreateStableGuid(item.Id).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
            _itemCache[itemId] = new ApiResultCacheEntry(subscription, item, DateTimeOffset.UtcNow);

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
    /// Drops expired cache entries and invalidates the whole cache when the set of enabled
    /// virtual subscriptions (or the daily refresh bucket) changed. Called before listings are
    /// rebuilt so removed subscriptions/items can no longer be resolved for playback.
    /// </summary>
    private void InvalidateStaleCache()
    {
        var stateKey = DataVersion;
        if (_cacheStateKey != stateKey)
        {
            _itemCache.Clear();
            _cacheStateKey = stateKey;
            return;
        }

        if (_itemCache.IsEmpty)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var (key, entry) in _itemCache)
        {
            if (IsExpired(entry, now))
            {
                _itemCache.TryRemove(key, out _);
            }
        }

        if (_itemCache.Count > MaxCacheEntries)
        {
            _logger.LogDebug("Channel item cache exceeded {MaxCacheEntries} entries; clearing.", MaxCacheEntries);
            _itemCache.Clear();
        }
    }

    private bool IsExpired(in ApiResultCacheEntry entry) => IsExpired(entry, DateTimeOffset.UtcNow);

    private static bool IsExpired(in ApiResultCacheEntry entry, DateTimeOffset now) => now - entry.CachedAt > CacheEntryLifetime;

    /// <summary>
    /// Checks whether a URL's domain is in the allowed list or unknown domains are permitted.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns><c>true</c> if the URL is allowed; otherwise <c>false</c>.</returns>
    private bool IsDomainAllowed(string url)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return true;
        }

        if (config.Network.AllowUnknownDomains)
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var hostParts = uri.Host.Split('.');
        if (hostParts.Length < 2)
        {
            return false;
        }

        var topDomain = string.Join('.', hostParts[^2..]);
        return config.AllowedDomains.Contains(topDomain);
    }

    private async Task<string?> GetOrDownloadSubtitleAsync(string url, string itemId, CancellationToken cancellationToken)
    {
        try
        {
            var cacheDir = Path.Combine(Path.GetTempPath(), "jellyfin-mediathek-subtitles");
            Directory.CreateDirectory(cacheDir);
            var fileName = CreateStableGuid(url).ToString("N", System.Globalization.CultureInfo.InvariantCulture) + ".vtt";
            var filePath = Path.Combine(cacheDir, fileName);

            // Use cached file if it exists and is recent (1 day)
            if (File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                if (DateTime.UtcNow - info.LastWriteTimeUtc < TimeSpan.FromDays(1) && info.Length > 0)
                {
                    return filePath;
                }
            }

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using (httpStream.ConfigureAwait(false))
            {
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await httpStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to download subtitle from '{Url}' for item '{ItemId}'.", url, itemId);
            return null;
        }
    }

    private async Task<ApiResultCacheEntry?> TryRecoverEntryAsync(string id, CancellationToken cancellationToken)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            return null;
        }

        // id is vitem:<subId>-<suffix> where suffix is stable GUID (new) or raw Mediathek id (old)
        if (!id.StartsWith(ItemPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var remainder = id.Substring(ItemPrefix.Length);
        var dashIndex = remainder.IndexOf('-', StringComparison.Ordinal);
        if (dashIndex <= 0)
        {
            return null;
        }

        var subIdPart = remainder.Substring(0, dashIndex);
        var suffix = remainder.Substring(dashIndex + 1);

        var subscription = config.Subscriptions.FirstOrDefault(s =>
            s.IsEnabled && s.IsVirtual &&
            string.Equals(s.Id.ToString("N", System.Globalization.CultureInfo.InvariantCulture), subIdPart, StringComparison.OrdinalIgnoreCase));
        if (subscription == null)
        {
            return null;
        }

        await foreach (var (item, _) in _subscriptionProcessor.GetChannelItemsAsync(subscription, cancellationToken).ConfigureAwait(false))
        {
            var guidSuffix = CreateStableGuid(item.Id).ToString("N", System.Globalization.CultureInfo.InvariantCulture);
            if (string.Equals(item.Id, suffix, StringComparison.Ordinal)
                || string.Equals(guidSuffix, suffix, StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResultCacheEntry(subscription, item, DateTimeOffset.UtcNow);
            }
        }

        return null;
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

    private readonly record struct ApiResultCacheEntry(Subscription Subscription, Api.Models.ResultItemDto Item, DateTimeOffset CachedAt);
}
