using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

    private static readonly Regex DurationRegex = new Regex(@"Duration:\s+(\d{2}:\d{2}:\d{2}\.\d+)", RegexOptions.Compiled);
    private static readonly Regex TimeRegex = new Regex(@"time=(\d{2}:\d{2}:\d{2}\.\d+)", RegexOptions.Compiled);

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
    public async Task<bool> ExtractAudioAsync(string tempVideoPath, string outputAudioPath, string languageCode, IProgress<double> progress, CancellationToken cancellationToken)
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
        var res = await ExecuteFFmpegAsync(args, cancellationToken, false, progress).ConfigureAwait(false);

        return res.ExitCode == 0;
    }

    /// <inheritdoc />
    public async Task<bool> ExtractAudioFromWebAsync(string videoUrl, string outputAudioPath, string languageCode, bool setOriginalLanguageTag, bool isAudioDescription, IProgress<double> progress, CancellationToken cancellationToken)
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

        var res = await ExecuteFFmpegAsync(args, cancellationToken, false, progress).ConfigureAwait(false);
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

        string actualUrlOrPath = urlOrPath;
        if (urlOrPath.EndsWith(".strm", StringComparison.OrdinalIgnoreCase) && File.Exists(urlOrPath))
        {
            try
            {
                actualUrlOrPath = (await File.ReadAllTextAsync(urlOrPath, cancellationToken).ConfigureAwait(false)).Trim();
                _logger.LogDebug("Read URL from .strm file: {Url}", actualUrlOrPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read .strm file at {Path}", urlOrPath);
                return null;
            }
        }

        _logger.LogDebug("Probing media info for '{UrlOrPath}' ", actualUrlOrPath);

        // Security check: If it's not a verified local file path, it must be a valid URL
        if (!File.Exists(actualUrlOrPath))
        {
            try
            {
                if (!await _strmValidationService.ValidateUrlAsync(actualUrlOrPath, cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogError("Validation failed for '{Input}' (not a local file and URL validation failed)", actualUrlOrPath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation threw exception for '{Input}'", actualUrlOrPath);
                return null;
            }
        }

        // Arguments to get stream info in JSON format
        // We remove -select_streams v:0 to support audio-only files
        string[] args =
        [
            "-v", "error",
            "-show_entries", "stream=width,height,duration,codec_type:format=duration,size",
            "-of", "json",
            actualUrlOrPath
        ];

        var res = await ExecuteFFmpegAsync(args, cancellationToken, true).ConfigureAwait(false);

        if (res.ExitCode != 0)
        {
            _logger.LogWarning("ffprobe failed for '{UrlOrPath}' with exit code {ExitCode}. Error: {Error}", actualUrlOrPath, res.ExitCode, res.Error);
            return null;
        }

        if (string.IsNullOrWhiteSpace(res.Output))
        {
            _logger.LogWarning("ffprobe returned empty output for '{UrlOrPath}'.", actualUrlOrPath);
            return null;
        }

        var result = JsonSerializer.Deserialize<FfprobeOutput>(res.Output);

        if (result?.Streams == null || result.Streams.Count == 0)
        {
            _logger.LogWarning("No streams found for '{UrlOrPath}' or JSON parsing failed.", actualUrlOrPath);
            return null;
        }

        // Prefer video stream for width/height, but take duration from any stream or format
        var videoStream = result.Streams.FirstOrDefault(s => s.CodecType == "video");
        var firstStream = result.Streams[0];
        var format = result.Format;

        TimeSpan? duration = null;

        // Try to get duration from format first (usually most reliable)
        if (double.TryParse(format?.Duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var formatDuration))
        {
            duration = TimeSpan.FromSeconds(formatDuration);
        }
        else
        {
            // Fallback to streams
            foreach (var stream in result.Streams)
            {
                if (double.TryParse(stream.Duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var streamDuration))
                {
                    duration = TimeSpan.FromSeconds(streamDuration);
                    break;
                }
            }
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
            Width = videoStream?.Width,
            Height = videoStream?.Height,
            Duration = duration,
            FileSize = fileSize
        };
    }

    /// <inheritdoc />
    public async Task<bool> DownloadM3U8Async(string url, string outputPath, IProgress<double> progress, CancellationToken cancellationToken)
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

        var res = await ExecuteFFmpegAsync(args, cancellationToken, false, progress).ConfigureAwait(false);

        return res.ExitCode == 0;
    }

    /// <inheritdoc />
    public async Task<(int ExitCode, string Output, string Error)> ExecuteFFmpegAsync(IEnumerable<string> args, CancellationToken cancellationToken, bool useProbe = false, IProgress<double>? progress = null)
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

            if (progress != null)
            {
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                TimeSpan totalDuration = TimeSpan.Zero;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                        ParseProgress(e.Data, progress, ref totalDuration);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
            }
            else
            {
                var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

                await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync(cancellationToken)).ConfigureAwait(false);

                return (process.ExitCode, await outputTask.ConfigureAwait(false), await errorTask.ConfigureAwait(false));
            }
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

    private void ParseProgress(string line, IProgress<double> progress, ref TimeSpan totalDuration)
    {
        // Check for duration
        if (totalDuration == TimeSpan.Zero)
        {
            var match = DurationRegex.Match(line);
            if (match.Success)
            {
                if (TimeSpan.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var duration))
                {
                    totalDuration = duration;
                }
            }
        }

        // Check for time
        if (totalDuration != TimeSpan.Zero)
        {
            var match = TimeRegex.Match(line);
            if (match.Success)
            {
                if (TimeSpan.TryParse(match.Groups[1].Value, CultureInfo.InvariantCulture, out var currentTime))
                {
                    var percentage = (currentTime.TotalSeconds / totalDuration.TotalSeconds) * 100;
                    if (percentage > 100)
                    {
                        percentage = 100;
                    }

                    progress.Report(percentage);
                }
            }
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

        [JsonPropertyName("codec_type")]
        public string? CodecType { get; init; }
    }

    private sealed record FfprobeFormat
    {
        [JsonPropertyName("duration")]
        public string? Duration { get; init; } // Can also be in format section

        [JsonPropertyName("size")]
        public string? Size { get; init; } // Size as string
    }
}
