using System;
using System.IO;
using System.Net.Http;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Handlers;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Jellyfin.Plugin.MediathekViewDL.Services.Metadata;
using Jellyfin.Plugin.MediathekViewDL.Services.Subscriptions;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.MediathekViewDL
{
    /// <summary>
    /// Registers plugin services.
    /// </summary>
    public class ServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            // Register a named client for FileDownloader
            serviceCollection.AddHttpClient("FileDownloaderClient");

            // Register the typed client for API
            serviceCollection.AddHttpClient<IMediathekViewApiClient, MediathekViewApiClient>();

            // Database
            serviceCollection.AddDbContext<MediathekViewDlDbContext>(options =>
            {
                var dbPath = Path.Combine(Plugin.Instance!.DataFolderPath, "mediathek-dl.db");
                var dbDir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                options.UseSqlite($"Data Source={dbPath}");
            });

            serviceCollection.AddSingleton<DatabaseMigrator>();
            serviceCollection.AddHostedService<MigrationHostedService>();
            serviceCollection.AddSingleton<IQualityCacheRepository, DbQualityCacheRepository>();
            serviceCollection.AddSingleton<IDownloadHistoryRepository, DbDownloadHistoryRepository>();

            serviceCollection.AddSingleton<IConfigurationProvider, PluginConfigurationProvider>();
            serviceCollection.AddSingleton<ILanguageDetectionService, LanguageDetectionService>();
            serviceCollection.AddSingleton<IVideoParser, VideoParser>();
            serviceCollection.AddSingleton<IFileNameBuilderService, FileNameBuilderService>();
            serviceCollection.AddSingleton<ILocalMediaScanner, LocalMediaScanner>();
            // IMediathekViewApiClient is already registered via AddHttpClient above
            serviceCollection.AddTransient<MediathekViewDlApiService>();
            serviceCollection.AddTransient<IFFmpegService, FFmpegService>();
            serviceCollection.AddTransient<IFileDownloader, FileDownloader>();
            serviceCollection.AddTransient<ISubscriptionProcessor, SubscriptionProcessor>();

            // Register Download Handlers
            serviceCollection.AddTransient<IDownloadHandler, DirectDownloadHandler>();
            serviceCollection.AddTransient<IDownloadHandler, StreamingUrlHandler>();
            serviceCollection.AddTransient<IDownloadHandler, AudioExtractionHandler>();
            serviceCollection.AddTransient<IDownloadHandler, QualityUpgradeHandler>();

            serviceCollection.AddTransient<IDownloadManager, DownloadManager>();
            serviceCollection.AddSingleton<IDownloadQueueManager, DownloadQueueManager>();
            serviceCollection.AddSingleton<IStrmValidationService, StrmValidationService>();
            serviceCollection.AddTransient<INfoService, NfoService>();
        }
    }
}
