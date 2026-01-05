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

namespace Jellyfin.Plugin.MediathekViewDL.Services.Downloading.Clients;

/// <summary>
/// Service for handling ffmpeg operations.
/// </summary>
public class FFmpegService : IFFmpegService
{
    private readonly ILogger<FFmpegService> _logger;
    private readonly IMediaEncoder _mediaEncoder;
    private readonly IStrmValidationService _strmValidationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FFmpegService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mediaEncoder">The MediaEncoder.</param>
    /// <param name="strmValidationService">The StrmValidationService.</param>
    public FFmpegService(ILogger<FFmpegService> logger, IMediaEncoder mediaEncoder, IStrmValidationService strmValidationService)
    {
        _logger = logger;
        _mediaEncoder = mediaEncoder;
        _strmValidationService = strmValidationService;
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
        var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
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
    public async Task<bool> ExtractAudioFromWebAsync(string videoUrl, string outputAudioPath, string languageCode, bool setOriginalLanguageTag, bool isAudioDescription, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _strmValidationService.ValidateUrlAsync(videoUrl, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogError("URL validation failed for '{Url}'", videoUrl);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "URL validation threw exception for '{Url}'", videoUrl);
            return false;
        }

        _logger.LogInformation("Extracting audio from '{Input}' to '{Output}' with language '{Lang}'", videoUrl, outputAudioPath, languageCode);
        if (string.IsNullOrWhiteSpace(_mediaEncoder.EncoderPath))
        {
            _logger.LogError("FFmpeg encoder path is not configured.");
            return false;
        }

        // Build ffmpeg arguments
        var args = new List<string>
        {
            "-i",
            videoUrl,
            "-vn",
            "-acodec",
            "copy",
            "-metadata:s:a:0",
            $"language={languageCode}"
        };

        var dispositions = new List<string>();

        if (setOriginalLanguageTag)
        {
            dispositions.Add("original");
        }

        if (isAudioDescription)
        {
            dispositions.Add("visual_impaired");
        }

        if (dispositions.Count > 0)
        {
            args.Add("-disposition:a:0");
            args.Add(string.Join("+", dispositions));
        }

        args.Add("-f");
        args.Add("matroska");
        args.Add("-y"); // Force overwrite Temp Path.
        args.Add(outputAudioPath);

        // Set up the process start info
        var startInfo = new ProcessStartInfo
        {
            FileName = _mediaEncoder.EncoderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Add arguments to the process
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process();
        process.StartInfo = startInfo;

        var onExitHandler = GetProcessExitHandler(process);
        try
        {
            process.Start();

            AppDomain.CurrentDomain.ProcessExit += onExitHandler;

            // Registriere den Abbruch des Tokens, um den Prozess hart zu killen
            using var registration = cancellationToken.Register(() =>
            {
                KillProcess(process);
            });

            var errorReadTask = process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var error = await errorReadTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("ffmpeg process failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return false;
            }

            _logger.LogInformation("Successfully extracted audio for '{Output}'", outputAudioPath);
            return true;
        }
        finally
        {
            AppDomain.CurrentDomain.ProcessExit -= onExitHandler;
            KillProcess(process);
        }
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

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
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
            if (double.TryParse(videoStream.Duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var streamDuration))
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

    /// <inheritdoc />
    public async Task<bool> DownloadM3U8Async(string url, string outputPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading M3U8 stream from '{Url}' to '{Output}'", url, outputPath);
        if (string.IsNullOrWhiteSpace(_mediaEncoder.EncoderPath))
        {
            _logger.LogError("FFmpeg encoder path is not configured.");
            return false;
        }

        // Build ffmpeg arguments for downloading HLS stream
        // -protocol_whitelist file,http,https,tcp,tls: Allow necessary protocols
        var args = new List<string>
        {
            "-protocol_whitelist", "file,http,https,tcp,tls",
            "-i", url,
            "-c", "copy",
            "-bsf:a", "aac_adtstoasc", // Often needed for converting ADTS AAC to MP4
            "-y",
            outputPath
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = _mediaEncoder.EncoderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process();
        process.StartInfo = startInfo;

        var onExitHandler = GetProcessExitHandler(process);
        try
        {
            process.Start();

            AppDomain.CurrentDomain.ProcessExit += onExitHandler;

            using var registration = cancellationToken.Register(() =>
            {
                KillProcess(process);
            });

            // FFmpeg writes progress and logs to stderr
            var errorReadTask = process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            var error = await errorReadTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("ffmpeg process failed with exit code {ExitCode}. Error: {Error}", process.ExitCode, error);
                return false;
            }

            _logger.LogInformation("Successfully downloaded M3U8 stream to '{Output}'", outputPath);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Download of M3U8 stream cancelled.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading M3U8 stream from '{Url}'", url);
            return false;
        }
        finally
        {
            AppDomain.CurrentDomain.ProcessExit -= onExitHandler;
            KillProcess(process);
        }
    }

    private EventHandler GetProcessExitHandler(Process process)
    {
        return (sender, e) =>
        {
            KillProcess(process);
        };
    }

    private void KillProcess(Process process)
    {
        try
        {
            if (process is { HasExited: false })
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to kill ffmpeg process.");
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
