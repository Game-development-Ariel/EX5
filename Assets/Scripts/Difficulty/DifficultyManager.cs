using UnityEngine;

/// <summary>
/// Loads the difficulty profile at the start of the level based on what the player picked in the main menu
/// (saved in PlayerPrefs under "LitT.Difficulty"). Exposes the chosen profile through a static property so
/// every other system reads one source of truth, and raises the DifficultyProfileLoaded event so systems
/// that need to initialize themselves (reset counters, pick cameras) can listen.
/// </summary>
[DefaultExecutionOrder(DifficultyManager.ExecutionOrder)]
public class DifficultyManager : MonoBehaviour
{
    /// <summary>Runs before the other scripts, so Current is ready when they wake up.</summary>
    public const int ExecutionOrder = -100;

    // The tutorial's memories were tuned around this value. A profile with another value scales them.
    private const float BaselineMemoryHope = 15f;

    /// <summary>The PlayerPrefs key used by the main menu to remember the player's choice.</summary>
    public const string DifficultyPrefsKey = "LitT.Difficulty";

    [Tooltip("Used only when PlayerPrefs has no saved choice. Set in the Inspector to the Medium asset")]
    [SerializeField] private DifficultyProfile defaultProfile;

    [Tooltip("The live config. Its numbers replace the profile's when the player picks a level. Optional")]
    [SerializeField] private LiveConfigLoader liveConfig;

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

    /// <summary>
    /// Called by the start menu when the player picks a level. Saves the choice, loads its profile
    /// and raises profileLoaded again, so every listener starts over with the new numbers.
    /// </summary>
    public void Choose(Difficulty difficulty)
    {
        PlayerPrefs.SetString(DifficultyPrefsKey, difficulty.ToString());
        PlayerPrefs.Save();
        CurrentAsset = LoadProfile();
        Current = CurrentAsset;
        profileLoaded?.Raise();
    }

    /// <summary>
    /// The hope a memory gives under the current difficulty. Each memory keeps its own tuned value,
    /// and the profile's MemoryHopeValue scales it (15 is the baseline, so Hard's 10 gives two thirds).
    /// </summary>
    public static int ScaleMemoryHope(int tunedValue)
    {
        if (Current == null)
        {
            return tunedValue;
        }
        return Mathf.RoundToInt(tunedValue * Current.MemoryHopeValue / BaselineMemoryHope);
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
        DifficultyProfile profile = BuiltInProfile();
        if (liveConfig == null)
        {
            return profile;
        }

        // A copy, so the live numbers never get written into the asset on disk.
        profile = Instantiate(profile);
        profile.UseLiveNumbers(liveConfig.Config.NumbersFor(profile.Difficulty));
        return profile;
    }

    private DifficultyProfile BuiltInProfile()
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
