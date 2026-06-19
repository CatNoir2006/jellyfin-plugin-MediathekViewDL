using System;
using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.MediathekViewDL.Data;

/// <summary>
/// Tracks executed migration tasks to ensure idempotency.
/// </summary>
public class MigrationHistory
{
    /// <summary>
    /// Gets or sets the unique name of the migration task.
    /// </summary>
    [Key]
    public string MigrationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution timestamp.
    /// </summary>
    public DateTimeOffset ExecutedAt { get; set; }
}
