using System;
using System.IO;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// A stream wrapper that throttles the reading speed using a TokenBucketRateLimiter.
/// </summary>
public class ThrottledStream : Stream
{
    private readonly Stream _innerStream;
    private readonly TokenBucketRateLimiter _limiter;
    private readonly int _tokenLimit;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottledStream"/> class.
    /// </summary>
    /// <param name="innerStream">The source stream to wrap.</param>
    /// <param name="bytesPerSecond">The maximum bytes per second to allow.</param>
    public ThrottledStream(Stream innerStream, long bytesPerSecond)
    {
        if (bytesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesPerSecond), "Bandwidth limit must be greater than zero.");
        }

        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        _tokenLimit = (int)bytesPerSecond;

        var options = new TokenBucketRateLimiterOptions
        {
            TokenLimit = _tokenLimit, // Burst size equal to one second of data
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = int.MaxValue, // Allow infinite queuing so AcquireAsync waits instead of failing
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = _tokenLimit,
            AutoReplenishment = true
        };

        _limiter = new TokenBucketRateLimiter(options);
    }

    /// <inheritdoc />
    public override bool CanRead => _innerStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _innerStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => _innerStream.CanWrite;

    /// <inheritdoc />
    public override long Length => _innerStream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    /// <inheritdoc />
    public override void Flush() => _innerStream.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        // Synchronous read is also throttled but blocking
        _limiter.AcquireAsync(Math.Min(count, _tokenLimit)).AsTask().Wait();
        return _innerStream.Read(buffer, offset, count);
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // Request tokens for the amount we want to read.
        // If count is larger than the bucket size, we might need to loop or just take what we can.
        // Simple approach: The limiter will wait until tokens are available.
        // However, TokenBucketRateLimiter will reject requests larger than TokenLimit.
        // So we must clamp the request to the TokenLimit (which we set to bytesPerSecond).
        int bytesToRead = Math.Min(count, _tokenLimit);

        using var lease = await _limiter.AcquireAsync(bytesToRead, cancellationToken).ConfigureAwait(false);

        if (lease.IsAcquired)
        {
#pragma warning disable CA1835 // Prefer Memory<T> overloads
            // We consciously call the byte[] overload of the inner stream here to match the signature
            // If the inner stream is optimized for byte[], this is better.
            // However, CA1835 wants us to use Memory.
            // Let's use the Memory overload if available on inner stream?
            // Actually, Stream.ReadAsync(byte[]) base implementation calls ReadAsync(Memory).
            // But since we are overriding it, we can just forward it properly.
            return await _innerStream.ReadAsync(buffer, offset, bytesToRead, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA1835
        }

        // Should not happen with QueueLimit = 0 and infinite wait, unless cancelled
        throw new OperationCanceledException("Rate limiter acquisition failed.");
    }

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int bytesToRead = Math.Min(buffer.Length, _tokenLimit);

        using var lease = await _limiter.AcquireAsync(bytesToRead, cancellationToken).ConfigureAwait(false);

        if (lease.IsAcquired)
        {
            return await _innerStream.ReadAsync(buffer.Slice(0, bytesToRead), cancellationToken).ConfigureAwait(false);
        }

        throw new OperationCanceledException("Rate limiter acquisition failed.");
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => _innerStream.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerStream.Dispose();
            _limiter.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await _innerStream.DisposeAsync().ConfigureAwait(false);
        _limiter.Dispose();
        await base.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
