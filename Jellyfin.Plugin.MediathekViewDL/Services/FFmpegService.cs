using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services;

/// <summary>
/// Service for handling ffmpeg operations.
/// </summary>
public class FFmpegService
{
    private readonly ILogger<FFmpegService> _logger;
    private readonly IMediaEncoder _mediaEncoder;

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mediaEncoder">The MediaEncoder.</param>
    public FFmpegService(ILogger<FFmpegService> logger, IMediaEncoder mediaEncoder)
    {
        _logger = logger;
        _mediaEncoder = mediaEncoder;
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
        if (string.IsNullOrWhiteSpace(_mediaEncoder.EncoderPath))
        {
            _logger.LogError("FFmpeg encoder path is not configured.");
            return false;
        }

        // Build ffmpeg arguments
        string[] args = ["-i", tempVideoPath, "-vn", "-acodec", "copy", "-metadata:s:a:0", $"language={languageCode}", "-y", outputAudioPath];

        // Set up the process start info
        var startInfo = new ProcessStartInfo
        {
            FileName = _mediaEncoder.EncoderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Add arguments to the process
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process();
        process.StartInfo = startInfo;
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
