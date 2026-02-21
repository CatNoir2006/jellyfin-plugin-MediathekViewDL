using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models.Enums;

/// <summary>
/// Machine-readable error identifiers for the API.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiErrorId
{
    /// <summary>
    /// An unexpected internal error occurred.
    /// </summary>
    InternalError,

    /// <summary>
    /// The plugin has not been initialized correctly.
    /// </summary>
    InitializationError,

    /// <summary>
    /// The plugin configuration is not available.
    /// </summary>
    ConfigurationNotAvailable,

    /// <summary>
    /// The provided item is invalid or missing required data.
    /// </summary>
    InvalidItem,

    /// <summary>
    /// The provided options are invalid.
    /// </summary>
    InvalidOptions,

    /// <summary>
    /// The subscription configuration is invalid.
    /// </summary>
    InvalidSubscription,

    /// <summary>
    /// Could not parse the video information.
    /// </summary>
    ParseError,

    /// <summary>
    /// The generated or provided path is invalid.
    /// </summary>
    InvalidPath,

    /// <summary>
    /// The provided path is unsafe (e.g. path traversal).
    /// </summary>
    UnsafePath,

    /// <summary>
    /// The search criteria are invalid or missing.
    /// </summary>
    InvalidSearch,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The operation is invalid in the current state.
    /// </summary>
    InvalidOperation,

    /// <summary>
    /// Not enough free disk space.
    /// </summary>
    InsufficientDiskSpace,

    /// <summary>
    /// The filename contains invalid characters.
    /// </summary>
    InvalidFilename,

    /// <summary>
    /// The MediathekView API is unavailable.
    /// </summary>
    MediathekUnavailable,

    /// <summary>
    /// The MediathekView API returned an invalid response.
    /// </summary>
    MediathekInvalidResponse,

    /// <summary>
    /// The MediathekView API returned an error.
    /// </summary>
    MediathekApiError,

    /// <summary>
    /// A general error occurred while calling the MediathekView API.
    /// </summary>
    MediathekError
}
