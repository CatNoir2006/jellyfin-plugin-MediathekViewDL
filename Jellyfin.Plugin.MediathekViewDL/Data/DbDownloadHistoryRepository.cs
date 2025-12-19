using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Database implementation of the download history repository.
/// </summary>
public class DbDownloadHistoryRepository : IDownloadHistoryRepository
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbDownloadHistoryRepository"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    public DbDownloadHistoryRepository(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async Task AddAsync(string videoUrl, string itemId, Guid subscriptionId, string downloadPath, string title, string? language)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        var entry = new DownloadHistoryEntry
        {
            ItemId = itemId,
            VideoUrl = videoUrl,
            VideoUrlHash = HashUrl(videoUrl),
            SubscriptionId = subscriptionId,
            Timestamp = DateTimeOffset.UtcNow,
            DownloadPath = downloadPath,
            Title = title,
            Language = language ?? "deu"
        };
        context.DownloadHistory.Add(entry);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUrlAsync(string videoUrl)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        var hash = HashUrl(videoUrl);
        return await context.DownloadHistory
            .AsNoTracking()
            .AnyAsync(e => e.VideoUrlHash == hash)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByItemIdAsync(string itemId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            .AnyAsync(e => e.ItemId == itemId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUrlAndSubscriptionIdAsync(string videoUrl, Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        var hash = HashUrl(videoUrl);
        return await context.DownloadHistory
            .AsNoTracking()
            .AnyAsync(e => e.VideoUrlHash == hash && e.SubscriptionId == subscriptionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByItemIdAndSubscriptionIdAsync(string itemId, Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            .AnyAsync(e => e.ItemId == itemId && e.SubscriptionId == subscriptionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByHashAsync(string videoUrlHash)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            .AnyAsync(e => e.VideoUrlHash == videoUrlHash)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DownloadHistoryEntry?> GetByVideoUrlAsync(string videoUrl)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        var hash = HashUrl(videoUrl);
        return await context.DownloadHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.VideoUrlHash == hash)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DownloadHistoryEntry?> GetByItemIdAndSubscriptionIdAsync(string itemId, Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ItemId == itemId && e.SubscriptionId == subscriptionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DownloadHistoryEntry?> GetByItemIdAsync(string itemId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ItemId == itemId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DownloadHistoryEntry?> GetByUrlAndSubscriptionIdAsync(string videoUrl, Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        var hash = HashUrl(videoUrl);
        return await context.DownloadHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.VideoUrlHash == hash && e.SubscriptionId == subscriptionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DownloadHistoryEntry>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            .Where(e => e.SubscriptionId == subscriptionId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveBySubscriptionIdAsync(Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        // ExecuteDeleteAsync is more efficient in EF Core 7+ (available in .NET 9)
        await context.DownloadHistory
            .Where(e => e.SubscriptionId == subscriptionId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DownloadHistoryEntry>> GetRecentHistoryAsync(int limit)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        return await context.DownloadHistory
            .AsNoTracking()
            // SQLite has issues sorting by DateTimeOffset.
            // Since Timestamp is set to UtcNow on insertion and Id is auto-incrementing,
            // sorting by Id yields the same chronological order.
            .OrderByDescending(e => e.Id)
            .Take(limit)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    private static string HashUrl(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToBase64String(bytes);
    }
}
