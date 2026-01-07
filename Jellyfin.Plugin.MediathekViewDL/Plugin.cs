using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.MediathekViewDL.Api;
using Jellyfin.Plugin.MediathekViewDL.Configuration;
using Jellyfin.Plugin.MediathekViewDL.Services;
using Jellyfin.Plugin.MediathekViewDL.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MediathekViewDL;

/// <summary>
/// The main plugin class.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "Mediathek Downloader";

    /// <inheritdoc />
    public override string Description => "Sucht und lädt Inhalte aus den Mediatheken über die MediathekViewWeb-API.";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("a31b415a-5264-419d-b152-8c8192a54994");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        var prefix = GetType().Namespace;
        yield return
            new PluginPageInfo { Name = Name, EmbeddedResourcePath = prefix + ".Configuration.Web.configPage.html", EnableInMainMenu = true, };
        yield return
            new PluginPageInfo { Name = "MediathekViewDL.js", EmbeddedResourcePath = prefix + ".Configuration.Web.configPage.js" };
    }
}
