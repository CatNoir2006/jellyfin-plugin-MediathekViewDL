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
        string[] args = ["-i", tempVideoPath, "-vn", "-acodec", "copy", "-metadata:s:a:0", $"language={languageCode}", "-f", "matroska", "-y", outputAudioPath];
        // Execute ffmpeg
        var res = await ExecuteFFmpegAsync(args, cancellationToken).ConfigureAwait(false);

        return res.ExitCode == 0;
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

        var res = await ExecuteFFmpegAsync(args, cancellationToken).ConfigureAwait(false);
        return res.ExitCode == 0;
    }

    /// <inheritdoc />
    public async Task<LocalMediaInfo?> GetMediaInfoAsync(string urlOrPath, CancellationToken cancellationToken)
    {
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

        var res = await ExecuteFFmpegAsync(args, cancellationToken, true).ConfigureAwait(false);

        if (res.ExitCode != 0)
        {
            return null;
        }

        var result = JsonSerializer.Deserialize<FfprobeOutput>(res.Output);

        if (result?.Streams == null || result.Streams.Count == 0)
        {
            _logger.LogWarning("No video stream found for '{UrlOrPath}' or JSON parsing failed.", urlOrPath);
            return null;
        }

        var videoStream = result.Streams![0];
        var format = result.Format;

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

    /// <inheritdoc />
    public async Task<bool> DownloadM3U8Async(string url, string outputPath, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _strmValidationService.ValidateUrlAsync(url, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogError("URL validation failed for '{Url}'", url);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "URL validation threw exception for '{Url}'", url);
            return false;
        }

        _logger.LogInformation("Downloading M3U8 stream from '{Url}' to '{Output}'", url, outputPath);

        // Build ffmpeg arguments for downloading HLS stream
        // -protocol_whitelist file,http,https,tcp,tls: Allow necessary protocols
        var args = new List<string>
        {
            "-protocol_whitelist",
            "file,http,https,tcp,tls",
            "-i",
            url,
            "-c",
            "copy",
            "-f",
            "matroska",
            "-y",
            outputPath
        };

        var res = await ExecuteFFmpegAsync(args, cancellationToken).ConfigureAwait(false);

        return res.ExitCode == 0;
    }

    /// <inheritdoc />
    public async Task<(int ExitCode, string Output, string Error)> ExecuteFFmpegAsync(IEnumerable<string> args, CancellationToken cancellationToken, bool useProbe = false)
    {
        string? executablePath = useProbe ? _mediaEncoder.ProbePath : _mediaEncoder.EncoderPath;
        string toolName = useProbe ? "FFprobe" : "FFmpeg";

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            _logger.LogError("{ToolName} path is not configured.", toolName);
            return (-1, string.Empty, $"{toolName} path is not configured.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
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

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync(cancellationToken)).ConfigureAwait(false);

            return (process.ExitCode, await outputTask.ConfigureAwait(false), await errorTask.ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("{ToolName} execution cancelled.", toolName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing {ToolName}.", toolName);
            return (-1, string.Empty, ex.Message);
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
