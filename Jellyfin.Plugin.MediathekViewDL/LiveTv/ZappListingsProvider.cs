using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.External;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ZappListingsProvider"/> class.
    /// </summary>
    /// <param name="apiClient">The api client.</param>
    public ZappListingsProvider(IMediathekViewApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <inheritdoc />
    public string Name => "Zapp";

    /// <inheritdoc />
    public string Type => "zapp";

    /// <inheritdoc />
    public async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(ListingsProviderInfo info, string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
    {
        var shows = await _apiClient.GetCurrentZappShowAsync(channelId, cancellationToken).ConfigureAwait(false);

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
        var channels = await _apiClient.GetZappChannelsAsync(cancellationToken).ConfigureAwait(false);
        return channels.Select(c => new ChannelInfo
        {
            Name = c.Name,
            Id = c.Id,
            Path = c.StreamUrl,
            ChannelType = ChannelType.TV,
            TunerHostId = "zapp"
        }).ToList();
    }
}
