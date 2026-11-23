using UnityEngine;

/// <summary>
/// The shim between an in-world sound source (steps, radio, digging) and the visual-cue overlay.
/// When accessibility mode "visual cues for sound" is on, calling Emit raises SoundCueRaised with
/// the direction from the hero to this source and the normalised loudness. With the mode off, Emit
/// does nothing, so adding this component never changes the game for hearing players.
///
/// Attach it next to any AudioSource worth seeing — it does not read audio itself, because the game
/// already knows where its sounds come from (guard controllers, radios, dig points). Each system
/// calls Emit when it plays a cue that matters to the player.
/// </summary>
public class VisualCueForSound : MonoBehaviour
{
    [Tooltip("The event the overlay listens to. Assign the same asset on every emitter and on the overlay")]
    [SerializeField] private SoundCueRaisedEvent soundCueRaised;

    [Tooltip("The hero the chevron points away from. Optional; without a hero the raw world direction is used")]
    [SerializeField] private Transform hero;

    [Tooltip("A short label the overlay can show next to the chevron (for example \"steps\" or \"radio\")")]
    [SerializeField] private string label;

    [Tooltip("If true, Emit is ignored while the mode is off. If false, the event is always raised and the overlay decides")]
    [SerializeField] private bool respectModeToggle = true;

    /// <summary>Raises a cue for a sound that just played here, with loudness in 0..1.</summary>
    public void Emit(float loudness)
    {
        if (soundCueRaised == null)
        {
            return;
        }
        if (respectModeToggle && !AccessibilitySettings.Current.VisualCuesForSound)
        {
            return;
        }
        Vector2 direction = hero != null
            ? (Vector2)(transform.position - hero.position)
            : (Vector2)transform.position;
        soundCueRaised.Raise(new SoundCue(direction, loudness, label));
    }

    /// <summary>Same as Emit, but the caller gives the label so one emitter can report many sounds.</summary>
    public void Emit(float loudness, string overrideLabel)
    {
        if (soundCueRaised == null)
        {
            return;
        }
        if (respectModeToggle && !AccessibilitySettings.Current.VisualCuesForSound)
        {
            return;
        }
        Vector2 direction = hero != null
            ? (Vector2)(transform.position - hero.position)
            : (Vector2)transform.position;
        soundCueRaised.Raise(new SoundCue(direction, loudness, overrideLabel));
    }
}
