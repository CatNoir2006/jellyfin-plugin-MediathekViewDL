using System;
using System.Net;

namespace Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;

/// <summary>
/// Exception thrown when the MediathekView API returns a non-successful HTTP status code.
/// </summary>
public class MediathekApiException : MediathekException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekApiException"/> class.
    /// </summary>
    public MediathekApiException()
        : this(null, HttpStatusCode.InternalServerError)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekApiException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MediathekApiException(string? message)
        : this(message, HttpStatusCode.InternalServerError)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekApiException"/> class with a specified error message and the HTTP status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public MediathekApiException(string? message, HttpStatusCode statusCode)
        : this(message, statusCode, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekApiException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public MediathekApiException(string? message, Exception? innerException)
        : this(message, HttpStatusCode.InternalServerError, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekApiException"/> class with a specified error message, the HTTP status code, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public MediathekApiException(string? message, HttpStatusCode statusCode, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}
