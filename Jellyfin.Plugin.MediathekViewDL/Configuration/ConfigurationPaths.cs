namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// The configuration paths for the main plugin configuration.
/// </summary>
public record ConfigurationPaths
{
    /// <summary>
    /// Gets or sets the default path where completed downloads are stored.
    /// Can be overridden by a subscription.
    /// </summary>
    public string DefaultDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for show downloads in subscriptions.
    /// </summary>
    public string DefaultSubscriptionShowPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for movie downloads in subscriptions.
    /// </summary>
    public string DefaultSubscriptionMoviePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for manual show downloads.
    /// </summary>
    public string DefaultManualShowPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default path for manual movie downloads.
    /// </summary>
    public string DefaultManualMoviePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temporary path where files are stored during download.
    /// If empty, the destination path is used directly.
    /// </summary>
    public string TempDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether Paths for movies should contain the 'Topic' of the Movie.
    /// </summary>
    public bool UseTopicForMoviePath { get; set; }

    /// <summary>
    /// Determines whether all path settings are empty or not.
    /// </summary>
    /// <returns>True if its the default.</returns>
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(DefaultDownloadPath) &&
               string.IsNullOrEmpty(DefaultSubscriptionShowPath) &&
               string.IsNullOrEmpty(DefaultSubscriptionMoviePath) &&
               string.IsNullOrEmpty(DefaultManualShowPath) &&
               string.IsNullOrEmpty(DefaultManualMoviePath) &&
               string.IsNullOrEmpty(TempDownloadPath) &&
               !UseTopicForMoviePath;
    }
}
