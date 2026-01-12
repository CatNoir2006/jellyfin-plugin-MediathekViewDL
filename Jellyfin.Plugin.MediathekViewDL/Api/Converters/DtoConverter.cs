using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Api.External.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.ResourceItem;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Converters;

/// <summary>
/// Static converter class for mapping external API models to internal DTOs and vice versa.
/// </summary>
public static class DtoConverter
{
    /// <summary>
    /// Converts a <see cref="ApiQueryDto"/> to a <see cref="ApiQuery"/>.
    /// </summary>
    /// <param name="dto">The DTO to convert.</param>
    /// <returns>The converted model.</returns>
    public static ApiQuery ToModel(this ApiQueryDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ApiQuery
        {
            Future = dto.Future,
            MaxDuration = dto.MaxDuration,
            MinDuration = dto.MinDuration,
            Offset = dto.Offset,
            Size = dto.Size,
            SortBy = dto.SortBy.ToString().ToLowerInvariant(),
            SortOrder = dto.SortOrder.ToString().ToLowerInvariant(),
            Queries = new Collection<QueryFields>(dto.Queries.Select(ToModel).ToList())
        };
    }

    /// <summary>
    /// Converts a <see cref="QueryFieldsDto"/> to a <see cref="QueryFields"/>.
    /// </summary>
    /// <param name="dto">The DTO to convert.</param>
    /// <returns>The converted model.</returns>
    public static QueryFields ToModel(this QueryFieldsDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var fields = new Collection<string>();
        if (dto.Fields.HasFlag(QueryFieldType.Title))
        {
            fields.Add("title");
        }

        if (dto.Fields.HasFlag(QueryFieldType.Topic))
        {
            fields.Add("topic");
        }

        if (dto.Fields.HasFlag(QueryFieldType.Description))
        {
            fields.Add("description");
        }

        if (dto.Fields.HasFlag(QueryFieldType.Channel))
        {
            fields.Add("channel");
        }

        return new QueryFields
        {
            Query = dto.Query,
            Fields = fields
        };
    }

    /// <summary>
    /// Converts a <see cref="QueryInfo"/> to a <see cref="QueryInfoDto"/>.
    /// </summary>
    /// <param name="queryInfo">The query info to convert.</param>
    /// <returns>The converted DTO.</returns>
    public static QueryInfoDto ToDto(this QueryInfo queryInfo)
    {
        ArgumentNullException.ThrowIfNull(queryInfo);

        TimeSpan searchEngineTime = TimeSpan.Zero;
        // Parse the search engine time from string to double milliseconds with dot as decimal separator
        if (double.TryParse(queryInfo.SearchEngineTime, NumberStyles.Any, CultureInfo.InvariantCulture, out double milliseconds))
        {
            searchEngineTime = TimeSpan.FromMilliseconds(milliseconds);
        }

        TotalRelation totalRelation = TotalRelation.Equal;
        if (!string.IsNullOrEmpty(queryInfo.TotalRelation))
        {
            totalRelation = queryInfo.TotalRelation.ToLowerInvariant() switch
            {
                "gte" or "gt" => TotalRelation.GreaterThan,
                "eq" => TotalRelation.Equal,
                _ => totalRelation
            };
        }

        return new QueryInfoDto
        {
            MovieListTimestamp = queryInfo.FilmlisteTimestamp,
            SearchEngineTime = searchEngineTime,
            ResultCount = queryInfo.ResultCount,
            TotalResults = queryInfo.TotalResults,
            TotalRelation = totalRelation,
            TotalEntries = queryInfo.TotalEntries
        };
    }

    /// <summary>
    /// Converts a <see cref="ResultItem"/> to a <see cref="ResultItemDto"/>.
    /// </summary>
    /// <param name="resultItem">The result item to convert.</param>
    /// <returns>The converted DTO.</returns>
    public static ResultItemDto ToDto(this ResultItem resultItem)
    {
        ArgumentNullException.ThrowIfNull(resultItem);

        var videoUrls = new List<VideoUrlDto>();

        if (!string.IsNullOrEmpty(resultItem.UrlVideoLow))
        {
            videoUrls.Add(new VideoUrlDto
            {
                Url = resultItem.UrlVideoLow,
                Quality = 1, // Low
                Size = null, // Individual size unknown
                Language = null,
                Flags = ItemFeatures.None
            });
        }

        if (!string.IsNullOrEmpty(resultItem.UrlVideo))
        {
            videoUrls.Add(new VideoUrlDto
            {
                Url = resultItem.UrlVideo,
                Quality = 2, // Standard
                Size = resultItem.Size, // Assume main size applies here roughly, or unknown
                Language = null,
                Flags = ItemFeatures.None
            });
        }

        if (!string.IsNullOrEmpty(resultItem.UrlVideoHd))
        {
            videoUrls.Add(new VideoUrlDto
            {
                Url = resultItem.UrlVideoHd,
                Quality = 3, // HD
                Size = null,
                Language = null,
                Flags = ItemFeatures.None
            });
        }

        var subtitleUrls = new List<SubtitleUrlDto>();
        if (!string.IsNullOrEmpty(resultItem.UrlSubtitle))
        {
            subtitleUrls.Add(new SubtitleUrlDto
            {
                Url = resultItem.UrlSubtitle,
                Type = DetermineSubtitleType(resultItem.UrlSubtitle),
                Language = null, // Unknown language
                Flags = ItemFeatures.None
            });
        }

        return new ResultItemDto
        {
            Id = resultItem.Id,
            Title = resultItem.Title,
            Topic = resultItem.Topic,
            Channel = resultItem.Channel,
            Description = resultItem.Description,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(resultItem.Timestamp),
            Duration = TimeSpan.FromSeconds(resultItem.Duration),
            Size = resultItem.Size,
            VideoUrls = videoUrls,
            SubtitleUrls = subtitleUrls,
            ExternalIds = new List<ExternalId>() // Populate if available, currently not provided by API (ZDF has IMDb, but it's not contained in MediathekViewApi)
        };
    }

    /// <summary>
    /// Converts a <see cref="ResultChannels"/> to a <see cref="QueryResultDto"/>.
    /// </summary>
    /// <param name="resultChannels">The result channels object.</param>
    /// <returns>The converted DTO.</returns>
    public static QueryResultDto ToDto(this ResultChannels resultChannels)
    {
        ArgumentNullException.ThrowIfNull(resultChannels);

        var itemDtos = new List<ResultItemDto>();
        if (resultChannels.Results != null)
        {
            itemDtos.AddRange(resultChannels.Results.Select(ToDto));
        }

        return new QueryResultDto
        {
            QueryInfo = resultChannels.QueryInfo.ToDto(),
            Results = itemDtos
        };
    }

    private static SubtitleType DetermineSubtitleType(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return SubtitleType.Unknown;
        }

        bool IsTtml()
        {
            return url.Contains("ebutt", StringComparison.OrdinalIgnoreCase) ||
                   url.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
                   url.EndsWith("subtitle", StringComparison.OrdinalIgnoreCase);
        }

        bool IsWebVtt()
        {
            return url.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase) ||
                   url.Contains("webvtt", StringComparison.OrdinalIgnoreCase);
        }

        if (IsTtml())
        {
            return SubtitleType.TTML;
        }

        if (IsWebVtt())
        {
            return SubtitleType.WEBVTT;
        }

        return SubtitleType.Unknown;
    }
}
