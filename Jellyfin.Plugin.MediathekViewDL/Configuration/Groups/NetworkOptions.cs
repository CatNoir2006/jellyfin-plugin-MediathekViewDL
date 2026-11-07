using MediathekView.Api.External;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;

/// <summary>
/// Options for network and security.
/// </summary>
public record NetworkOptions
{
    /// <summary>
    /// Gets or sets the base URL of the MediathekViewWeb server.
    /// The <c>/api</c> path is added automatically when building API endpoint URLs.
    /// Can be changed to point at a local test server instance, e.g. http://localhost:8000, to test new backend features.
    /// Leave empty to use the default.
    /// </summary>
    public string ApiBaseUrl { get; set; } = MediathekViewApiClientOptions.DefaultApiBaseUrl;

    /// <summary>
    /// Gets or sets the base URL of the Zapp API server.
    /// The <c>/v1</c> path is added automatically when building Zapp endpoint URLs.
    /// Can be changed to point at a local test server instance, e.g. http://localhost:3000, to test new backend features.
    /// Leave empty to use the default.
    /// </summary>
    public string ZappApiBaseUrl { get; set; } = MediathekViewApiClientOptions.DefaultZappApiBaseUrl;

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

    /// <summary>
    /// Gets or sets the User-Agent header to use for download requests.
    /// </summary>
    public string UserAgent { get; set; } = "MediatekViewDL/1";
}
