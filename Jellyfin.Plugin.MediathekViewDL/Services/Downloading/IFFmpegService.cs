using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Interface for the FFmpegService.
/// </summary>
public interface IFFmpegService
{
    /// <summary>
    /// Extracts the audio track from a video file and saves it as an MKA file.
    /// </summary>
    /// <param name="tempVideoPath">The path to the temporary input video file.</param>
    /// <param name="outputAudioPath">The path for the output MKA audio file.</param>
    /// <param name="languageCode">The 3-letter language code (e.g., 'eng') to set in the metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    Task<bool> ExtractAudioAsync(string tempVideoPath, string outputAudioPath, string languageCode, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the media information (width, height, duration) from a remote URL or local file.
    /// </summary>
    /// <param name="urlOrPath">The URL or file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media info, or null if it could not be determined.</returns>
    Task<Library.LocalMediaInfo?> GetMediaInfoAsync(string urlOrPath, CancellationToken cancellationToken);
}
