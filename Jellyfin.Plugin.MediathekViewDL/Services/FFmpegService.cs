using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service for handling ffmpeg operations.
/// </summary>
public class FFmpegService
{
    private readonly ILogger<FFmpegService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public FFmpegService(ILogger<FFmpegService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts the audio track from a video file and saves it as an MKA file.
    /// </summary>
    /// <param name="tempVideoPath">The path to the temporary input video file.</param>
    /// <param name="outputAudioPath">The path for the output MKA audio file.</param>
    /// <param name="languageCode">The 3-letter language code (e.g., 'eng') to set in the metadata.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the extraction was successful, otherwise false.</returns>
    public async Task<bool> ExtractAudioAsync(string tempVideoPath, string outputAudioPath, string languageCode, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Extracting audio from '{Input}' to '{Output}' with language '{Lang}'", tempVideoPath, outputAudioPath, languageCode);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg", // Todo: Get from jellyfin configuration
                Arguments = $"-i \"{tempVideoPath}\" -vn -acodec copy -metadata:s:a:0 language={languageCode} -y \"{outputAudioPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        string error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            _logger.LogError("ffmpeg process failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
            return false;
        }

        _logger.LogInformation("Successfully extracted audio for '{Output}'", outputAudioPath);
        return true;
    }
}
