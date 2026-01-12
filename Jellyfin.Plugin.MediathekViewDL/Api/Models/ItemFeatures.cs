using System;

namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Features of a media item.
/// </summary>
[Flags]
public enum ItemFeatures
{
    /// <summary>
    /// No features.
    /// </summary>
    None = 0,

    /// <summary>
    /// Item has audio description.
    /// </summary>
    IsAudioDescription = 1 << 0,

    /// <summary>
    /// Item has sign language.
    /// </summary>
    IsSignLanguage = 1 << 1,

    /// <summary>
    /// Item is for hard of hearing.
    /// </summary>
    IsHardOfHearing = 1 << 2,
}
