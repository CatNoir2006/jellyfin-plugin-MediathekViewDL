using System;
using System.Linq;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.LiveTv;

namespace Jellyfin.Plugin.MediathekViewDL.LiveTv;

/// <summary>
/// Utility methods for Live TV.
/// </summary>
public static class LiveTvUtils
{
    /// <summary>
    /// Gets the external channel id.
    /// </summary>
    /// <param name="channelId">The internal channel id.</param>
    /// <returns>The external channel id.</returns>
    public static string GetExtChannelId(string channelId)
        => Constants.ChannelIdPrefix + channelId;

    /// <summary>
    /// Gets the internal channel id from an external channel id.
    /// </summary>
    /// <param name="extChannelId">The external channel id.</param>
    /// <returns>The internal channel id.</returns>
    public static string GetInternalChannelId(string extChannelId)
        => IsExtChannelId(extChannelId) ? extChannelId[Constants.ChannelIdPrefix.Length..] : extChannelId;

    /// <summary>
    /// Determines whether the specified channel id is an external channel id.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <returns><c>true</c> if the channel id is an external channel id; otherwise, <c>false</c>.</returns>
    public static bool IsExtChannelId(string channelId)
        => channelId.StartsWith(Constants.ChannelIdPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the tuner host info for the zapp tuner.
    /// </summary>
    /// <param name="serverConfig">The server configuration manager.</param>
    /// <returns>The tuner host info, or null if not found.</returns>
    public static TunerHostInfo? GetTunerHostInfo(IServerConfigurationManager serverConfig)
    {
        var liveTvConfig = serverConfig.GetConfiguration<LiveTvOptions>("livetv");
        return liveTvConfig.TunerHosts.FirstOrDefault(t => t.Type == "zapp");
    }
}
