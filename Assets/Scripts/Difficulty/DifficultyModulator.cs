using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reads the SkillTracker every 30 seconds and layers a dynamic multiplier on top of the Medium profile.
/// Lives only while the game runs on Medium. Easy and Hard commit to their presets and the modulator is off.
///
/// The spec also forbids changes in three windows: the first 30 seconds of a stage, while scripted sequences play
/// (break point in stage 3, light build-up, stars in stage 4), and while the player is in an uninterrupted dig
/// longer than 15 seconds. The modulator holds the list of scripted sequences by reference, so new sequences only
/// need to be added to the asset list.
/// </summary>
public class DifficultyModulator : MonoBehaviour
{
    // The modulator ticks every 30 seconds, and the first 30 seconds are quiet to let the player settle in.
    private const float ModulationTickSeconds = 30f;
    private const float StageWarmupSeconds = 30f;

    // Thresholds between modes, from section 1.3.
    private const float MercyAtOrBelow = -7f;
    private const float EaseAtOrBelow = -3f;
    private const float TightenAtOrAbove = 3f;
    private const float ChallengeAtOrAbove = 7f;

    // Mercy: cell checks ≈ +15 seconds on a 45..60-second base, camera cone ≈ -10 degrees, next catch loses half hope.
    private const float MercyGuardCheckMultiplier = 1.3f;
    private const float MercyCameraConeMultiplier = 0.85f;
    private const float MercyHopeLostMultiplier = 0.5f;

    // Ease: cell checks ≈ +5 seconds, battery ≈ +10 seconds on a 65-second base.
    private const float EaseGuardCheckMultiplier = 1.1f;
    private const float EaseBatteryMultiplier = 1.15f;

    // Tighten: cell checks ≈ -5 seconds, guard sometimes arrives 3 seconds early, modelled as a shorter patrol cycle.
    private const float TightenGuardCheckMultiplier = 0.9f;
    private const float TightenPatrolCycleMultiplier = 0.95f;

    // Challenge: cell checks ≈ -10 seconds, temporary extra camera modelled as +1 to the camera count.
    private const float ChallengeGuardCheckMultiplier = 0.8f;
    private const float ChallengeCameraCountMultiplier = 1.2f;

    // No multiplier recorded for a parameter means 1.0, no change.
    private const float NoChange = 1f;

    // Dig longer than this (without a break) freezes the modulator, to avoid a betrayal mid-action.
    private const float ContinuousDigFreezeSeconds = 15f;

    [SerializeField] private SkillTracker skillTracker;
    [SerializeField] private DifficultyProfile mediumProfile;
    [SerializeField] private StageStartedEvent stageStarted;

    [Tooltip("Events that mark a scripted sequence starting. The modulator freezes while any of them is active")]
    [SerializeField] private List<GameEvent> scriptedSequenceStarts = new List<GameEvent>();

    [Tooltip("Events that mark a scripted sequence ending. Paired with starts by index; must be the same length")]
    [SerializeField] private List<GameEvent> scriptedSequenceEnds = new List<GameEvent>();

    private readonly Dictionary<string, float> emptyModifiers = new Dictionary<string, float>();
    private readonly Dictionary<ModulationMode, Dictionary<string, float>> modeModifiers = new Dictionary<ModulationMode, Dictionary<string, float>>();

    private Dictionary<string, float> currentModifiers;
    private ModulationMode currentMode = ModulationMode.Default;
    private float stageStartedAt;

    // False until the first stage starts. Not "stageStartedAt < 0": a stage can start at any time, also a negative one in a test.
    private bool stageRunning;
    private int scriptedSequencesActive;
    private float continuousDigSeconds;
    private bool isDigging;

    /// <summary>The mode the modulator was in at the last tick. Exposed for tests and tooling.</summary>
    public ModulationMode CurrentMode => currentMode;

    /// <summary>True when the modulator is active: Medium profile, past warm-up, no scripted sequence, no long dig.</summary>
    public bool IsActive => IsEnabledForDifficulty() && !IsFrozen();

    private void Awake()
    {
        BuildModeTables();
        currentModifiers = modeModifiers[ModulationMode.Default];
    }

    private void OnEnable()
    {
        if (stageStarted != null) stageStarted.Subscribe(OnStageStarted);
        for (int i = 0; i < scriptedSequenceStarts.Count; i++)
        {
            if (scriptedSequenceStarts[i] != null) scriptedSequenceStarts[i].Subscribe(OnScriptedSequenceStarted);
        }
        for (int i = 0; i < scriptedSequenceEnds.Count; i++)
        {
            if (scriptedSequenceEnds[i] != null) scriptedSequenceEnds[i].Subscribe(OnScriptedSequenceEnded);
        }
    }

    private void OnDisable()
    {
        if (stageStarted != null) stageStarted.Unsubscribe(OnStageStarted);
        for (int i = 0; i < scriptedSequenceStarts.Count; i++)
        {
            if (scriptedSequenceStarts[i] != null) scriptedSequenceStarts[i].Unsubscribe(OnScriptedSequenceStarted);
        }
        for (int i = 0; i < scriptedSequenceEnds.Count; i++)
        {
            if (scriptedSequenceEnds[i] != null) scriptedSequenceEnds[i].Unsubscribe(OnScriptedSequenceEnded);
        }
    }

    private void Update()
    {
        if (isDigging)
        {
            continuousDigSeconds += Time.deltaTime;
        }
    }

    /// <summary>Called by the dig system every time a session starts or ends.</summary>
    public void SetDigging(bool digging)
    {
        if (!digging)
        {
            continuousDigSeconds = 0f;
        }
        isDigging = digging;
    }

    /// <summary>Called by the external 30-second ticker (or by a test), reads the score and picks a mode.</summary>
    public void Tick()
    {
        if (!IsEnabledForDifficulty())
        {
            SetMode(ModulationMode.Default);
            return;
        }
        if (IsFrozen())
        {
            return;
        }
        SetMode(PickMode(skillTracker != null ? skillTracker.CurrentScore : 0f));
    }

    /// <summary>Returns the multiplier for a parameter, or 1 when no multiplier is active.</summary>
    public float Multiplier(string name)
    {
        if (!IsEnabledForDifficulty() || IsFrozen())
        {
            return NoChange;
        }
        if (currentModifiers == null)
        {
            return NoChange;
        }
        return currentModifiers.TryGetValue(name, out float value) ? value : NoChange;
    }

    /// <summary>Exposes the modifier table of a mode, for the tests to prove each mode is set up correctly.</summary>
    public IReadOnlyDictionary<string, float> ModifiersFor(ModulationMode mode)
    {
        if (modeModifiers.Count == 0)
        {
            BuildModeTables();
        }
        return modeModifiers.TryGetValue(mode, out Dictionary<string, float> table) ? table : emptyModifiers;
    }

    private bool IsEnabledForDifficulty()
    {
        return mediumProfile != null && mediumProfile.Difficulty == Difficulty.Medium;
    }

    private bool IsFrozen()
    {
        if (!stageRunning)
        {
            return true;
        }
        if (Time.time - stageStartedAt < StageWarmupSeconds)
        {
            return true;
        }
        if (scriptedSequencesActive > 0)
        {
            return true;
        }
        if (isDigging && continuousDigSeconds > ContinuousDigFreezeSeconds)
        {
            return true;
        }
        return false;
    }

    private ModulationMode PickMode(float score)
    {
        if (score <= MercyAtOrBelow) return ModulationMode.Mercy;
        if (score <= EaseAtOrBelow) return ModulationMode.Ease;
        if (score >= ChallengeAtOrAbove) return ModulationMode.Challenge;
        if (score >= TightenAtOrAbove) return ModulationMode.Tighten;
        return ModulationMode.Default;
    }

    private void SetMode(ModulationMode mode)
    {
        currentMode = mode;
        currentModifiers = modeModifiers[mode];
    }

    private void OnStageStarted()
    {
        stageStartedAt = Time.time;
        stageRunning = true;
        scriptedSequencesActive = 0;
        continuousDigSeconds = 0f;
        isDigging = false;
        SetMode(ModulationMode.Default);
    }

    private void OnScriptedSequenceStarted()
    {
        scriptedSequencesActive++;
    }

    private void OnScriptedSequenceEnded()
    {
        if (scriptedSequencesActive > 0)
        {
            scriptedSequencesActive--;
        }
    }

    private void BuildModeTables()
    {
        modeModifiers.Clear();

        modeModifiers[ModulationMode.Mercy] = new Dictionary<string, float>
        {
            { nameof(IDifficultyProfile.GuardCheckMinSeconds), MercyGuardCheckMultiplier },
            { nameof(IDifficultyProfile.GuardCheckMaxSeconds), MercyGuardCheckMultiplier },
            { nameof(IDifficultyProfile.CameraConeDegrees), MercyCameraConeMultiplier },
            { nameof(IDifficultyProfile.HopeLostWhenCaught), MercyHopeLostMultiplier }
        };

        modeModifiers[ModulationMode.Ease] = new Dictionary<string, float>
        {
            { nameof(IDifficultyProfile.GuardCheckMinSeconds), EaseGuardCheckMultiplier },
            { nameof(IDifficultyProfile.GuardCheckMaxSeconds), EaseGuardCheckMultiplier },
            { nameof(IDifficultyProfile.BatterySeconds), EaseBatteryMultiplier }
        };

        modeModifiers[ModulationMode.Default] = new Dictionary<string, float>();

        modeModifiers[ModulationMode.Tighten] = new Dictionary<string, float>
        {
            { nameof(IDifficultyProfile.GuardCheckMinSeconds), TightenGuardCheckMultiplier },
            { nameof(IDifficultyProfile.GuardCheckMaxSeconds), TightenGuardCheckMultiplier },
            { nameof(IDifficultyProfile.GuardPatrolCycleSeconds), TightenPatrolCycleMultiplier }
        };

        modeModifiers[ModulationMode.Challenge] = new Dictionary<string, float>
        {
            { nameof(IDifficultyProfile.GuardCheckMinSeconds), ChallengeGuardCheckMultiplier },
            { nameof(IDifficultyProfile.GuardCheckMaxSeconds), ChallengeGuardCheckMultiplier },
            { nameof(IDifficultyProfile.CameraCount), ChallengeCameraCountMultiplier }
        };
    }
}
