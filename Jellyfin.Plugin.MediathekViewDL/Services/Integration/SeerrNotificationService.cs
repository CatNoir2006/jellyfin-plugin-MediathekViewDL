using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Integration;

/// <summary>
/// Implements Seerr / Jellyseerr background notification.
/// </summary>
public class SeerrNotificationService : ISeerrNotificationService
{
    private readonly ILogger<SeerrNotificationService> _logger;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeerrNotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public SeerrNotificationService(
        ILogger<SeerrNotificationService> logger,
        IConfigurationProvider configurationProvider,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configurationProvider = configurationProvider;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<bool> TriggerRecentlyAddedScanAsync(CancellationToken cancellationToken)
    {
        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null || !config.Federation.AutoTriggerSeerrScan)
        {
            return true;
        }

        var baseUrl = config.Federation.SeerrUrl;
        var apiKey = config.Federation.SeerrApiKey;

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("Seerr scan trigger skipped: URL or API key is not configured.");
            return true;
        }

        try
        {
            var url = $"{baseUrl.TrimEnd('/')}/api/v1/settings/jobs/jellyfin-recently-added-scan/run";
            using var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.TryAddWithoutValidation("X-Api-Key", apiKey.Trim());

            using var resp = await client.SendAsync(req, cancellationToken).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully triggered Seerr / Jellyseerr recently-added scan (HTTP {StatusCode})", (int)resp.StatusCode);
                return true;
            }

            _logger.LogWarning("Seerr returned status {StatusCode} when triggering recently-added scan", (int)resp.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send scan trigger to Seerr at {Url}", baseUrl);
            return false;
        }
    }
}
