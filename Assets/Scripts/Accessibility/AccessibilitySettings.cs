using UnityEngine;

/// <summary>
/// The player's accessibility choices, saved as an asset so any system can read them without knowing
/// the manager. Each field is a separate toggle, and each degrades gracefully when the feature it
/// supports is missing. The defaults match the "silent, dark, diegetic" feel of the game (section 2.2,
/// hope-as-body): no screen text unless the player asks for it, no colour changes, no help, with
/// subtitles on because they are an ethical baseline (section 2.3, exception for accessibility warnings).
/// The static Current is loaded from PlayerPrefs on first access, so edit-mode code and systems that
/// start before the manager can still read the same values.
/// </summary>
[CreateAssetMenu(menuName = "Tunnel/Accessibility Settings")]
public class AccessibilitySettings : ScriptableObject
{
    /// <summary>The highest brightness floor a player can ask for. The slider goes from 0 to this.</summary>
    public const float MaxMinimumBrightness = 0.3f;

    private const string PrefPrefix = "LITT.Accessibility.";

    private static AccessibilitySettings currentCache;

    [Tooltip("Shows a soft chevron at the edge of the screen for important sounds (steps, radio, digging), with direction and loudness")]
    [SerializeField] private bool visualCuesForSound;

    [Tooltip("Boosts the contrast of the picture and the glow of highlighted objects, so the dark tunnel stays readable for low-vision players")]
    [SerializeField] private bool highContrast;

    [Tooltip("Adds a shape tag beside colour cues (calm=circle, worry=square, despair=triangle), so mood is not carried by colour alone")]
    [SerializeField] private bool colorblindFriendly;

    [Tooltip("Lets the player tap a key many times instead of holding it, for players with motor limits that make long holds hard")]
    [SerializeField] private bool tapInsteadOfHold;

    [Tooltip("Turns flashing lights and alarms into steady colours and soft fades, for players with photosensitive epilepsy")]
    [SerializeField] private bool reducedFlashing;

    [Tooltip("Removes every camera shake, so players with motion sensitivity can play without nausea")]
    [SerializeField] private bool noScreenShake;

    [Tooltip("Shows subtitles for voice lines and important sounds. Default on: subtitles are an ethical baseline, not a game feedback")]
    [SerializeField] private bool subtitlesEnabled = true;

    [Tooltip("The darkest the picture is allowed to get, from 0 (as dark as the design wants) to 0.3 (never fully black)")]
    [Range(0f, 0.3f)]
    [SerializeField] private float minimumBrightness;

    [Tooltip("Raised after any setting changes, so every listener can refresh at once")]
    [SerializeField] private GameEvent settingsChanged;

    /// <summary>The single settings asset the whole game reads. Loaded from PlayerPrefs on first access.</summary>
    public static AccessibilitySettings Current
    {
        get
        {
            if (currentCache == null)
            {
                currentCache = CreateInstance<AccessibilitySettings>();
                currentCache.name = "Accessibility (runtime)";
                currentCache.Load();
            }
            return currentCache;
        }
    }

    /// <summary>Used only by tests and the manager to replace the shared instance.</summary>
    public static void OverrideCurrent(AccessibilitySettings replacement)
    {
        currentCache = replacement;
    }

    public bool VisualCuesForSound => visualCuesForSound;
    public bool HighContrast => highContrast;
    public bool ColorblindFriendly => colorblindFriendly;
    public bool TapInsteadOfHold => tapInsteadOfHold;
    public bool ReducedFlashing => reducedFlashing;
    public bool NoScreenShake => noScreenShake;
    public bool SubtitlesEnabled => subtitlesEnabled;
    public float MinimumBrightness => minimumBrightness;
    public GameEvent SettingsChanged => settingsChanged;

    /// <summary>Changes one toggle and tells every listener. Named Set so the manager can call it from a menu.</summary>
    public void Set(bool newVisualCuesForSound, bool newHighContrast, bool newColorblindFriendly,
        bool newTapInsteadOfHold, bool newReducedFlashing, bool newNoScreenShake,
        bool newSubtitlesEnabled, float newMinimumBrightness)
    {
        visualCuesForSound = newVisualCuesForSound;
        highContrast = newHighContrast;
        colorblindFriendly = newColorblindFriendly;
        tapInsteadOfHold = newTapInsteadOfHold;
        reducedFlashing = newReducedFlashing;
        noScreenShake = newNoScreenShake;
        subtitlesEnabled = newSubtitlesEnabled;
        minimumBrightness = Mathf.Clamp(newMinimumBrightness, 0f, MaxMinimumBrightness);
        RaiseChanged();
    }

    /// <summary>Tells every listener a value has changed. Public so menu code can call it after a direct edit.</summary>
    public void RaiseChanged()
    {
        if (settingsChanged != null)
        {
            settingsChanged.Raise();
        }
    }

    /// <summary>Reads every toggle from PlayerPrefs, with the defaults above as fallback.</summary>
    public void Load()
    {
        visualCuesForSound = GetBool(nameof(visualCuesForSound), visualCuesForSound);
        highContrast = GetBool(nameof(highContrast), highContrast);
        colorblindFriendly = GetBool(nameof(colorblindFriendly), colorblindFriendly);
        tapInsteadOfHold = GetBool(nameof(tapInsteadOfHold), tapInsteadOfHold);
        reducedFlashing = GetBool(nameof(reducedFlashing), reducedFlashing);
        noScreenShake = GetBool(nameof(noScreenShake), noScreenShake);
        subtitlesEnabled = GetBool(nameof(subtitlesEnabled), subtitlesEnabled);
        minimumBrightness = Mathf.Clamp(PlayerPrefs.GetFloat(PrefPrefix + nameof(minimumBrightness), minimumBrightness), 0f, MaxMinimumBrightness);
    }

    /// <summary>Writes every toggle to PlayerPrefs. Called by the manager each time a value changes.</summary>
    public void Save()
    {
        SetBool(nameof(visualCuesForSound), visualCuesForSound);
        SetBool(nameof(highContrast), highContrast);
        SetBool(nameof(colorblindFriendly), colorblindFriendly);
        SetBool(nameof(tapInsteadOfHold), tapInsteadOfHold);
        SetBool(nameof(reducedFlashing), reducedFlashing);
        SetBool(nameof(noScreenShake), noScreenShake);
        SetBool(nameof(subtitlesEnabled), subtitlesEnabled);
        PlayerPrefs.SetFloat(PrefPrefix + nameof(minimumBrightness), minimumBrightness);
        PlayerPrefs.Save();
    }

    private static bool GetBool(string key, bool fallback)
    {
        // PlayerPrefs has no bool, so 0/1 stands in. A missing key returns the fallback unchanged.
        return PlayerPrefs.GetInt(PrefPrefix + key, fallback ? 1 : 0) != 0;
    }

    private static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(PrefPrefix + key, value ? 1 : 0);
    }
}
