namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Options for network and security.
/// </summary>
public record NetworkOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether downloads from unknown domains are allowed.
    /// This may be usefull if ARD or ZDF adds new CDNs that are not yet whitelisted.
    /// This may pose a security risk, so use with caution.
    /// </summary>
    public bool AllowUnknownDomains { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether http is allowed for download URLs.
    /// This may be necessary as some URLs do not support https for some reason.
    /// I recommend keeping this off and only turning it on if you encounter problems.
    /// </summary>
    public bool AllowHttp { get; set; }
}
