using System.Collections.Generic;

namespace Jellyfin.Plugin.MediathekViewDL.LiveTv;

/// <summary>
/// Provides logo URLs for Zapp channels.
/// </summary>
public static class ZappChannelLogoProvider
{
    private const string BaseUrl = "https://raw.githubusercontent.com/mediathekview/MediathekView/master/src/main/resources/icons/sender/";

    private static readonly Dictionary<string, string> _logos = new()
    {
        { "das_erste", "ard.svg" },
        { "zdf", "zdf.svg" },
        { "zdf_neo", "ZDFneo.svg" },
        { "zdf_info", "ZDFinfo.svg" },
        { "arte", "arte.svg" },
        { "dreisat", "3sat.svg" },
        { "kika", "kika.svg" },
        { "phoenix", "phoenix.svg" },
        { "one", "one.svg" },
        { "tagesschau24", "tagesschau24.svg" },
        { "br_nord", "br.svg" },
        { "br_sued", "br.svg" },
        { "hr", "hr.svg" },
        { "mdr_sachsen", "mdr.svg" },
        { "mdr_sachsen_anhalt", "mdr.svg" },
        { "mdr_thueringen", "mdr.svg" },
        { "ndr_hh", "ndr.svg" },
        { "ndr_mv", "ndr.svg" },
        { "ndr_nds", "ndr.svg" },
        { "ndr_sh", "ndr.svg" },
        { "rbb_berlin", "rbb.svg" },
        { "rbb_brandenburg", "rbb.svg" },
        { "rb", "radio-bremen.svg" },
        { "sr", "sr.svg" },
        { "swr_bw", "swr.svg" },
        { "swr_rp", "swr.svg" },
        { "wdr", "wdr.svg" },
        { "ard_alpha", "ard-alpha.svg" },
        { "parlamentsfernsehen_1", "Deutscher_Bundestag.svg" },
        { "parlamentsfernsehen_2", "Deutscher_Bundestag.svg" }
    };

    /// <summary>
    /// For now we disable images as the are not correctly displayed in Jellyfin.
    /// </summary>
    private static bool enableImages = false;

    /// <summary>
    /// Gets the logo URL for a channel.
    /// </summary>
    /// <param name="channelId">The channel identifier.</param>
    /// <returns>The logo URL, or null if not found.</returns>
    public static string? GetLogoUrl(string channelId)
    {
        if (!enableImages)
        {
            return null;
        }

        if (_logos.TryGetValue(channelId, out var filename))
        {
            return BaseUrl + filename;
        }

        return null;
    }
}
