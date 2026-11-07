using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Api.Models;
using MediaBrowser.Common.Api;
using MediathekView.Api.Exceptions;
using MediathekView.Api.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Controller;

/// <summary>
/// The Controller for general plugin endpoints.
/// </summary>
[ApiController]
[Route("MediathekViewDL/[controller]")]
[Authorize(Policy = Policies.RequiresElevation)]
public class PluginController : ControllerBase
{
    private readonly IMediathekViewApiClient _apiClient;
    private readonly ILogger<PluginController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginController"/> class.
    /// </summary>
    /// <param name="apiClient">The MediathekView API client.</param>
    /// <param name="logger">The logger.</param>
    public PluginController(IMediathekViewApiClient apiClient, ILogger<PluginController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the initialization error, if any.
    /// </summary>
    /// <returns>The error message or null.</returns>
    [HttpGet("InitializationError")]
    public ActionResult<string?> GetInitializationError()
    {
        if (Plugin.Instance?.InitializationException is null)
        {
            return Ok(null);
        }

        string msg = Plugin.Instance.InitializationException.Message;
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = "Ein unbekannter Fehler während der Initialisierung ist aufgetreten.";
        }

        return Ok(msg);
    }

    /// <summary>
    /// Tests the connection to the MediathekViewWeb server by calling its stats health endpoint.
    /// </summary>
    /// <param name="baseUrl">Optional base URL to test. When omitted, the currently configured base URL is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection test result.</returns>
    [HttpPost("TestApiConnection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionTestResultDto>> TestApiConnection([FromQuery] string? baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.CheckConnectionAsync(baseUrl, cancellationToken).ConfigureAwait(false);
            return Ok(new ConnectionTestResultDto(true, "Verbindung erfolgreich: Der MediathekViewWeb Server läuft."));
        }
        catch (MediathekException ex)
        {
            _logger.LogWarning(ex, "API connection test failed for base URL '{BaseUrl}'", baseUrl);
            return Ok(new ConnectionTestResultDto(false, ex.Message));
        }
    }

    /// <summary>
    /// Tests the connection to the Zapp API server by calling its channelInfoList endpoint.
    /// </summary>
    /// <param name="baseUrl">Optional base URL to test. When omitted, the currently configured Zapp base URL is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection test result.</returns>
    [HttpPost("TestZappConnection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionTestResultDto>> TestZappConnection([FromQuery] string? baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.CheckZappConnectionAsync(baseUrl, cancellationToken).ConfigureAwait(false);
            return Ok(new ConnectionTestResultDto(true, "Verbindung erfolgreich: Die Zapp API ist erreichbar und liefert ein parsbares Ergebnis."));
        }
        catch (MediathekException ex)
        {
            _logger.LogWarning(ex, "Zapp API connection test failed for base URL '{BaseUrl}'", baseUrl);
            return Ok(new ConnectionTestResultDto(false, ex.Message));
        }
    }

    /// <summary>
    /// Notifies plugin services that the configuration was updated.
    /// Used by the frontend after saving through Jellyfin's configuration endpoints,
    /// so that settings like the parallel download limit apply without a restart.
    /// </summary>
    /// <returns>No content.</returns>
    [HttpPost("ConfigurationChanged")]
    public IActionResult NotifyConfigurationChanged()
    {
        Plugin.RaiseConfigurationSaved();
        return NoContent();
    }
}
