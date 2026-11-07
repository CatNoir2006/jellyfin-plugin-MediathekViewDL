namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// The result of a MediathekViewWeb server connection check.
/// </summary>
/// <param name="Success">A value indicating whether the check succeeded.</param>
/// <param name="Message">A human-readable detail message.</param>
public record ConnectionTestResultDto(bool Success, string Message);
