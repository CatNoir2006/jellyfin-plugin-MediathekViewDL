using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Represents an active or recently active download in the queue.
/// </summary>
public class ActiveDownload
{
    /// <summary>
    /// Gets or sets the unique identifier for this download instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the subscription ID if this download belongs to one.
    /// </summary>
    public Guid? SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the download job details.
    /// </summary>
    public DownloadJob Job { get; set; } = new();

    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    public DownloadStatus Status { get; set; } = DownloadStatus.Queued;

    /// <summary>
    /// Gets or sets the progress (0-100).
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Gets or sets the error message if the job failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the time when the download was created/queued.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the cancellation token source for this job.
    /// </summary>
    [JsonIgnore]
    public CancellationTokenSource Cts { get; } = new CancellationTokenSource();
}
