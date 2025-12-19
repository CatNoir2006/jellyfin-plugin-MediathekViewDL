using System;

namespace Jellyfin.Plugin.MediathekViewDL.Exceptions.ExternalApi;

/// <summary>
/// Exception thrown when the MediathekView API response cannot be parsed or deserialized.
/// </summary>
public class MediathekParsingException : MediathekException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekParsingException"/> class.
    /// </summary>
    public MediathekParsingException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekParsingException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MediathekParsingException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediathekParsingException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public MediathekParsingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
