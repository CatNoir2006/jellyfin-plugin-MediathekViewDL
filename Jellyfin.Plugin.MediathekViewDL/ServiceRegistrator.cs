using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
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

            serviceCollection.AddSingleton<LanguageDetectionService>();
            serviceCollection.AddSingleton<VideoParser>();
            serviceCollection.AddSingleton<FileNameBuilderService>();
            serviceCollection.AddSingleton<LocalMediaScanner>();
            serviceCollection.AddTransient<MediathekViewApiClient>();
            serviceCollection.AddTransient<MediathekViewDlApiService>();
            serviceCollection.AddTransient<FFmpegService>();
            serviceCollection.AddTransient<FileDownloader>();
            serviceCollection.AddTransient<SubscriptionProcessor>();
            serviceCollection.AddTransient<DownloadManager>();
            serviceCollection.AddSingleton<StrmValidationService>();
        }
    }
}
