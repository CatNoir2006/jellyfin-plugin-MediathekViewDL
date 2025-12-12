using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Services.Media;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.MediathekViewDL.Services.Metadata;

/// <summary>
/// Default implementation of <see cref="INfoService"/>.
/// </summary>
public class NfoService : INfoService
{
    private readonly ILogger<NfoService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NfoService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public NfoService(ILogger<NfoService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void CreateNfo(ResultItem item, VideoInfo? videoInfo, string videoFilePath)
    {
        try
        {
            var nfoPath = Path.ChangeExtension(videoFilePath, ".nfo");
            _logger.LogInformation("Creating NFO file at {Path}", nfoPath);

            var root = new XElement("episodedetails");

            // Title
            var title = !string.IsNullOrWhiteSpace(videoInfo?.Title) ? videoInfo.Title : item.Title;
            if (!string.IsNullOrWhiteSpace(title))
            {
                root.Add(new XElement("title", title));
            }

            // Show Title (Topic)
            if (!string.IsNullOrWhiteSpace(item.Topic))
            {
                root.Add(new XElement("showtitle", item.Topic));
            }

            // Plot / Description
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                root.Add(new XElement("plot", item.Description));
            }

            // Season / Episode
            if (videoInfo != null)
            {
                if (videoInfo.SeasonNumber.HasValue)
                {
                    root.Add(new XElement("season", videoInfo.SeasonNumber.Value));
                }

                if (videoInfo.EpisodeNumber.HasValue)
                {
                    root.Add(new XElement("episode", videoInfo.EpisodeNumber.Value));
                }
            }

            // Date Added
            // Timestamp is unix epoch
            try
            {
                // Format: yyyy-MM-dd HH:mm:ss
                var dateAdded = DateTimeOffset.FromUnixTimeSeconds(item.Timestamp).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                root.Add(new XElement("dateadded", dateAdded));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse timestamp {Timestamp} for NFO generation", item.Timestamp);
            }

            // Studio / Channel
            if (!string.IsNullOrWhiteSpace(item.Channel))
            {
                root.Add(new XElement("studio", item.Channel));
            }

            // Unique ID
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                // default to default provider
                root.Add(new XElement("uniqueid", new XAttribute("type", "mediathekview"), item.Id));
            }

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                root);

            using var stream = new FileStream(nfoPath, FileMode.Create, FileAccess.Write, FileShare.None);
            doc.Save(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating NFO file for {Path}", videoFilePath);
        }
    }
}
