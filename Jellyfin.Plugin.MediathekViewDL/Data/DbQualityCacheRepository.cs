using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Database implementation of the quality cache repository using Entity Framework Core.
/// </summary>
public class DbQualityCacheRepository : IQualityCacheRepository
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DatabaseMigrator _migrator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbQualityCacheRepository"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="migrator">The database migrator.</param>
    public DbQualityCacheRepository(IServiceScopeFactory scopeFactory, DatabaseMigrator migrator)
    {
        _scopeFactory = scopeFactory;
        _migrator = migrator;
    }

    /// <inheritdoc />
    public async Task<QualityCacheEntry?> GetByUrlAsync(string url)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();

        // Ensure database is created/migrated (lazy initialization)
        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        var hash = HashUrl(url);
        return await context.QualityCacheEntries
            .AsNoTracking() // Read-only operation optimization
            .FirstOrDefaultAsync(e => e.UrlHash == hash)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddOrUpdateAsync(string url, int width, int height, TimeSpan duration, long size)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();
        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        var hash = HashUrl(url);
        var entry = await context.QualityCacheEntries
            .FirstOrDefaultAsync(e => e.UrlHash == hash)
            .ConfigureAwait(false);

        if (entry == null)
        {
            entry = new QualityCacheEntry
            {
                UrlHash = hash,
                Width = width,
                Height = height,
                Size = size,
                Duration = duration,
                LastUpdated = DateTimeOffset.UtcNow
            };
            context.QualityCacheEntries.Add(entry);
        }
        else
        {
            entry.Width = width;
            entry.Height = height;
            entry.Size = size;
            entry.LastUpdated = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveByUrlAsync(string url)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();
        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        var hash = HashUrl(url);
        // ExecuteDeleteAsync is more efficient in EF Core 7+ (available in .NET 9)
        await context.QualityCacheEntries
            .Where(e => e.UrlHash == hash)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveByAgeAsync(TimeSpan maxAge)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediathekViewDlDbContext>();
        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);

        var cutoff = DateTimeOffset.UtcNow - maxAge;
        await context.QualityCacheEntries
            .Where(e => e.LastUpdated < cutoff)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    private static string HashUrl(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Convert.ToBase64String(bytes);
    }
}
