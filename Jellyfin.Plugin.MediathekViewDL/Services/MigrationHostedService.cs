using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Runs database migrations and configuration updates at startup.
/// </summary>
public class MigrationHostedService : IHostedService
{
    private readonly DatabaseMigrator _migrator;
    private readonly IConfigurationProvider _configProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationHostedService"/> class.
    /// </summary>
    /// <param name="migrator">The database migrator.</param>
    /// <param name="configProvider">The configuration provider.</param>
    public MigrationHostedService(DatabaseMigrator migrator, IConfigurationProvider configProvider)
    {
        _migrator = migrator;
        _configProvider = configProvider;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);
        MigrateSubscriptions();
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void MigrateSubscriptions()
    {
        var config = _configProvider.Configuration;
        var changed = false;

        foreach (var subscription in config.Subscriptions)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (subscription.Criteria.Count == 0 && subscription.Queries.Count > 0)
            {
                foreach (var oldQuery in subscription.Queries)
                {
                    var newCriteria = new QueryFieldsDto
                    {
                        Query = oldQuery.Query,
                        Fields = 0
                    };

                    foreach (var field in oldQuery.Fields)
                    {
                        if (field.Equals("title", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields |= QueryFieldType.Title;
                        }
                        else if (field.Equals("topic", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields |= QueryFieldType.Topic;
                        }
                        else if (field.Equals("description", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields |= QueryFieldType.Description;
                        }
                        else if (field.Equals("channel", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields |= QueryFieldType.Channel;
                        }
                    }

                    if (newCriteria.Fields == 0)
                    {
                        // No valid fields, skip this query, this should not happen though (hoppfully).
                        // But to be safe, we disable the subscription to avoid unexpected behavior.
                        subscription.IsEnabled = false;
                        continue;
                    }

                    subscription.Criteria.Add(newCriteria);
                }

                changed = true;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (changed)
        {
            _configProvider.Save();
        }
    }
}
