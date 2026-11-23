/// <summary>
/// The one way every system asks "what is this parameter right now?". A plain profile returns its own fields,
/// a modulated profile multiplies them by the dynamic layer. Systems don't know which kind they hold,
/// so no code is scattered across the game to compute effective values.
/// </summary>
public interface IDifficultyProfile
{
    /// <summary>The underlying preset this profile is based on.</summary>
    Difficulty Difficulty { get; }

    // Guards -----------------------------------------------------------------

    float GuardCheckMinSeconds { get; }
    float GuardCheckMaxSeconds { get; }
    float GuardPatrolCycleSeconds { get; }
    float GuardSecondPatrolCycleSeconds { get; }
    int Stage2GuardCount { get; }

    bool RandomListenPauseEnabled { get; }
    float RandomListenPauseProbability { get; }
    float RandomListenPauseDurationSeconds { get; }
    float RandomListenPauseIntervalMinSeconds { get; }
    float RandomListenPauseIntervalMaxSeconds { get; }

    // Cameras ----------------------------------------------------------------

    float CameraConeDegrees { get; }
    int CameraCount { get; }
    bool RotatingCameraEnabled { get; }
    float RotatingCameraPeriodSeconds { get; }

    // Hope -------------------------------------------------------------------

    int StartingHope { get; }
    int HopeLostWhenCaught { get; }
    float HopeNaturalDecayPerMinute { get; }
    int MemoryHopeValue { get; }

    // Dig --------------------------------------------------------------------

    float DigMetersPerSecond { get; }
    int DigNoiseCorridors { get; }
    bool HasContinuousDigThreshold { get; }
    float ContinuousDigThresholdSeconds { get; }

    // Flashlight -------------------------------------------------------------

    float BatterySeconds { get; }

    // Journal ----------------------------------------------------------------

    bool JournalHintsEnabled { get; }
    int JournalHintCapturesRequired { get; }

    /// <summary>
    /// Reads any parameter by its property name, like "GuardCheckMinSeconds". Returns the effective value.
    /// Used by code paths that need a value chosen from a string, like the modulator's multipliers.
    /// </summary>
    float CurrentValue(string name);
}
