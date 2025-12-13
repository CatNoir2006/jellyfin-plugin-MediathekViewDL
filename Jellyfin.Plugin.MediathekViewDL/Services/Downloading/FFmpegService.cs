using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.MediathekViewDL.Services.Library;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading;

/// <summary>
/// Service for handling ffmpeg operations.
/// </summary>
public class FFmpegService : IFFmpegService
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<LocalMediaInfo?> GetMediaInfoAsync(string urlOrPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_mediaEncoder.ProbePath))
        {
            _logger.LogError("FFmpeg probe path is not configured.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(urlOrPath))
        {
            _logger.LogError("URL or path cannot be null or empty.");
            return null;
        }

        _logger.LogDebug("Probing media info for '{UrlOrPath}' ", urlOrPath);

        // Arguments to get video stream info in JSON format, minimal probing
        string[] args =
        [
            "-v", "error",
            "-select_streams", "v:0", // Select the first video stream
            "-show_entries", "stream=width,height,duration:format=duration,size",
            "-of", "json",
            urlOrPath
        ];

        var startInfo = new ProcessStartInfo
        {
            FileName = _mediaEncoder.ProbePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process();
        process.StartInfo = startInfo;

        try
        {
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            string error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                _logger.LogWarning("ffprobe process failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return null;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                _logger.LogWarning("ffprobe returned empty output for '{UrlOrPath}'.", urlOrPath);
                return null;
            }

            var ffprobeResult = JsonSerializer.Deserialize<FfprobeOutput>(output);

            if (ffprobeResult?.Streams == null || ffprobeResult.Streams.Count == 0)
            {
                _logger.LogWarning("No video stream found for '{UrlOrPath}' or JSON parsing failed.", urlOrPath);
                return null;
            }

            var videoStream = ffprobeResult.Streams![0];
            var format = ffprobeResult.Format;

            TimeSpan? duration = null;
            if (double.TryParse((string?)videoStream.Duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var streamDuration))
            {
                duration = TimeSpan.FromSeconds(streamDuration);
            }
            else if (double.TryParse(format?.Duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var formatDuration))
            {
                duration = TimeSpan.FromSeconds(formatDuration);
            }

            long? fileSize = null;
            if (long.TryParse(format?.Size, out var formatSize))
            {
                fileSize = formatSize;
            }
            else if (File.Exists(urlOrPath))
            {
                // Fallback for local files if ffprobe doesn't provide size
                try
                {
                    fileSize = new FileInfo(urlOrPath).Length;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get file size for local file '{UrlOrPath}'.", urlOrPath);
                }
            }

            return new LocalMediaInfo
            {
                FilePath = urlOrPath,
                Width = videoStream.Width,
                Height = videoStream.Height,
                Duration = duration,
                FileSize = fileSize
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ffprobe operation cancelled for '{UrlOrPath}'.", urlOrPath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media info for '{UrlOrPath}': {Message}", urlOrPath, ex.Message);
            return null;
        }
    }

    // Helper classes for JSON deserialization of ffprobe output
    private sealed record FfprobeOutput
    {
        [JsonPropertyName("streams")]
        public List<FfprobeStream>? Streams { get; init; } // Verwenden Sie 'init' für Unveränderlichkeit

        [JsonPropertyName("format")]
        public FfprobeFormat? Format { get; init; }
    }

    private sealed record FfprobeStream
    {
        [JsonPropertyName("width")]
        public int? Width { get; init; }

        [JsonPropertyName("height")]
        public int? Height { get; init; }

        [JsonPropertyName("duration")]
        public string? Duration { get; init; } // ffprobe outputs duration as string "HH:MM:SS.MICROSECONDS"
    }

    private sealed record FfprobeFormat
    {
        [JsonPropertyName("duration")]
        public string? Duration { get; init; } // Can also be in format section

        [JsonPropertyName("size")]
        public string? Size { get; init; } // Size as string
    }
}
