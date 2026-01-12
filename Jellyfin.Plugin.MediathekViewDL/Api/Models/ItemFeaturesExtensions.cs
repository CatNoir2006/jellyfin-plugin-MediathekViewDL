namespace Jellyfin.Plugin.MediathekViewDL.Api.Models;

/// <summary>
/// Extensions for <see cref="ItemFeatures"/>.
/// </summary>
public static class ItemFeaturesExtensions
{
    /// <summary>
    /// Creates <see cref="ItemFeatures"/> from boolean values.
    /// </summary>
    /// <param name="isAudioDesc">Whether it is audio description.</param>
    /// <param name="isSignLang">Whether it is sign language.</param>
    /// <param name="isHardOfHearing">Whether it is hard of hearing.</param>
    /// <returns>The combined features.</returns>
    public static ItemFeatures FromBool(bool isAudioDesc, bool isSignLang, bool isHardOfHearing)
    {
        ItemFeatures flags = ItemFeatures.None;
        if (isAudioDesc)
        {
            flags |= ItemFeatures.IsAudioDescription;
        }

        if (isSignLang)
        {
            flags |= ItemFeatures.IsSignLanguage;
        }

        if (isHardOfHearing)
        {
            flags |= ItemFeatures.IsHardOfHearing;
        }

        return flags;
    }

    /// <summary>
    /// Adds a feature.
    /// </summary>
    /// <param name="core">The core features.</param>
    /// <param name="flag">The feature to add.</param>
    /// <returns>The combined features.</returns>
    public static ItemFeatures AddFlag(this ItemFeatures core, ItemFeatures flag)
    {
        return core | flag;
    }

    /// <summary>
    /// Removes a feature.
    /// </summary>
    /// <param name="core">The core features.</param>
    /// <param name="flag">The feature to remove.</param>
    /// <returns>The combined features.</returns>
    public static ItemFeatures RemoveFlag(this ItemFeatures core, ItemFeatures flag)
    {
        return core & ~flag;
    }
}
