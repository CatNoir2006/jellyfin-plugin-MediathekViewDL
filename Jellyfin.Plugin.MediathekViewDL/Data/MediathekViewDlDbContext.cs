using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Database context for the MediathekViewDL plugin.
/// </summary>
public class MediathekViewDlDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewDlDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public MediathekViewDlDbContext(DbContextOptions<MediathekViewDlDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the quality cache entries.
    /// </summary>
    public DbSet<QualityCacheEntry> QualityCacheEntries { get; set; } = null!;

    /// <summary>
    /// Gets or sets the download history entries.
    /// </summary>
    public DbSet<DownloadHistoryEntry> DownloadHistory { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QualityCacheEntry>(entity =>
        {
            entity.HasIndex(e => e.UrlHash);
        });

        modelBuilder.Entity<DownloadHistoryEntry>(entity =>
        {
            entity.HasIndex(e => e.VideoUrlHash);
            entity.HasIndex(e => e.SubscriptionId);
        });
    }
}
