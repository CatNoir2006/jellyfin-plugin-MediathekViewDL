using System;

namespace Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;

/// <summary>
/// Base class for all MediathekViewDL API related exceptions.
/// </summary>
public abstract class MediathekException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekException"/> class.
    /// </summary>
    protected MediathekException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected MediathekException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    protected MediathekException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
