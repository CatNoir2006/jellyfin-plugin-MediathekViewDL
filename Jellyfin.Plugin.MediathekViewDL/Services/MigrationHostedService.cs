using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Exceptions;
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
        try
        {
            await _migrator.EnsureMigratedAsync().ConfigureAwait(false);
            MigrateConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize MediathekViewDL plugin. The plugin will be disabled to prevent server instability.");
            if (Plugin.Instance is not null)
            {
                Plugin.Instance.InitializationException = ex;
            }
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void MigrateConfiguration()
    {
        var config = _configProvider.Configuration;
        const int TargetVersion = 4;
        const int MinimumSupportetUpgradeVersion = 3;

        // Skip migration if Downgrading and log an error. Downgrading is not supported to prevent data loss and inconsistencies. The plugin should be updated to the latest version to ensure compatibility with the existing configuration.
        if (config.ConfigVersion > TargetVersion)
        {
            _logger.LogError("Configuration version {ConfigVersion} is newer than the current supported version {CurrentConfigVersion}. No migration will be performed. Please update the plugin to the latest version. Downgrading is not Supportet.", config.ConfigVersion, TargetVersion);
            throw new UnsupportedConfigurationVersionException(config.ConfigVersion, MinimumSupportetUpgradeVersion, TargetVersion);
        }

        // Skip migration if the configuration version is up to date
        if (config.ConfigVersion == TargetVersion)
        {
            return;
        }

        // Fresh installation detection: If version is 0 and its never been run.
        // We set it to TargetVersion immediately to avoid the "Unsupported Version" error.
        if (config.ConfigVersion == 0 && config.LastRun == default)
        {
            _logger.LogInformation("Fresh installation detected. Setting configuration version to {TargetVersion}.", TargetVersion);
            config.ConfigVersion = TargetVersion;
            _configProvider.Save();
            return;
        }

        if (config.ConfigVersion < MinimumSupportetUpgradeVersion)
        {
            _logger.LogError("Configuration version {ConfigVersion} is too old to be migrated to the current supported version {CurrentConfigVersion}. Please install a older Version of the plugin that supports this configuration version and update the configuration step by step until it can be migrated to the latest version.", config.ConfigVersion, TargetVersion);
            UnsupportedVersionUpgradeInfo(config.ConfigVersion, TargetVersion);
            throw new UnsupportedConfigurationVersionException(config.ConfigVersion, MinimumSupportetUpgradeVersion, TargetVersion);
        }

        _logger.LogInformation("Migrating configuration from version {OldVersion} to {NewVersion}", config.ConfigVersion, TargetVersion);

        // The migration is complete, update the config version if the Migration Versions is still outdated
        if (config.ConfigVersion < TargetVersion)
        {
            config.ConfigVersion = TargetVersion;
            _configProvider.Save();
        }

        _logger.LogInformation("Configuration migration completed successfully.");
    }

    private void UnsupportedVersionUpgradeInfo(int? currentVersion, int targetVersion)
    {
        // This is not used at the moment, as the migrations support all Version at the moment.
        // In future versions this could be just to provide more detailed Information about the Upgrade path.
    }
}
