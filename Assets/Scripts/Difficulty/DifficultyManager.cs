using UnityEngine;

/// <summary>
/// Loads the difficulty profile at the start of the level based on what the player picked in the main menu
/// (saved in PlayerPrefs under "LitT.Difficulty"). Exposes the chosen profile through a static property so
/// every other system reads one source of truth, and raises the DifficultyProfileLoaded event so systems
/// that need to initialize themselves (reset counters, pick cameras) can listen.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    /// <summary>The PlayerPrefs key used by the main menu to remember the player's choice.</summary>
    public const string DifficultyPrefsKey = "LitT.Difficulty";

    [Tooltip("Used only when PlayerPrefs has no saved choice. Set in the Inspector to the Medium asset")]
    [SerializeField] private DifficultyProfile defaultProfile;

    [Tooltip("Raised after Current is set, so listeners can read the profile and initialize themselves")]
    [SerializeField] private GameEvent profileLoaded;

    /// <summary>The profile all systems read from. Set by DifficultyManager's Awake, null until then.</summary>
    public static IDifficultyProfile Current { get; private set; }

    /// <summary>The underlying asset, for systems that need the concrete profile (for example the modulator wrapping it).</summary>
    public static DifficultyProfile CurrentAsset { get; private set; }

    private void Awake()
    {
        CurrentAsset = LoadProfile();
        Current = CurrentAsset;
    }

    private void Start()
    {
        profileLoaded?.Raise();
    }

    /// <summary>Lets the modulator or a test swap the exposed profile for a wrapped version.</summary>
    public static void Override(IDifficultyProfile profile)
    {
        Current = profile;
    }

    /// <summary>For tests only: clears the static state between runs so one test can't leak into the next.</summary>
    public static void ResetForTests()
    {
        Current = null;
        CurrentAsset = null;
    }

    /// <summary>
    /// Clears Current and CurrentAsset at the start of every run. Unity's domain reload is off in this project,
    /// so static values survive otherwise and would carry one play's difficulty into the next.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Current = null;
        CurrentAsset = null;
    }

    private DifficultyProfile LoadProfile()
    {
        string saved = PlayerPrefs.GetString(DifficultyPrefsKey, string.Empty);
        if (saved == Difficulty.Easy.ToString())
        {
            return DifficultyProfileFactory.CreateEasy();
        }
        if (saved == Difficulty.Hard.ToString())
        {
            return DifficultyProfileFactory.CreateHard();
        }
        if (saved == Difficulty.Medium.ToString())
        {
            return DifficultyProfileFactory.CreateMedium();
        }
        return defaultProfile != null ? defaultProfile : DifficultyProfileFactory.CreateMedium();
    }
}
