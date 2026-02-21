using Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// A standardized error response for the plugin API.
/// </summary>
/// <param name="Id">A unique machine-readable identifier for the error type.</param>
/// <param name="Detail">A human-readable explanation of the error.</param>
public record ApiErrorDto(ApiErrorId Id, string Detail);
