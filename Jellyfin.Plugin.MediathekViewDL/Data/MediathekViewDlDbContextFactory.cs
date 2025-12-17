using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Factory for creating the <see cref="MediathekViewDlDbContext"/> at design time.
/// </summary>
public class MediathekViewDlDbContextFactory : IDesignTimeDbContextFactory<MediathekViewDlDbContext>
{
    /// <inheritdoc />
    public MediathekViewDlDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MediathekViewDlDbContext>();
        optionsBuilder.UseSqlite("Data Source=mediathek-dl.db");

        return new MediathekViewDlDbContext(optionsBuilder.Options);
    }
}
