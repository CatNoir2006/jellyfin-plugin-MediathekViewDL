using System.Collections.Generic;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Represents the result of a download job execution with per-item details.
/// </summary>
public sealed class DownloadJobResult
{
    /// <summary>
    /// Gets a value indicating whether the overall job was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the per-item results.
    /// </summary>
    public IReadOnlyList<DownloadItemResult> ItemResults { get; init; } = [];
}
