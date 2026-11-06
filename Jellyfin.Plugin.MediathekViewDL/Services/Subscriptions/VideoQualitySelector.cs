using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using MediathekView.Api.Models.ResourceItem;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;

/// <summary>
/// Selects the best matching video streams for download based on the configured quality settings.
/// </summary>
public class VideoQualitySelector : IVideoQualitySelector
{
    private const int QualityIndexLow = 1;
    private const int QualityIndexStandard = 2;
    private const int QualityIndexHd = 3;

    private readonly ILogger<VideoQualitySelector> _logger;
    private readonly IFFmpegService _ffmpegService;

    // In-memory cache for probed heights, valid for the lifetime of this instance
    // (i.e. one scan run / one manual download request).
    private readonly ConcurrentDictionary<string, int?> _probedHeights = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoQualitySelector"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ffmpegService">The FFmpeg service used to probe resolutions.</param>
    public VideoQualitySelector(ILogger<VideoQualitySelector> logger, IFFmpegService ffmpegService)
    {
        _logger = logger;
        _ffmpegService = ffmpegService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VideoUrlDto>> GetOrderedCandidatesAsync(IReadOnlyList<VideoUrlDto> videoUrls, BaseDownloadSettings settings, CancellationToken cancellationToken)
    {
        var candidates = videoUrls
            .Where(v => !string.IsNullOrWhiteSpace(v.Url))
            .DistinctBy(v => v.Url, StringComparer.OrdinalIgnoreCase);

        bool rangeConfigured = settings.MinHeight is not null || settings.MaxHeight is not null;
        var matched = new List<(VideoUrlDto Stream, int? Height)>();
        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var height = await GetEffectiveHeightAsync(candidate, settings, cancellationToken).ConfigureAwait(false);
            if (height is null)
            {
                continue;
            }

            if (rangeConfigured)
            {
                if (settings.MinHeight is int minHeight && height.Value < minHeight)
                {
                    continue;
                }

                if (settings.MaxHeight is int maxHeight && height.Value > maxHeight)
                {
                    continue;
                }
            }

            matched.Add((candidate, height));
        }

        return OrderByHeight(matched, settings.SelectionOrder)
            .Select(entry => entry.Stream)
            .ToList();
    }

    private static IOrderedEnumerable<(VideoUrlDto Stream, int? Height)> OrderByHeight(
        IEnumerable<(VideoUrlDto Stream, int? Height)> entries,
        QualitySelectionOrder order)
    {
        return order == QualitySelectionOrder.HighestToLowest
            ? entries.OrderByDescending(e => e.Height ?? e.Stream.Quality).ThenByDescending(e => e.Stream.Quality)
            : entries.OrderBy(e => e.Height ?? e.Stream.Quality).ThenBy(e => e.Stream.Quality);
    }

    private async Task<int?> GetEffectiveHeightAsync(VideoUrlDto stream, BaseDownloadSettings settings, CancellationToken cancellationToken)
    {
        if (_probedHeights.TryGetValue(stream.Url, out var cached))
        {
            return cached ?? (settings.StrictResolution ? null : EstimateHeight(stream.Quality));
        }

        int? height = null;
        if (settings.SkipVideoQualityProbing)
        {
            // Probing is disabled: trust the quality estimation of the API instead
            // of running ffprobe (and its URL validation) against the stream.
            height = EstimateHeight(stream.Quality);
        }
        else
        {
            try
            {
                var info = await _ffmpegService.GetMediaInfoAsync(stream.Url, cancellationToken).ConfigureAwait(false);
                height = info?.Height;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to probe the resolution of '{Url}'. Using fallback estimate.", stream.Url);
            }
        }

        _probedHeights[stream.Url] = height;

        if (height is not null)
        {
            return height;
        }

        return settings.StrictResolution ? null : EstimateHeight(stream.Quality);
    }

    private static int? EstimateHeight(int quality)
    {
        return quality switch
        {
            QualityIndexHd => 1080,
            QualityIndexStandard => 720,
            QualityIndexLow => 480,
            _ => null
        };
    }
}
