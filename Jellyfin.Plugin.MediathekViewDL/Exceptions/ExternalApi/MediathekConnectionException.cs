using System;

namespace Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;

/// <summary>
/// Exception thrown when a connection to the MediathekView API fails (e.g., network issues, timeouts).
/// </summary>
public class MediathekConnectionException : MediathekException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekConnectionException"/> class.
    /// </summary>
    public MediathekConnectionException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekConnectionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MediathekConnectionException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekConnectionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public MediathekConnectionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
