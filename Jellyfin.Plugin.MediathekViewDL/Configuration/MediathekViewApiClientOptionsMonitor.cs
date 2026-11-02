using System;
using MediathekView.Api.External;
using Microsoft.Extensions.Options;

namespace Jellyfin.Plugin.MediathekViewDL.Configuration;

/// <summary>
/// Adapter that exposes the current plugin configuration as <see cref="MediathekViewApiClientOptions"/>.
/// </summary>
public sealed class MediathekViewApiClientOptionsMonitor : IOptionsMonitor<MediathekViewApiClientOptions>
{
    private static readonly NoOpDisposable _noOpDisposable = new();

    private readonly IConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekViewApiClientOptionsMonitor"/> class.
    /// </summary>
    /// <param name="configurationProvider">The plugin configuration provider.</param>
    public MediathekViewApiClientOptionsMonitor(IConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }

    /// <inheritdoc />
    public MediathekViewApiClientOptions CurrentValue => Get(null);

    /// <inheritdoc />
    public MediathekViewApiClientOptions Get(string? name)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        var defaults = new MediathekViewApiClientOptions();
        return new MediathekViewApiClientOptions
        {
            MaxSearchPages = config?.Search.MaxPages ?? defaults.MaxSearchPages,
            AllowHttp = config?.Network.AllowHttp ?? false,
            FetchStreamSizes = config?.Search.FetchStreamSizes ?? false
        };
    }

    /// <inheritdoc />
    public IDisposable? OnChange(Action<MediathekViewApiClientOptions, string?> listener)
    {
        // Options are read lazily from the live plugin configuration on every access, so there are no change notifications.
        return _noOpDisposable;
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
