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
    /// Gets or sets the download history entries.
    /// </summary>
    public DbSet<DownloadHistoryEntry> DownloadHistory { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media items.
    /// </summary>
    public DbSet<MediaItem> MediaItems { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media urls.
    /// </summary>
    public DbSet<MediaUrl> MediaUrls { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media item urls.
    /// </summary>
    public DbSet<MediaItemUrl> MediaItemUrls { get; set; } = null!;

    /// <summary>
    /// Gets or sets the download entries.
    /// </summary>
    public DbSet<DownloadEntry> DownloadEntries { get; set; } = null!;

    /// <summary>
    /// Gets or sets the migration history.
    /// </summary>
    public DbSet<MigrationHistory> MigrationHistory { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaItemUrl>()
            .HasKey(miu => new { miu.MediaItemId, miu.MediaUrlId });

        modelBuilder.Entity<MediaItemUrl>()
            .HasOne(miu => miu.MediaItem)
            .WithMany(mi => mi.MediaItemUrls)
            .HasForeignKey(miu => miu.MediaItemId);

        modelBuilder.Entity<MediaItemUrl>()
            .HasOne(miu => miu.MediaUrl)
            .WithMany(mu => mu.MediaItemUrls)
            .HasForeignKey(miu => miu.MediaUrlId);

        modelBuilder.Entity<DownloadEntry>()
            .HasOne(de => de.MediaItem)
            .WithMany()
            .HasForeignKey(de => de.MediaItemId);
    }
}
