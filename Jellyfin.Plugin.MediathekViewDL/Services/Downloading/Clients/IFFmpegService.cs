using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;

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
    /// Extracts the audio track from a url and saves it as an MKA file.
    /// </summary>
    /// <param name="videoUrl">The path to the temporary input video file.</param>
    /// <param name="outputAudioPath">The path for the output MKA audio file.</param>
    /// <param name="languageCode">The 3-letter language code (e.g., 'eng') to set in the metadata.</param>
    /// <param name="setOriginalLanguageTag">Whether to tag the audio as original language.</param>
    /// <param name="isAudioDescription">Whether the audio track is an audio description.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    Task<bool> ExtractAudioFromWebAsync(string videoUrl, string outputAudioPath, string languageCode, bool setOriginalLanguageTag, bool isAudioDescription, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the media information (width, height, duration) from a remote URL or local file.
    /// </summary>
    /// <param name="urlOrPath">The URL or file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The media info, or null if it could not be determined.</returns>
    Task<Library.LocalMediaInfo?> GetMediaInfoAsync(string urlOrPath, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads an M3U8 stream and saves it as a local file.
    /// </summary>
    /// <param name="url">The URL of the M3U8 stream.</param>
    /// <param name="outputPath">The path for the output file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the download was successful, otherwise false.</returns>
    Task<bool> DownloadM3U8Async(string url, string outputPath, CancellationToken cancellationToken);

    /// <summary>
    /// Executes FFmpeg with the specified arguments.
    /// </summary>
    /// <param name="args">The arguments to pass to FFmpeg.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="useProbe">If true, uses the ProbePath (ffprobe) instead of EncoderPath (ffmpeg).</param>
    /// <returns>A tuple containing the exit code, standard output, and standard error.</returns>
    Task<(int ExitCode, string Output, string Error)> ExecuteFFmpegAsync(IEnumerable<string> args, CancellationToken cancellationToken, bool useProbe = false);
}
