using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service for validating streaming URLs in .strm files.
/// </summary>
public class StrmValidationService
{
    private readonly ILogger<StrmValidationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrmValidationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public StrmValidationService(ILogger<StrmValidationService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Gets the plugin configuration. Virtual for testing.
    /// </summary>
    protected virtual PluginConfiguration Configuration => Plugin.Instance!.Configuration;

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
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Invalid URL format: {Url}", url);
            return false;
        }

        // Security check: Only allow HTTPS
        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            _logger.LogWarning("Insecure URL scheme (not HTTPS): {Url}", url);
            return false;
        }

        var config = Configuration;
        if (config == null)
        {
            _logger.LogError("Plugin configuration not available.");
            return false; // Fail safe
        }

        // Domain check (reuse logic/list from configuration)
        if (!config.AllowUnknownDomains)
        {
            var host = uri.Host;
            var hostParts = host.Split('.');
            if (hostParts.Length >= 2)
            {
                var topDomain = string.Join('.', hostParts[^2..]);
                if (!config.AllowedDomains.Contains(topDomain))
                {
                    _logger.LogWarning("Domain '{Domain}' is not in the allowed list. URL: {Url}", topDomain, url);
                    return false;
                }
            }
            else
            {
                _logger.LogWarning("Invalid host format: {Host}", host);
                return false;
            }
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            // Use HEAD request to check if the file exists without downloading it
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            // If HEAD fails (some servers might not support it), try a Range request for the first byte
            if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                 _logger.LogDebug("HEAD request not allowed for {Url}, trying GET with Range header.", url);
                 using var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
                 getRequest.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);
                 using var getResponse = await client.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                 return getResponse.IsSuccessStatusCode;
            }

            _logger.LogDebug("URL validation failed for {Url}. Status Code: {StatusCode}", url, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
             _logger.LogDebug(ex, "HTTP request error validating URL {Url}", url);
             return false;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating URL {Url}", url);
            return false;
        }
    }
}
