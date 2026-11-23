using UnityEngine;

/// <summary>
/// Wraps a DifficultyProfile and a DifficultyModulator. Returns the base value times the modulator's
/// multiplier for the named parameter. Non-multiplied parameters pass through unchanged. Systems read
/// this through IDifficultyProfile and can't tell they are reading a modulated value, which is the point:
/// one API for every system, one place that layers the dynamic adjustment.
/// </summary>
public class ModulatedDifficultyProfile : IDifficultyProfile
{
    // When the modulator asks for no change, the multiplier is 1.0.
    private const float NoChange = 1f;

    private readonly DifficultyProfile baseProfile;
    private readonly DifficultyModulator modulator;

    public ModulatedDifficultyProfile(DifficultyProfile baseProfile, DifficultyModulator modulator)
    {
        this.baseProfile = baseProfile;
        this.modulator = modulator;
    }

    public Difficulty Difficulty => baseProfile.Difficulty;

    public float GuardCheckMinSeconds => Scaled(nameof(GuardCheckMinSeconds), baseProfile.GuardCheckMinSeconds);
    public float GuardCheckMaxSeconds => Scaled(nameof(GuardCheckMaxSeconds), baseProfile.GuardCheckMaxSeconds);
    public float GuardPatrolCycleSeconds => Scaled(nameof(GuardPatrolCycleSeconds), baseProfile.GuardPatrolCycleSeconds);
    public float GuardSecondPatrolCycleSeconds => Scaled(nameof(GuardSecondPatrolCycleSeconds), baseProfile.GuardSecondPatrolCycleSeconds);
    public int Stage2GuardCount => Mathf.RoundToInt(Scaled(nameof(Stage2GuardCount), baseProfile.Stage2GuardCount));

    public bool RandomListenPauseEnabled => baseProfile.RandomListenPauseEnabled;
    public float RandomListenPauseProbability => Scaled(nameof(RandomListenPauseProbability), baseProfile.RandomListenPauseProbability);
    public float RandomListenPauseDurationSeconds => Scaled(nameof(RandomListenPauseDurationSeconds), baseProfile.RandomListenPauseDurationSeconds);
    public float RandomListenPauseIntervalMinSeconds => Scaled(nameof(RandomListenPauseIntervalMinSeconds), baseProfile.RandomListenPauseIntervalMinSeconds);
    public float RandomListenPauseIntervalMaxSeconds => Scaled(nameof(RandomListenPauseIntervalMaxSeconds), baseProfile.RandomListenPauseIntervalMaxSeconds);

    public float CameraConeDegrees => Scaled(nameof(CameraConeDegrees), baseProfile.CameraConeDegrees);
    public int CameraCount => Mathf.RoundToInt(Scaled(nameof(CameraCount), baseProfile.CameraCount));
    public bool RotatingCameraEnabled => baseProfile.RotatingCameraEnabled;
    public float RotatingCameraPeriodSeconds => Scaled(nameof(RotatingCameraPeriodSeconds), baseProfile.RotatingCameraPeriodSeconds);

    public int StartingHope => Mathf.RoundToInt(Scaled(nameof(StartingHope), baseProfile.StartingHope));
    public int HopeLostWhenCaught => Mathf.RoundToInt(Scaled(nameof(HopeLostWhenCaught), baseProfile.HopeLostWhenCaught));
    public float HopeNaturalDecayPerMinute => Scaled(nameof(HopeNaturalDecayPerMinute), baseProfile.HopeNaturalDecayPerMinute);
    public int MemoryHopeValue => Mathf.RoundToInt(Scaled(nameof(MemoryHopeValue), baseProfile.MemoryHopeValue));

    public float DigMetersPerSecond => Scaled(nameof(DigMetersPerSecond), baseProfile.DigMetersPerSecond);
    public int DigNoiseCorridors => Mathf.RoundToInt(Scaled(nameof(DigNoiseCorridors), baseProfile.DigNoiseCorridors));
    public bool HasContinuousDigThreshold => baseProfile.HasContinuousDigThreshold;
    public float ContinuousDigThresholdSeconds => Scaled(nameof(ContinuousDigThresholdSeconds), baseProfile.ContinuousDigThresholdSeconds);

    public float BatterySeconds => Scaled(nameof(BatterySeconds), baseProfile.BatterySeconds);

    public bool JournalHintsEnabled => baseProfile.JournalHintsEnabled;
    public int JournalHintCapturesRequired => Mathf.RoundToInt(Scaled(nameof(JournalHintCapturesRequired), baseProfile.JournalHintCapturesRequired));

    /// <summary>Reads any parameter by its property name through the modulator. Same semantics as DifficultyProfile.CurrentValue.</summary>
    public float CurrentValue(string name)
    {
        return baseProfile.CurrentValue(name) * MultiplierFor(name);
    }

    private float Scaled(string name, float baseValue)
    {
        return baseValue * MultiplierFor(name);
    }

    private float MultiplierFor(string name)
    {
        return modulator != null ? modulator.Multiplier(name) : NoChange;
    }
}
