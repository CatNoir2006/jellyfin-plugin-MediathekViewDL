using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.MediathekViewDL.Providers;

/// <summary>
/// Registers clickable external web links to the ARD / ZDF Mediathek page in the Jellyfin web and client UI.
/// Appears natively under the item's 'Links' footer section.
/// </summary>
public class MediathekExternalUrlProvider : IExternalUrlProvider
{
    /// <inheritdoc />
    public string Name => "ARD / ZDF Mediathek";

    /// <inheritdoc />
    public IEnumerable<string> GetExternalUrls(BaseItem item)
    {
        if (item is not Movie && item is not Series && item is not Season && item is not Episode)
        {
            return Enumerable.Empty<string>();
        }

        var config = Plugin.Instance?.Configuration;
        if (config == null || !config.EnableExternalUrlProvider)
        {
            return Enumerable.Empty<string>();
        }

        // 1. Direct Web URL from ProviderIds if available
        if (item.ProviderIds != null)
        {
            foreach (var key in new[] { "MediathekWebUrl", "MediathekUrl", "ArdMediathekUrl", "ZdfMediathekUrl" })
            {
                if (item.ProviderIds.TryGetValue(key, out var url) && !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    return new[] { url };
                }
            }
        }

        // 2. Check path for embedded URL (.strm or sidecar)
        if (!string.IsNullOrWhiteSpace(item.Path) && item.Path.EndsWith(".strm", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (System.IO.File.Exists(item.Path))
                {
                    var content = System.IO.File.ReadAllText(item.Path).Trim();
                    if (content.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        content.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if it's an ARD / ZDF CDN stream or mediathek link
                        if (config.AllowedDomains.Any(d => content.Contains(d, StringComparison.OrdinalIgnoreCase)))
                        {
                            return new[] { content };
                        }
                    }
                }
            }
            catch
            {
                // Ignore file read exceptions
            }
        }

        return Enumerable.Empty<string>();
    }
}
