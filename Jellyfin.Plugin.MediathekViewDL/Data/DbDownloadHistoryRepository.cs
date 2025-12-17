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
    public async Task AddAsync(DownloadHistoryEntry entry)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        await context.Database.MigrateAsync().ConfigureAwait(false);

        context.DownloadHistory.Add(entry);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string videoUrl)
    {
        var hash = HashUrl(videoUrl);
        return await ExistsByHashAsync(hash).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByHashAsync(string videoUrlHash)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        await context.Database.MigrateAsync().ConfigureAwait(false);

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

        await context.Database.MigrateAsync().ConfigureAwait(false);

        var hash = HashUrl(videoUrl);
        return await context.DownloadHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.VideoUrlHash == hash)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DownloadHistoryEntry>> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        await context.Database.MigrateAsync().ConfigureAwait(false);

        return await context.DownloadHistory
            .AsNoTracking()
            .Where(e => e.SubscriptionId == subscriptionId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    private static string HashUrl(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToBase64String(bytes);
    }
}
