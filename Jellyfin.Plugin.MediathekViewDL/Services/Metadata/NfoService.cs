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
    public void CreateNfo(NfoDTO item)
    {
        try
        {
            _logger.LogInformation("Creating NFO file at {Path}", item.FilePath);

            string rootElementName = item.IsEpisode ? "episodedetails" : "movie";
            var root = new XElement(rootElementName);

            // Title
            if (!string.IsNullOrWhiteSpace(item.Title))
            {
                root.Add(new XElement("title", item.Title));
            }

            // Show Title (Topic)
            if (!string.IsNullOrWhiteSpace(item.Show))
            {
                root.Add(new XElement("showtitle", item.Show));
            }

            // Plot / Description
            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                root.Add(new XElement("plot", item.Description));
            }

            // Season
            if (item is { Season: not null, IsEpisode: true })
            {
                root.Add(new XElement("season", item.Season.Value));
            }

            // Episode
            if (item is { Episode: not null, IsEpisode: true })
            {
                root.Add(new XElement("episode", item.Episode.Value));
            }

            // Date Added
            try
            {
                if (item.AirDate.HasValue)
                {
                    // Format: yyyy-MM-dd HH:mm:ss
                    var dateAdded = item.AirDate.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    root.Add(new XElement("dateadded", dateAdded));
                }
            }
            catch
            {
                // ignored
            }

            // Studio / Channel
            if (!string.IsNullOrWhiteSpace(item.Studio))
            {
                // We skip studio as it would prevent Jellyfin from setting this based on TMDB data
                // root.Add(new XElement("studio", item.Studio));
            }

            // Unique ID
            if (!string.IsNullOrWhiteSpace(item.Id) && !string.IsNullOrWhiteSpace(item.IdSource))
            {
                // default to default provider
                root.Add(new XElement("uniqueid", new XAttribute("type", item.IdSource), item.Id));
            }

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                root);

            using var stream = new FileStream(item.FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            doc.Save(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating NFO file for {Title}", item.Title);
        }
    }
}
