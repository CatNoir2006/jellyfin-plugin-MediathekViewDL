using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Configuration.Groups;
using Jellyfin.Plugin.MediathekViewDL.Configuration.SubscriptionSettings;
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
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void MigrateConfiguration()
    {
        var config = _configProvider.Configuration;
        const int CurrentConfigVersion = 2;

        if (config.ConfigVersion >= CurrentConfigVersion)
        {
            return;
        }

        _logger.LogInformation("Migrating configuration from version {OldVersion} to {NewVersion}", config.ConfigVersion, CurrentConfigVersion);

        // Version 0 -> 1: Migrate Configuration Top-Level Settings to grouped settings
        // This Migration is planed to be supported until Version 1.0.0.0
        if (config.ConfigVersion < 1)
        {
            // Migrate old path settings to new Paths property if it's empty
            config.Paths = new ConfigurationPaths()
            {
                TempDownloadPath = config.DeprecatedTempDownloadPath,
                DefaultDownloadPath = config.DeprecatedDefaultDownloadPath,
                DefaultManualMoviePath = config.DeprecatedDefaultManualMoviePath,
                DefaultManualShowPath = config.DeprecatedDefaultManualShowPath,
                DefaultSubscriptionMoviePath = config.DeprecatedDefaultSubscriptionMoviePath,
                DefaultSubscriptionShowPath = config.DeprecatedDefaultSubscriptionShowPath,
                UseTopicForMoviePath = config.DeprecatedUseTopicForMoviePath,
            };

            // Migrate Download Options
            config.Download = new DownloadOptions
            {
                DownloadSubtitles = config.DeprecatedDownloadSubtitles,
                EnableDirectAudioExtraction = config.DeprecatedEnableDirectAudioExtraction,
                MaxBandwidthMBits = config.DeprecatedMaxBandwidthMBits,
                MinFreeDiskSpaceBytes = config.DeprecatedMinFreeDiskSpaceBytes,
                ScanLibraryAfterDownload = config.DeprecatedScanLibraryAfterDownload
            };

            // Migrate Search Options
            config.Search = new SearchOptions { FetchStreamSizes = config.DeprecatedFetchStreamSizes, SearchInFutureBroadcasts = config.DeprecatedSearchInFutureBroadcasts };

            // Migrate Network Options
            config.Network = new NetworkOptions { AllowUnknownDomains = config.DeprecatedAllowUnknownDomains, AllowHttp = config.DeprecatedAllowHttp };

            // Migrate Maintenance Options
            config.Maintenance = new MaintenanceOptions { EnableStrmCleanup = config.DeprecatedEnableStrmCleanup, AllowDownloadOnUnknownDiskSpace = config.DeprecatedAllowDownloadOnUnknownDiskSpace };

            // Update config version
            config.ConfigVersion = 1;
            _configProvider.Save();
        }

        // Version 1 -> 2: Migrate Subscription Top-Level Options to SettingsGroup
        // This Migration is planed to be supported until Version 1.0.0.0
        if (config.ConfigVersion < 2)
        {
            // Migrate Subscription Queries to Criteria (The very old format)
            void MigrateSubscriptionLegacy()
            {
                foreach (var subscription in config.Subscriptions)
                {
                    if (subscription.DeprecatedCriteria.Count != 0 || subscription.DeprecatedQueries.Count <= 0)
                    {
                        continue;
                    }

                    foreach (var oldQuery in subscription.DeprecatedQueries)
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

                        subscription.DeprecatedCriteria.Add(newCriteria);
                    }
                }

                // Update config version
                config.ConfigVersion = 2;
                _configProvider.Save();
            }

            MigrateSubscriptionLegacy();

            // Migrate Subscription Flat Properties to Settings Grouped in Category's
            foreach (var sub in config.Subscriptions)
            {
                sub.Accessibility = new AccessibilitySettings() { AllowAudioDescription = sub.DeprecatedAllowAudioDescription, AllowSignLanguage = sub.DeprecatedAllowSignLanguage };
                sub.Download = new DownloadSettings()
                {
                    AllowFallbackToLowerQuality = sub.DeprecatedAllowFallbackToLowerQuality,
                    AutoUpgradeToHigherQuality = sub.DeprecatedAutoUpgradeToHigherQuality,
                    DownloadFullVideoForSecondaryAudio = sub.DeprecatedDownloadFullVideoForSecondaryAudio,
                    DownloadPath = sub.DeprecatedDownloadPath,
                    EnhancedDuplicateDetection = sub.DeprecatedEnhancedDuplicateDetection,
                    QualityCheckWithUrl = sub.DeprecatedQualityCheckWithUrl,
                    UseStreamingUrlFiles = sub.DeprecatedUseStreamingUrlFiles,
                };
                sub.Metadata = new MetadataSettings()
                {
                    AppendDateToTitle = sub.DeprecatedAppendDateToTitle, AppendTimeToTitle = sub.DeprecatedAppendTimeToTitle, CreateNfo = sub.DeprecatedCreateNfo, OriginalLanguage = sub.DeprecatedOriginalLanguage,
                };
                sub.Search = new SearchSettings()
                {
                    Criteria = sub.DeprecatedCriteria,
                    MinBroadcastDate = sub.DeprecatedMinBroadcastDate,
                    MaxBroadcastDate = sub.DeprecatedMaxBroadcastDate,
                    MinDurationMinutes = sub.DeprecatedMinDurationMinutes,
                    MaxDurationMinutes = sub.DeprecatedMaxDurationMinutes,
                };
                sub.Series = new SeriesSettings()
                {
                    AllowAbsoluteEpisodeNumbering = sub.DeprecatedAllowAbsoluteEpisodeNumbering,
                    EnforceSeriesParsing = sub.DeprecatedEnforceSeriesParsing,
                    TreatNonEpisodesAsExtras = sub.DeprecatedTreatNonEpisodesAsExtras,
                    SaveExtrasAsStrm = sub.DeprecatedSaveExtrasAsStrm,
                    SaveGenericExtras = sub.DeprecatedSaveGenericExtras,
                    SaveInterviews = sub.DeprecatedSaveInterviews,
                    SaveTrailers = sub.DeprecatedSaveTrailers,
                };
            }

            // Update config version
            config.ConfigVersion = 1;
            _configProvider.Save();
        }

        // The migration is complete, update the config version if the Migration Versions is still outdated
        if (config.ConfigVersion < CurrentConfigVersion)
        {
            config.ConfigVersion = CurrentConfigVersion;
            _configProvider.Save();
        }

        _logger.LogInformation("Configuration migration completed successfully.");
    }
}
