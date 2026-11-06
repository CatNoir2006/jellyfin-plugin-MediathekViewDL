using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Controller;

/// <summary>
/// The Controller for general plugin endpoints.
/// </summary>
[ApiController]
[Route("MediathekViewDL/[controller]")]
[Authorize(Policy = Policies.RequiresElevation)]
public class PluginController : ControllerBase
{
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
