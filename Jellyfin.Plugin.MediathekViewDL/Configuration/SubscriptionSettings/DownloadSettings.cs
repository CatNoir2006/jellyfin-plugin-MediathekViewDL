namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Settings for the download process within a subscription.
/// </summary>
public record DownloadSettings : BaseDownloadSettings
{
    /// <summary>
    /// Gets the specific download path for this subscription. Overrides the default path if set.
    /// </summary>
    public string DownloadPath { get; init; } = string.Empty;
}
