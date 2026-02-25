using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Implementation of the temporary metadata cache.
/// </summary>
public class TempMetadataCache : ITempMetadataCache
{
    private readonly ILogger<TempMetadataCache> _logger;
    private readonly IFFmpegService _ffmpegService;
    private readonly ConcurrentDictionary<string, (LocalMediaInfo Info, DateTime? LastWrite)> _cache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TempMetadataCache"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="ffmpegService">The FFmpeg service.</param>
    public TempMetadataCache(ILogger<TempMetadataCache> logger, IFFmpegService ffmpegService)
    {
        _logger = logger;
        _ffmpegService = ffmpegService;
    }

    /// <inheritdoc />
    public async Task<LocalMediaInfo?> GetMetadataAsync(string urlOrPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(urlOrPath))
        {
            return null;
        }

        try
        {
            DateTime? lastWrite = null;
            bool isLocal = File.Exists(urlOrPath);
            if (isLocal)
            {
                lastWrite = File.GetLastWriteTimeUtc(urlOrPath);
            }

            if (_cache.TryGetValue(urlOrPath, out var entry) && entry.LastWrite == lastWrite)
            {
                _logger.LogTrace("Returning cached metadata for: {Path}", urlOrPath);
                return entry.Info;
            }

            _logger.LogDebug("Probing metadata for: {Path}", urlOrPath);
            var info = await _ffmpegService.GetMediaInfoAsync(urlOrPath, cancellationToken).ConfigureAwait(false);

            if (info != null)
            {
                _cache[urlOrPath] = (info, lastWrite);
            }

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting metadata for: {Path}", urlOrPath);
            return null;
        }
    }
}
