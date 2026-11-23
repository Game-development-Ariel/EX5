using UnityEngine;

/// <summary>
/// On the opening scene's root object. On Start it reads the content intensity: if skipOpening is on,
/// it raises skipToTutorialStartEvent so the game jumps straight to the tutorial start (the cell).
/// If skipOpening is off, nothing happens here, and the opening plays normally (in Implied or Direct).
/// </summary>
public class OpeningSkipRouter : MonoBehaviour
{
    [Tooltip("The intensity settings. Falls back to ContentIntensityManager.Current if empty")]
    [SerializeField] private ContentIntensity intensity;

    [Header("Raises")]
    [Tooltip("Raised on Start when skipOpening is true. Listeners load the tutorial scene")]
    [SerializeField] private GameEvent skipToTutorialStartEvent;

    /// <summary>True after Start decided to skip and raised the event.</summary>
    public bool Skipped { get; private set; }

    private void Start()
    {
        ContentIntensity current = intensity != null ? intensity : ContentIntensityManager.Current;
        if (current == null || !current.SkipOpening)
        {
            return;
        }
        Skipped = true;
        if (skipToTutorialStartEvent != null)
        {
            skipToTutorialStartEvent.Raise();
        }
    }
}
