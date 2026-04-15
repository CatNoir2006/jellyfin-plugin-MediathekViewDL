using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Library;

/// <summary>
/// Service for validating streaming URLs in .strm files.
/// </summary>
public class StrmValidationService : IStrmValidationService
{
    private readonly ILogger<StrmValidationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigurationProvider _configurationProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrmValidationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="configurationProvider">The configuration provider.</param>
    public StrmValidationService(ILogger<StrmValidationService> logger, IHttpClientFactory httpClientFactory, IConfigurationProvider configurationProvider)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configurationProvider = configurationProvider;
    }

    /// <summary>
    /// Validates a streaming URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the URL is valid and accessible, otherwise false.</returns>
    public async Task<bool> ValidateUrlAsync(string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid URL format: {url}", nameof(url));
        }

        var config = _configurationProvider.ConfigurationOrNull;
        if (config == null)
        {
            throw new InvalidOperationException("Plugin configuration not available.");
        }

        // Security check: Only allow HTTPS
        if (uri.Scheme != Uri.UriSchemeHttps && !(config.Network.AllowHttp && uri.Scheme == Uri.UriSchemeHttp))
        {
            throw new ArgumentException($"Insecure URL scheme (not HTTPS): {url}", nameof(url));
        }

        // Domain check (reuse logic/list from configuration)
        if (!config.Network.AllowUnknownDomains)
        {
            var host = uri.Host;
            var hostParts = host.Split('.');
            if (hostParts.Length >= 2)
            {
                var topDomain = string.Join('.', hostParts[^2..]);
                if (!config.AllowedDomains.Contains(topDomain))
                {
                    throw new InvalidOperationException($"Domain '{topDomain}' is not in the allowed list. URL: {url}");
                }
            }
            else
            {
                throw new ArgumentException($"Invalid host format: {host}", nameof(url));
            }
        }

        var client = _httpClientFactory.CreateClient();
        // Use HEAD request to check if the file exists without downloading it
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        // If HEAD fails (some servers might not support it), try a Range request for the first byte
        if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
        {
            _logger.LogDebug("HEAD request not allowed for {Url}, trying GET with Range header.", url);
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
            getRequest.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);
            using var getResponse = await client.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (getResponse.IsSuccessStatusCode)
            {
                return true;
            }

            if (getResponse.StatusCode == HttpStatusCode.NotFound || getResponse.StatusCode == HttpStatusCode.Gone)
            {
                _logger.LogInformation("URL validation confirmed invalid (404/410) for {Url}. Status Code: {StatusCode}", url, getResponse.StatusCode);
                return false;
            }

            throw new HttpRequestException($"Validation failed with status code {getResponse.StatusCode} for URL {url}");
        }

        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Gone)
        {
            _logger.LogInformation("URL validation confirmed invalid (404/410) for {Url}. Status Code: {StatusCode}", url, response.StatusCode);
            return false;
        }

        throw new HttpRequestException($"Validation failed with status code {response.StatusCode} for URL {url}");
    }
}
