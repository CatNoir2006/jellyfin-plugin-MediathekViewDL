using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Migration task to move data from the legacy DownloadHistoryEntry to the new normalized schema.
/// </summary>
public class DownloadHistoryMigrationTask : IMigrationTask
{
    /// <summary>
    /// Gets the unique name of the migration task.
    /// </summary>
    public string Name => "DownloadHistoryMigration";

    /// <inheritdoc/>
    public async Task ExecuteAsync(MediathekViewDlDbContext context, CancellationToken ct)
    {
        // Fetch all legacy entries.
        // We use a raw query or a direct DbSet cast if possible, but since we are changing schema,
        // we should assume the legacy table still exists in the DB.
        var legacyEntries = await context.Set<DownloadHistoryEntry>()
            .AsNoTracking()
            .ToListAsync(ct).ConfigureAwait(false);

        if (legacyEntries.Count == 0)
        {
            return;
        }

        foreach (var legacy in legacyEntries)
        {
            // 1. Get or Create MediaItem
            var mediaItem = await context.MediaItems
                .FirstOrDefaultAsync(m => m.MvwItemId == legacy.ItemId, ct).ConfigureAwait(false);

            if (mediaItem == null)
            {
                mediaItem = new MediaItem
                {
                    MvwItemId = legacy.ItemId,
                    OriginalTitle = legacy.Title,
                    ParsedTitle = legacy.Title, // Default to title
                    Description = string.Empty, // Not in legacy
                    Timestamp = legacy.Timestamp,
                    Duration = 0, // Not in legacy
                    Language = legacy.Language
                };
                context.MediaItems.Add(mediaItem);
                await context.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            // 2. Get or Create MediaUrl
            var mediaUrl = await context.MediaUrls
                .FirstOrDefaultAsync(u => u.Url == legacy.VideoUrl, ct).ConfigureAwait(false);

            if (mediaUrl == null)
            {
                mediaUrl = new MediaUrl
                {
                    Url = legacy.VideoUrl,
                    Type = UrlType.Video // Defaulting to Video
                };
                context.MediaUrls.Add(mediaUrl);
                await context.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            // 3. Link them in MediaItemUrl if not linked
            var exists = await context.MediaItemUrls
                .AnyAsync(miu => miu.MediaItemId == mediaItem.Id && miu.MediaUrlId == mediaUrl.Id, ct).ConfigureAwait(false);

            if (!exists)
            {
                context.MediaItemUrls.Add(new MediaItemUrl
                {
                    MediaItemId = mediaItem.Id,
                    MediaUrlId = mediaUrl.Id
                });
                await context.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            // 4. Create new DownloadEntry
            var newEntry = new DownloadEntry
            {
                SubscriptionId = legacy.SubscriptionId,
                MediaItemId = mediaItem.Id,
                DownloadPath = legacy.DownloadPath,
                Timestamp = legacy.Timestamp
            };
            context.DownloadEntries.Add(newEntry);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }
}
