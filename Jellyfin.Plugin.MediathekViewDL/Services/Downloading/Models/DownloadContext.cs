namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Models;

/// <summary>
/// Specifies the context of a download.
/// </summary>
public enum DownloadContext
{
    /// <summary>
    /// The download is part of an automated subscription.
    /// </summary>
    Subscription,

    /// <summary>
    /// The download was triggered manually by the user.
    /// </summary>
    Manual
}
