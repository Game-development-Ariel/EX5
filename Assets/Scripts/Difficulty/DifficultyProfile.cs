using UnityEngine;

/// <summary>
/// Every tunable number of one difficulty level, saved as an asset so each level is data and not code.
/// Every system that reacts to difficulty (guards, cameras, hope, dig, flashlight, journal) reads from here
/// and holds no constant of its own, so the DifficultyLinterTest can prove the separation.
/// </summary>
[CreateAssetMenu(menuName = "Tunnel/Difficulty Profile")]
public class DifficultyProfile : ScriptableObject, IDifficultyProfile
{
    [Tooltip("Which preset this profile represents")]
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;

    // Guards -----------------------------------------------------------------

    [Header("Guards")]
    [Tooltip("Shortest seconds between random cell checks by the guard")]
    [Min(1f)]
    [SerializeField] private float guardCheckMinSeconds = 45f;

    [Tooltip("Longest seconds between random cell checks by the guard")]
    [Min(1f)]
    [SerializeField] private float guardCheckMaxSeconds = 60f;

    [Tooltip("Seconds the guard needs to complete one full patrol loop")]
    [Min(1f)]
    [SerializeField] private float guardPatrolCycleSeconds = 14f;

    [Tooltip("Seconds of the second guard's loop in stage 2. Used only when stage2GuardCount is 2")]
    [Min(1f)]
    [SerializeField] private float guardSecondPatrolCycleSeconds = 14f;

    [Tooltip("How many guards walk the stage 2 corridor")]
    [Min(1)]
    [SerializeField] private int stage2GuardCount = 2;

    [Header("Guard Listen Pause")]
    [Tooltip("True if the guard sometimes stops and listens for a brief moment")]
    [SerializeField] private bool randomListenPauseEnabled = false;

    [Tooltip("Chance in [0..1] that a scheduled listen pause actually fires")]
    [Range(0f, 1f)]
    [SerializeField] private float randomListenPauseProbability = 1f;

    [Tooltip("Seconds the guard stands still and listens when the pause fires")]
    [Min(0f)]
    [SerializeField] private float randomListenPauseDurationSeconds = 0.5f;

    [Tooltip("Shortest seconds between scheduled listen pauses")]
    [Min(1f)]
    [SerializeField] private float randomListenPauseIntervalMinSeconds = 20f;

    [Tooltip("Longest seconds between scheduled listen pauses")]
    [Min(1f)]
    [SerializeField] private float randomListenPauseIntervalMaxSeconds = 30f;

    // Cameras ----------------------------------------------------------------

    [Header("Cameras")]
    [Tooltip("The width of each camera's vision cone, in degrees")]
    [Range(1f, 359f)]
    [SerializeField] private float cameraConeDegrees = 60f;

    [Tooltip("How many cameras watch the stage")]
    [Min(0)]
    [SerializeField] private int cameraCount = 5;

    [Tooltip("True if one of the cameras rotates around its axis")]
    [SerializeField] private bool rotatingCameraEnabled = false;

    [Tooltip("Seconds for one full rotation of the rotating camera")]
    [Min(1f)]
    [SerializeField] private float rotatingCameraPeriodSeconds = 8f;

    // Hope -------------------------------------------------------------------

    [Header("Hope")]
    [Tooltip("Hope the player starts the stage with")]
    [Min(0)]
    [SerializeField] private int startingHope = 50;

    [Tooltip("Hope lost on every capture by a guard or camera")]
    [Min(0)]
    [SerializeField] private int hopeLostWhenCaught = 10;

    [Tooltip("Points of hope lost to the slow drain every minute. 0 by design: hope doesn't drop with time. Live Ops may turn it on")]
    [Min(0f)]
    [SerializeField] private float hopeNaturalDecayPerMinute;

    [Tooltip("Hope value one picked-up memory restores")]
    [Min(0)]
    [SerializeField] private int memoryHopeValue = 15;

    // Dig --------------------------------------------------------------------

    [Header("Dig")]
    [Tooltip("Meters added to the tunnel per second of continuous digging")]
    [Min(0.01f)]
    [SerializeField] private float digMetersPerSecond = 0.15f;

    [Tooltip("How many adjacent corridors can hear the dig sound")]
    [Min(0)]
    [SerializeField] private int digNoiseCorridors = 2;

    [Tooltip("True when the profile fires the guard early after too long a single dig")]
    [SerializeField] private bool hasContinuousDigThreshold = false;

    [Tooltip("Seconds of continuous digging that trigger an early guard arrival. Ignored unless the threshold is on")]
    [Min(1f)]
    [SerializeField] private float continuousDigThresholdSeconds = 20f;

    // Flashlight -------------------------------------------------------------

    [Header("Flashlight")]
    [Tooltip("Seconds of flashlight battery for one whole stage")]
    [Min(1f)]
    [SerializeField] private float batterySeconds = 65f;

    // Journal ----------------------------------------------------------------

    [Header("Journal")]
    [Tooltip("True when the journal shows a concrete hint after enough captures")]
    [SerializeField] private bool journalHintsEnabled = false;

    [Tooltip("Captures the journal needs to see before it offers a hint")]
    [Min(1)]
    [SerializeField] private int journalHintCapturesRequired = 2;

    // ------------------------------------------------------------------------

    /// <summary>
    /// Replaces the numbers the live config sets, and keeps the rest. Called on the copy of the profile
    /// that the level plays with, never on the asset.
    /// </summary>
    public void UseLiveNumbers(LiveDifficultyNumbers live)
    {
        if (live.startingHope != LiveDifficultyNumbers.NotSet)
        {
            startingHope = live.startingHope;
        }
        if (live.hopeLostWhenCaught != LiveDifficultyNumbers.NotSet)
        {
            hopeLostWhenCaught = live.hopeLostWhenCaught;
        }
        if (live.hopeNaturalDecayPerMinute >= 0f) // NotSet is negative, and a decay can't be.
        {
            hopeNaturalDecayPerMinute = live.hopeNaturalDecayPerMinute;
        }
    }

    public Difficulty Difficulty => difficulty;

    public float GuardCheckMinSeconds => guardCheckMinSeconds;
    public float GuardCheckMaxSeconds => guardCheckMaxSeconds;
    public float GuardPatrolCycleSeconds => guardPatrolCycleSeconds;
    public float GuardSecondPatrolCycleSeconds => guardSecondPatrolCycleSeconds;
    public int Stage2GuardCount => stage2GuardCount;

    public bool RandomListenPauseEnabled => randomListenPauseEnabled;
    public float RandomListenPauseProbability => randomListenPauseProbability;
    public float RandomListenPauseDurationSeconds => randomListenPauseDurationSeconds;
    public float RandomListenPauseIntervalMinSeconds => randomListenPauseIntervalMinSeconds;
    public float RandomListenPauseIntervalMaxSeconds => randomListenPauseIntervalMaxSeconds;

    public float CameraConeDegrees => cameraConeDegrees;
    public int CameraCount => cameraCount;
    public bool RotatingCameraEnabled => rotatingCameraEnabled;
    public float RotatingCameraPeriodSeconds => rotatingCameraPeriodSeconds;

    public int StartingHope => startingHope;
    public int HopeLostWhenCaught => hopeLostWhenCaught;
    public float HopeNaturalDecayPerMinute => hopeNaturalDecayPerMinute;
    public int MemoryHopeValue => memoryHopeValue;

    public float DigMetersPerSecond => digMetersPerSecond;
    public int DigNoiseCorridors => digNoiseCorridors;
    public bool HasContinuousDigThreshold => hasContinuousDigThreshold;
    public float ContinuousDigThresholdSeconds => continuousDigThresholdSeconds;

    public float BatterySeconds => batterySeconds;

    public bool JournalHintsEnabled => journalHintsEnabled;
    public int JournalHintCapturesRequired => journalHintCapturesRequired;

    /// <summary>
    /// Setter used by the factory and the tests, since the serialized fields are private.
    /// Not called at run time.
    /// </summary>
    public void SetFields(
        Difficulty preset,
        float guardCheckMin, float guardCheckMax,
        float patrolCycle, float secondPatrolCycle, int guardsInStage2,
        bool listenPauseOn, float listenPauseChance, float listenPauseDuration,
        float listenPauseIntervalMin, float listenPauseIntervalMax,
        float cameraCone, int cameras, bool rotatingOn, float rotatingPeriod,
        int startHope, int hopeLostCaught, float hopeDecayPerMinute, int memoryValue,
        float digSpeed, int digNoiseRooms, bool continuousDigOn, float continuousDigSeconds,
        float battery,
        bool journalHintsOn, int hintCaptures)
    {
        difficulty = preset;
        guardCheckMinSeconds = guardCheckMin;
        guardCheckMaxSeconds = guardCheckMax;
        guardPatrolCycleSeconds = patrolCycle;
        guardSecondPatrolCycleSeconds = secondPatrolCycle;
        stage2GuardCount = guardsInStage2;
        randomListenPauseEnabled = listenPauseOn;
        randomListenPauseProbability = listenPauseChance;
        randomListenPauseDurationSeconds = listenPauseDuration;
        randomListenPauseIntervalMinSeconds = listenPauseIntervalMin;
        randomListenPauseIntervalMaxSeconds = listenPauseIntervalMax;
        cameraConeDegrees = cameraCone;
        cameraCount = cameras;
        rotatingCameraEnabled = rotatingOn;
        rotatingCameraPeriodSeconds = rotatingPeriod;
        startingHope = startHope;
        hopeLostWhenCaught = hopeLostCaught;
        hopeNaturalDecayPerMinute = hopeDecayPerMinute;
        memoryHopeValue = memoryValue;
        digMetersPerSecond = digSpeed;
        digNoiseCorridors = digNoiseRooms;
        hasContinuousDigThreshold = continuousDigOn;
        continuousDigThresholdSeconds = continuousDigSeconds;
        batterySeconds = battery;
        journalHintsEnabled = journalHintsOn;
        journalHintCapturesRequired = hintCaptures;
    }

    /// <summary>
    /// Reads a parameter by its property name. The field-by-field switch is on purpose:
    /// the DifficultyLinterTest proves every parameter is listed, and reflection would hide typos.
    /// </summary>
    public float CurrentValue(string name)
    {
        switch (name)
        {
            case nameof(GuardCheckMinSeconds): return GuardCheckMinSeconds;
            case nameof(GuardCheckMaxSeconds): return GuardCheckMaxSeconds;
            case nameof(GuardPatrolCycleSeconds): return GuardPatrolCycleSeconds;
            case nameof(GuardSecondPatrolCycleSeconds): return GuardSecondPatrolCycleSeconds;
            case nameof(Stage2GuardCount): return Stage2GuardCount;
            case nameof(RandomListenPauseProbability): return RandomListenPauseProbability;
            case nameof(RandomListenPauseDurationSeconds): return RandomListenPauseDurationSeconds;
            case nameof(RandomListenPauseIntervalMinSeconds): return RandomListenPauseIntervalMinSeconds;
            case nameof(RandomListenPauseIntervalMaxSeconds): return RandomListenPauseIntervalMaxSeconds;
            case nameof(CameraConeDegrees): return CameraConeDegrees;
            case nameof(CameraCount): return CameraCount;
            case nameof(RotatingCameraPeriodSeconds): return RotatingCameraPeriodSeconds;
            case nameof(StartingHope): return StartingHope;
            case nameof(HopeLostWhenCaught): return HopeLostWhenCaught;
            case nameof(HopeNaturalDecayPerMinute): return HopeNaturalDecayPerMinute;
            case nameof(MemoryHopeValue): return MemoryHopeValue;
            case nameof(DigMetersPerSecond): return DigMetersPerSecond;
            case nameof(DigNoiseCorridors): return DigNoiseCorridors;
            case nameof(ContinuousDigThresholdSeconds): return ContinuousDigThresholdSeconds;
            case nameof(BatterySeconds): return BatterySeconds;
            case nameof(JournalHintCapturesRequired): return JournalHintCapturesRequired;
            default: return 0f;
        }
    }
}
