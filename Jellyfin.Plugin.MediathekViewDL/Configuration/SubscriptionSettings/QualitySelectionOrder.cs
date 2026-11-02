namespace Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;

/// <summary>
/// Defines the order in which available video qualities are preferred during download.
/// </summary>
public enum QualitySelectionOrder
{
    /// <summary>
    /// Prefer the highest available quality. This is the default.
    /// </summary>
    HighestToLowest = 0,

    /// <summary>
    /// Prefer the lowest available quality.
    /// </summary>
    LowestToHighest = 1
}
