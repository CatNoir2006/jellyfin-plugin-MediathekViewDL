using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

namespace Jellyfin.Plugin.MediathekViewDL.Api;

/// <summary>
/// Service for parsing search strings into query fields.
/// </summary>
public class QueryParser : IQueryParser
{
    /// <inheritdoc />
    public Collection<QueryFieldsDto> Parse(string? title, string? topic, string? channel, string? combinedSearch)
    {
        var allQueries = GenerateQueryFromCsv(title, [QueryFieldType.Title])
            .Concat(GenerateQueryFromCsv(topic, [QueryFieldType.Topic]))
            .Concat(GenerateQueryFromCsv(channel, [QueryFieldType.Channel]))
            .Concat(GenerateQueryFromCsv(combinedSearch, [QueryFieldType.Title, QueryFieldType.Topic]));

        return new Collection<QueryFieldsDto>(allQueries.ToList());
    }

    private static List<QueryFieldsDto> GenerateQueryFromCsv(string? input, Collection<QueryFieldType> fields)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(q =>
            new QueryFieldsDto()
            {
                Fields = fields,
                Query = q.StartsWith('!') ? q[1..].Trim() : q,
                IsExclude = q.StartsWith('!')
            }).ToList();
    }
}
