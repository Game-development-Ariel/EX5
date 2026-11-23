using UnityEngine;

/// <summary>
/// One sound that the visual-cue mode wants to show as a chevron. Direction is in world space, from the
/// hero to the sound; loudness is 0 (silent) to 1 (loudest). Label is the short hint shown with the chevron
/// in the overlay, for example "steps" or "radio". Nothing in this class refers to the UI — it is just data.
/// </summary>
public struct SoundCue
{
    /// <summary>World-space direction from the hero to the sound. Not normalised here on purpose: the overlay normalises for display.</summary>
    public Vector2 Direction;

    /// <summary>Loudness of the sound, from 0 (silent) to 1 (loudest). The overlay maps it to chevron size and alpha.</summary>
    public float Loudness;

    /// <summary>Short label the overlay writes beside the chevron ("steps", "radio", "digging"). Optional.</summary>
    public string Label;

    public SoundCue(Vector2 direction, float loudness, string label = null)
    {
        Direction = direction;
        Loudness = Mathf.Clamp01(loudness);
        Label = label;
    }
}
