using UnityEngine;

/// <summary>
/// Owns the shared AccessibilitySettings at runtime: loads from PlayerPrefs on Awake, and writes
/// back every time the settingsChanged event fires. Other systems read AccessibilitySettings.Current;
/// they do not need a reference to the manager. Static Instance is kept so the pause menu and the
/// title screen can reach the same object from code.
/// </summary>
[DefaultExecutionOrder(AccessibilityManager.ExecutionOrder)]
public class AccessibilityManager : MonoBehaviour
{
    /// <summary>Runs before the other scripts, so the saved settings are loaded when they wake up.</summary>
    public const int ExecutionOrder = -100;

    [Tooltip("The authored settings asset. Its values are the starting defaults; PlayerPrefs override them on Load")]
    [SerializeField] private AccessibilitySettings settings;

    [Tooltip("If true, the manager survives scene loads. Keep on for a game-wide menu")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    public static AccessibilityManager Instance { get; private set; }

    /// <summary>The settings object used right now. Falls back to AccessibilitySettings.Current if none is assigned.</summary>
    public AccessibilitySettings Settings => settings != null ? settings : AccessibilitySettings.Current;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Two managers in the scene would double-save. Keep the first one, drop the newcomer.
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (settings == null)
        {
            // Nothing in the Inspector. Use the shared runtime instance, which loads itself.
            settings = AccessibilitySettings.Current;
        }
        else
        {
            settings.Load();
            AccessibilitySettings.OverrideCurrent(settings);
        }
    }

    private void OnEnable()
    {
        if (Settings.SettingsChanged != null)
        {
            Settings.SettingsChanged.Subscribe(OnSettingsChanged);
        }
    }

    private void OnDisable()
    {
        if (Settings.SettingsChanged != null)
        {
            Settings.SettingsChanged.Unsubscribe(OnSettingsChanged);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSettingsChanged()
    {
        Settings.Save();
    }
}
