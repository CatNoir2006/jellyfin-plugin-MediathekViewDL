using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        foreach (var field in dto.Fields)
        {
            fields.Add(field.ToString().ToLowerInvariant());
        }

        return new QueryFields { Query = dto.Query, Fields = fields };
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
    /// <param name="upgradeToHttps">If http should be upgraded to https.</param>
    /// <returns>The converted DTO.</returns>
    public static ResultItemDto ToDto(this ResultItem resultItem, bool upgradeToHttps)
    {
        ArgumentNullException.ThrowIfNull(resultItem);

        var videoUrls = new List<VideoUrlDto>();

        if (!string.IsNullOrEmpty(resultItem.UrlVideoLow))
        {
            var videoUrl = resultItem.UrlVideoLow;
            videoUrls.Add(new VideoUrlDto
            {
                Url = upgradeToHttps ? UrlHttpsUpgrade(videoUrl) : videoUrl,
                Quality = 1, // Low
                Size = null, // Individual size unknown
                Language = null
            });
        }

        if (!string.IsNullOrEmpty(resultItem.UrlVideo))
        {
            var videoUrl = resultItem.UrlVideo;
            videoUrls.Add(new VideoUrlDto
            {
                Url = upgradeToHttps ? UrlHttpsUpgrade(videoUrl) : videoUrl,
                Quality = 2, // Standard
                Size = null, // Assume main size applies here roughly, or unknown
                Language = null
            });
        }

        if (!string.IsNullOrEmpty(resultItem.UrlVideoHd))
        {
            var videoUrl = resultItem.UrlVideoHd;
            videoUrls.Add(new VideoUrlDto
            {
                Url = upgradeToHttps ? UrlHttpsUpgrade(videoUrl) : videoUrl,
                Quality = 3, // HD
                Size = null,
                Language = null
            });
        }

        var subtitleUrls = ExtractSubtitleUrls(resultItem.UrlSubtitle);

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
    /// <param name="apiQueryDto">The API Query.</param>
    /// <param name="upgradeToHttps">If upgrade to https should be performed.</param>
    /// <returns>The converted DTO.</returns>
    public static QueryResultDto ToDto(this ResultChannels resultChannels, ApiQueryDto apiQueryDto, bool upgradeToHttps)
    {
        ArgumentNullException.ThrowIfNull(resultChannels);

        var itemDtos = new List<ResultItemDto>();
        if (resultChannels.Results != null)
        {
            itemDtos.AddRange(resultChannels.Results.Select(r => r.ToDto(upgradeToHttps)).Where(r =>
            {
                if (apiQueryDto.MinBroadcastDate.HasValue && r.Timestamp < apiQueryDto.MinBroadcastDate.Value)
                {
                    return false;
                }

                if (apiQueryDto.MaxBroadcastDate.HasValue && r.Timestamp > apiQueryDto.MaxBroadcastDate.Value)
                {
                    return false;
                }

                return true;
            }));
        }

        return new QueryResultDto { QueryInfo = resultChannels.QueryInfo.ToDto(), Results = itemDtos };
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
                   url.EndsWith(".ttml", StringComparison.OrdinalIgnoreCase) ||
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

    private static string UrlHttpsUpgrade(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var uriRes) && uriRes.Scheme == Uri.UriSchemeHttp)
        {
            var builder = new UriBuilder(uriRes) { Scheme = Uri.UriSchemeHttps };
            return builder.ToString();
        }

        return uri;
    }

    /// <summary>
    /// Extracts additional Subtitle Types from the Main Subtitle Url.
    /// </summary>
    /// <param name="subtitle">The SubtitleURL.</param>
    /// <returns>A list of subtitle URls.</returns>
    private static List<SubtitleUrlDto> ExtractSubtitleUrls(string subtitle)
    {
        var subtitleUrls = new List<SubtitleUrlDto>();

        if (string.IsNullOrWhiteSpace(subtitle))
        {
            return subtitleUrls;
        }

        void AddSub(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (subtitleUrls.Any(s => s.Url == url))
            {
                return;
            }

            subtitleUrls.Add(new SubtitleUrlDto
            {
                Url = url, Type = DetermineSubtitleType(url), Language = null // Unknown language
            });
        }

        void ExtractArdUrls()
        {
            const string Pattern = @"https:\/\/api\.ardmediathek\.de\/player-service\/subtitle\/(?:ebutt|webvtt)\/urn:ard:subtitle:([a-f0-9]+)(?:\.vtt)?";
            var match = Regex.Match(subtitle, Pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return;
            }

            var subtitleId = match.Groups[1].Value;
            AddSub($"https://api.ardmediathek.de/player-service/subtitle/ebutt/urn:ard:subtitle:{subtitleId}");
            AddSub($"https://api.ardmediathek.de/player-service/subtitle/webvtt/urn:ard:subtitle:{subtitleId}.vtt");
        }

        void ExtractZdfUrls()
        {
            if (string.IsNullOrEmpty(subtitle) || !subtitle.Contains("zdf.de", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int lastSlashIndex = subtitle.LastIndexOf('/');
            int lastDotIndex = subtitle.LastIndexOf('.');

            if (lastDotIndex <= lastSlashIndex)
            {
                return;
            }

            string baseUrl = subtitle.Substring(0, lastDotIndex);
            AddSub(baseUrl + ".xml");
            AddSub(baseUrl + ".vtt");
        }

        void ExtractKikaUrls()
        {
            if (string.IsNullOrEmpty(subtitle) || !subtitle.Contains("kika.de", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int lastSlashIndex = subtitle.LastIndexOf('/');

            if (lastSlashIndex <= 8) // "https://".Length
            {
                return;
            }

            string baseUrl = subtitle.Substring(0, lastSlashIndex);
            AddSub(baseUrl + "/subtitle");
            AddSub(baseUrl + "/webvtt");
        }

        AddSub(subtitle);
        ExtractArdUrls();
        ExtractZdfUrls();
        ExtractKikaUrls();

        return subtitleUrls;
    }
}
