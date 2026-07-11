using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Represents the result of a single download item execution.
/// </summary>
public sealed class DownloadItemResult
{
    /// <summary>
    /// Gets the destination path of the download item.
    /// </summary>
    public string DestinationPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type of download operation.
    /// </summary>
    public DownloadType JobType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the download was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the error message if the download failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets a value indicating whether the file already existed and was skipped.
    /// </summary>
    [JsonIgnore]
    public bool Skipped { get; init; }
}
