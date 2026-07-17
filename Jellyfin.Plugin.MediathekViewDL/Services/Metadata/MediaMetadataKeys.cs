using System;
using System.Text.Json;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

/// <summary>
/// Constants and helpers for the embedded MediathekViewDL media metadata.
/// </summary>
public static class MediaMetadataKeys
{
    /// <summary>
    /// The metadata key under which the JSON object is stored inside the matroska container.
    /// </summary>
    public const string MetadataKey = "MediathekViewDL";

    /// <summary>
    /// The comment line prefix used for .strm files.
    /// </summary>
    public const string StrmCommentPrefix = "# MediathekViewDL-Metadata: ";

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Serializes the given <see cref="MediaMetadata"/> to a single-line JSON string.
    /// </summary>
    /// <param name="metadata">The metadata to serialize.</param>
    /// <returns>The JSON string.</returns>
    public static string Serialize(MediaMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return JsonSerializer.Serialize(metadata, _serializerOptions);
    }
}
