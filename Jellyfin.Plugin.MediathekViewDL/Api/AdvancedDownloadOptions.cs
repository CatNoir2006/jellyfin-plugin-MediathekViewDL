using Jellyfin.Plugin.MediathekViewDL.Api;

namespace Jellyfin.Plugin.MediathekViewDL.Api
{
    /// <summary>
    /// Advanced download options. Used for manual downloads.
    /// </summary>
    public class AdvancedDownloadOptions
    {
        /// <summary>
        /// Gets the item to download.
        /// </summary>
        public required ResultItem Item { get; init; }

        /// <summary>
        /// Gets the download path.
        /// </summary>
        public required string DownloadPath { get; init; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public required string FileName { get; init; }

        /// <summary>
        /// Gets a value indicating whether to download subtitles.
        /// </summary>
        public bool DownloadSubtitles { get; init; }
    }
}
