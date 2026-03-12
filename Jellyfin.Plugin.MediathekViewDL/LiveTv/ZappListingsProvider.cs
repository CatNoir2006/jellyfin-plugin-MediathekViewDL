using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;

namespace Jellyfin.Plugin.MediathekViewDL.LiveTv;

/// <summary>
/// Provides listings for Zapp channels.
/// </summary>
public class ZappListingsProvider : IListingsProvider
{
    private readonly IMediathekViewApiClient _apiClient;
    private readonly IServerConfigurationManager _serverConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZappListingsProvider"/> class.
    /// </summary>
    /// <param name="apiClient">The api client.</param>
    /// <param name="serverConfig">The server configuration manager.</param>
    public ZappListingsProvider(IMediathekViewApiClient apiClient, IServerConfigurationManager serverConfig)
    {
        _apiClient = apiClient;
        _serverConfig = serverConfig;
    }

    /// <inheritdoc />
    public string Name => "Zapp";

    /// <inheritdoc />
    public string Type => "zapp";

    /// <inheritdoc />
    public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(ListingsProviderInfo info, string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
    {
        var internalId = LiveTvUtils.GetInternalChannelId(channelId);
        var shows = await _apiClient.GetCurrentZappShowAsync(internalId, cancellationToken).ConfigureAwait(false);

        var programs = shows.Select(show =>
            new ProgramInfo
            {
                Name = show.Title,
                Overview = show.Description,
                StartDate = show.StartTime?.UtcDateTime ?? DateTime.UtcNow,
                EndDate = show.EndTime?.UtcDateTime ?? DateTime.UtcNow.AddHours(1),
                ChannelId = channelId,
                Id = $"{channelId}_{show.StartTime?.ToUnixTimeSeconds()}",
                EpisodeTitle = show.Subtitle
            }).ToList();

        return programs;
    }

    /// <inheritdoc />
    public Task Validate(ListingsProviderInfo info, bool validateLogin, bool validateListings)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<NameIdPair>> GetLineups(ListingsProviderInfo info, string country, string location)
    {
        return Task.FromResult(new List<NameIdPair>
        {
            new NameIdPair { Name = "Zapp", Id = "zapp" }
        });
    }

    /// <inheritdoc />
    public async Task<List<ChannelInfo>> GetChannels(ListingsProviderInfo info, CancellationToken cancellationToken)
    {
        var tunerHostInfo = LiveTvUtils.GetTunerHostInfo(_serverConfig);
        var channels = await _apiClient.GetZappChannelsAsync(cancellationToken).ConfigureAwait(false);
        return channels.Select(c => new ChannelInfo
        {
            Name = c.Name,
            Id = LiveTvUtils.GetExtChannelId(c.Id),
            Path = c.StreamUrl,
            ChannelType = ChannelType.TV,
            TunerHostId = tunerHostInfo?.Id ?? "zapp",
            ImageUrl = ZappChannelLogoProvider.GetLogoUrl(c.Id)
        }).ToList();
    }
}
