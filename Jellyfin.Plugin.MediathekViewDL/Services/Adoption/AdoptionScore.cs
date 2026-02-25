namespace Jellyfin.Plugin.MediathekViewDL.Services.Adoption;

/// <summary>
/// Defines the type of a score component.
/// </summary>
public enum AdoptionScoreType
{
    /// <summary>
    /// A value that contributes to the weighted average.
    /// </summary>
    Value,

    /// <summary>
    /// A multiplier that is applied to the final result.
    /// </summary>
    Multiply
}

/// <summary>
/// Represents a single component of an adoption match score.
/// </summary>
/// <param name="Value">The score value (usually between 0 and 1).</param>
/// <param name="Weight">The weight for the average or the multiplier value.</param>
/// <param name="Type">The type of the score component.</param>
public record AdoptionScore(double Value, double Weight, AdoptionScoreType Type = AdoptionScoreType.Value);
