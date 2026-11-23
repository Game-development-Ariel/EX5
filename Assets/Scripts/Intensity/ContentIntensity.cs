using UnityEngine;

/// <summary>
/// The content intensity of the game, saved as an asset so the whole project reads the same values.
/// It holds the player's choice, Implied or Direct, and whether the content warning has already been shown. Anything that needs to react to a change listens to
/// modeChanged. The ContentIntensityManager loads and saves these values from PlayerPrefs.
/// </summary>
[CreateAssetMenu(menuName = "Intensity/Content Intensity")]
public class ContentIntensity : ScriptableObject
{
    [Tooltip("The current intensity. Implied is the default: the hard moments are suggested, not shown")]
    [SerializeField] private IntensityLevel mode = IntensityLevel.Implied;

    [Tooltip("True after the player saw the content warning once")]
    [SerializeField] private bool contentWarningShown;

    [Header("Raises")]
    [Tooltip("Raised whenever the intensity mode changes. The scenes listen and swap variants")]
    [SerializeField] private GameEvent modeChanged;

    /// <summary>The current intensity. Changing it raises modeChanged.</summary>
    public IntensityLevel Mode
    {
        get => mode;
        set
        {
            if (mode == value)
            {
                return;
            }
            mode = value;
            if (modeChanged != null)
            {
                modeChanged.Raise();
            }
        }
    }

    /// <summary>True after the player saw the content warning once.</summary>
    public bool ContentWarningShown
    {
        get => contentWarningShown;
        set => contentWarningShown = value;
    }

    /// <summary>The event raised when the intensity mode changes. Exposed so other scripts can subscribe.</summary>
    public GameEvent ModeChanged => modeChanged;
}
