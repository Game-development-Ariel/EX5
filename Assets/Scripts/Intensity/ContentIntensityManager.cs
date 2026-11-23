using UnityEngine;

/// <summary>
/// Loads the content intensity settings from PlayerPrefs on Awake, and saves them again when the
/// player changes them. One manager lives in the first scene and the rest of the game reads the
/// static Current. The PlayerPrefs key is "LitT.Intensity" (the mode), and two neighbors next to it
/// keep the skip-opening and content-warning-shown flags.
/// </summary>
public class ContentIntensityManager : MonoBehaviour
{
    /// <summary>The PlayerPrefs key for the chosen intensity mode (stored as an int).</summary>
    public const string ModeKey = "LitT.Intensity";

    /// <summary>The PlayerPrefs key for the skip-opening flag (stored as 0 or 1).</summary>
    public const string SkipOpeningKey = "LitT.Intensity.SkipOpening";

    /// <summary>The PlayerPrefs key for the content-warning-shown flag (stored as 0 or 1).</summary>
    public const string ContentWarningShownKey = "LitT.Intensity.ContentWarningShown";

    [Tooltip("The asset holding the current intensity. All scenes read from the same asset")]
    [SerializeField] private ContentIntensity intensity;

    /// <summary>The current content intensity, shared across the game.</summary>
    public static ContentIntensity Current { get; private set; }

    private void Awake()
    {
        Current = intensity;
        Load();
    }

    private void OnEnable()
    {
        if (intensity != null && intensity.ModeChanged != null)
        {
            intensity.ModeChanged.Subscribe(Save);
        }
    }

    private void OnDisable()
    {
        if (intensity != null && intensity.ModeChanged != null)
        {
            intensity.ModeChanged.Unsubscribe(Save);
        }
    }

    /// <summary>Reads the three values from PlayerPrefs. Missing keys keep the asset's defaults.</summary>
    public void Load()
    {
        if (intensity == null)
        {
            return;
        }
        if (PlayerPrefs.HasKey(ModeKey))
        {
            intensity.Mode = (IntensityLevel)PlayerPrefs.GetInt(ModeKey);
        }
        if (PlayerPrefs.HasKey(SkipOpeningKey))
        {
            intensity.SkipOpening = PlayerPrefs.GetInt(SkipOpeningKey) != 0;
        }
        if (PlayerPrefs.HasKey(ContentWarningShownKey))
        {
            intensity.ContentWarningShown = PlayerPrefs.GetInt(ContentWarningShownKey) != 0;
        }
    }

    /// <summary>Writes the three values to PlayerPrefs. Called on every mode change and on demand.</summary>
    public void Save()
    {
        if (intensity == null)
        {
            return;
        }
        PlayerPrefs.SetInt(ModeKey, (int)intensity.Mode);
        PlayerPrefs.SetInt(SkipOpeningKey, intensity.SkipOpening ? 1 : 0);
        PlayerPrefs.SetInt(ContentWarningShownKey, intensity.ContentWarningShown ? 1 : 0);
        PlayerPrefs.Save();
    }
}
