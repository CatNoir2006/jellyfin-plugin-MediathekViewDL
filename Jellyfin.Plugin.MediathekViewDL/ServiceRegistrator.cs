using System.IO;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Data;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Services.Downloading;
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
            serviceCollection.AddHttpClient(); // Required for FileDownloader

            // Database
            serviceCollection.AddDbContext<MediathekViewDlDbContext>(options =>
            {
                var dbPath = System.IO.Path.Combine(Plugin.Instance!.DataFolderPath, "mediathek-dl.db");
                var dbDir = System.IO.Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !System.IO.Directory.Exists(dbDir))
                {
                    System.IO.Directory.CreateDirectory(dbDir);
                }

                options.UseSqlite($"Data Source={dbPath}");
            });

            serviceCollection.AddSingleton<DatabaseMigrator>();
            serviceCollection.AddHostedService<MigrationHostedService>();
            serviceCollection.AddSingleton<IQualityCacheRepository, DbQualityCacheRepository>();
            serviceCollection.AddSingleton<IDownloadHistoryRepository, DbDownloadHistoryRepository>();

            serviceCollection.AddSingleton<ILanguageDetectionService, LanguageDetectionService>();
            serviceCollection.AddSingleton<IVideoParser, VideoParser>();
            serviceCollection.AddSingleton<IFileNameBuilderService, FileNameBuilderService>();
            serviceCollection.AddSingleton<ILocalMediaScanner, LocalMediaScanner>();
            serviceCollection.AddTransient<IMediathekViewApiClient, MediathekViewApiClient>();
            serviceCollection.AddTransient<MediathekViewDlApiService>();
            serviceCollection.AddTransient<IFFmpegService, FFmpegService>();
            serviceCollection.AddTransient<IFileDownloader, FileDownloader>();
            serviceCollection.AddTransient<ISubscriptionProcessor, SubscriptionProcessor>();
            serviceCollection.AddTransient<IDownloadManager, DownloadManager>();
            serviceCollection.AddSingleton<IDownloadQueueManager, DownloadQueueManager>();
            serviceCollection.AddSingleton<IStrmValidationService, StrmValidationService>();
            serviceCollection.AddTransient<INfoService, NfoService>();
        }
    }
}
