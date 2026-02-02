using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Runs database migrations and configuration updates at startup.
/// </summary>
public class MigrationHostedService : IHostedService
{
    private readonly DatabaseMigrator _migrator;
    private readonly IConfigurationProvider _configProvider;
    private readonly ILogger<MigrationHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationHostedService"/> class.
    /// </summary>
    /// <param name="migrator">The database migrator.</param>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="logger">The Logger.</param>
    public MigrationHostedService(DatabaseMigrator migrator, IConfigurationProvider configProvider, ILogger<MigrationHostedService> logger)
    {
        _migrator = migrator;
        _configProvider = configProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _migrator.EnsureMigratedAsync().ConfigureAwait(false);
        MigrateConfiguration();
        MigrateSubscriptions();
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void MigrateConfiguration()
    {
        var config = _configProvider.Configuration;

        // Migrate old path settings to new Paths property if it's empty
        if (config.Paths.IsEmpty())
        {
            var pathsOld = new ConfigurationPaths()
            {
                TempDownloadPath = config.DeprecatedTempDownloadPath,
                DefaultDownloadPath = config.DeprecatedDefaultDownloadPath,
                DefaultManualMoviePath = config.DeprecatedDefaultManualMoviePath,
                DefaultManualShowPath = config.DeprecatedDefaultManualShowPath,
                DefaultSubscriptionMoviePath = config.DeprecatedDefaultSubscriptionMoviePath,
                DefaultSubscriptionShowPath = config.DeprecatedDefaultSubscriptionShowPath,
                UseTopicForMoviePath = config.DeprecatedUseTopicForMoviePath,
            };
            config.Paths = pathsOld;
            _configProvider.Save();
        }
    }

    private void MigrateSubscriptions()
    {
        var config = _configProvider.Configuration;
        var changed = false;

        foreach (var subscription in config.Subscriptions)
        {
            if (subscription.Criteria.Count == 0 && subscription.Queries.Count > 0)
            {
                foreach (var oldQuery in subscription.Queries)
                {
                    var newCriteria = new QueryFieldsDto { Query = oldQuery.Query };

                    foreach (var field in oldQuery.Fields)
                    {
                        if (field.Equals("title", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields.Add(QueryFieldType.Title);
                        }
                        else if (field.Equals("topic", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields.Add(QueryFieldType.Topic);
                        }
                        else if (field.Equals("description", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields.Add(QueryFieldType.Description);
                        }
                        else if (field.Equals("channel", StringComparison.OrdinalIgnoreCase))
                        {
                            newCriteria.Fields.Add(QueryFieldType.Channel);
                        }
                    }

                    if (newCriteria.Fields.Count == 0)
                    {
                        _logger.LogWarning(
                            "Subscription '{SubscriptionName}' (ID: {SubscriptionId}) has a query with no valid fields after migration. Disabling subscription to prevent unexpected behavior. Query: '{QueryText}'",
                            subscription.Name,
                            subscription.Id,
                            oldQuery.Query);
                        subscription.IsEnabled = false;
                        continue;
                    }

                    subscription.Criteria.Add(newCriteria);
                }

                changed = true;
            }
        }

        if (changed)
        {
            _configProvider.Save();
        }
    }
}
