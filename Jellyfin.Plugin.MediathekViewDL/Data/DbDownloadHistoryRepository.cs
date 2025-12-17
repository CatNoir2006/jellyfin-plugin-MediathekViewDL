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
    private readonly DatabaseMigrator _migrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbDownloadHistoryRepository"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="migrator">The database migrator.</param>
    public DbDownloadHistoryRepository(IServiceScopeFactory scopeFactory, DatabaseMigrator migrator)
    {
        _scopeFactory = scopeFactory;
        _migrator = migrator;
    }

    /// <inheritdoc />
    public async Task AddAsync(string videoUrl, string itemId, Guid subscriptionId, string downloadPath)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        DownloadHistoryEntry entry = new DownloadHistoryEntry
        {
            VideoUrlHash = HashUrl(videoUrl),
            VideoUrl = videoUrl,
            ItemId = itemId,
            SubscriptionId = subscriptionId,
            Timestamp = DateTimeOffset.UtcNow,
            DownloadPath = downloadPath,
        };
        context.DownloadHistory.Add(entry);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUrlAsync(string videoUrl)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        var hash = HashUrl(videoUrl);
        return await context.DownloadHistory
            .AsNoTracking()
            .AnyAsync(e => e.VideoUrlHash == hash && e.SubscriptionId == subscriptionId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByHashAsync(string videoUrlHash)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

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

        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        // ExecuteDeleteAsync is more efficient in EF Core 7+ (available in .NET 9)
        await context.DownloadHistory
            .Where(e => e.SubscriptionId == subscriptionId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    private static string HashUrl(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToBase64String(bytes);
    }
}
