namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Represents the match information between a local file group and an API result.
/// </summary>
public record AdoptionMatch
{
    /// <summary>
    /// Gets the identifier of the matched item in the external API.
    /// </summary>
    public required string ApiId { get; init; }

    /// <summary>
    /// Gets the title of the matched item in the external API.
    /// </summary>
    public string? ApiTitle { get; init; }

    /// <summary>
    /// Gets the original video URL, if found (e.g., in a .txt info file).
    /// </summary>
    public string? VideoUrl { get; init; }

    /// <summary>
    /// Gets the match confidence (e.g., in percentage from 0 to 100).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets a value indicating whether the match is confirmed (e.g., already exists in the download history).
    /// </summary>
    public bool IsConfirmed { get; init; }
}
