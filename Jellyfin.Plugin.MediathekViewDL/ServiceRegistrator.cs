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
            serviceCollection.AddSingleton<IStrmValidationService, StrmValidationService>();
        }
    }
}
