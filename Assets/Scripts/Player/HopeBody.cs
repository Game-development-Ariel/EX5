using System;
using UnityEngine;

/// <summary>
/// Interprets the hope value as four tiers (Faith / Calm / Worry / Despair) and drives the
/// diegetic body signals that the player hears and sees: breathing loudness, pulse that masks
/// footsteps in Despair, and the broadcast of a tier change so <see cref="HopeSenses"/> and
/// other listeners stay in sync without each of them owning its own thresholds.
///
/// Thresholds come from section 2.2 of the dramatic-elements doc:
/// Faith &gt;= 75, Calm 40..75, Worry 25..40, Despair &lt; 25.
///
/// This script reads the decimal <see cref="ContinuousHope"/> when present (so slow decay is
/// felt) and falls back to the integer <see cref="Hope"/> otherwise, so it works whether or not
/// the continuous layer is wired on this prefab.
/// </summary>
[RequireComponent(typeof(Hope))]
public class HopeBody : MonoBehaviour
{
    public const float FaithThreshold = 75f;
    public const float CalmThreshold = 40f;
    public const float WorryThreshold = 25f;

    [Tooltip("Breath audio source placed on the hero. Loops while enabled")]
    [SerializeField] private AudioSource breathSource;

    [Tooltip("Breath clip for Faith and Calm — soft and barely there")]
    [SerializeField] private AudioClip quietBreathClip;

    [Tooltip("Breath clip for Worry — audible, slower cadence than the fast one")]
    [SerializeField] private AudioClip audibleBreathClip;

    [Tooltip("Breath clip for Despair — fast and shallow")]
    [SerializeField] private AudioClip fastBreathClip;

    [Tooltip("Pulse audio source. Loud in Despair so it masks footsteps the hero would hear outside")]
    [SerializeField] private AudioSource pulseSource;

    [Tooltip("Pulse gain multiplier added to Despair state. 1 means no change, > 1 louder")]
    [Range(1f, 4f)]
    [SerializeField] private float despairPulseGain = 2.5f;

    [Tooltip("Normal pulse gain outside Despair")]
    [Range(0f, 2f)]
    [SerializeField] private float normalPulseGain = 1f;

    [Tooltip("The pulse volume at gain 1, from 0 to 1. The gains above multiply it")]
    [Range(0f, 1f)]
    [SerializeField] private float pulseBaseVolume = 0.35f;

    [Tooltip("Breath volume in Faith and Calm (x), Worry (y) and Despair (z), from 0 to 1")]
    [SerializeField] private Vector3 breathVolumes = new Vector3(0f, 0.25f, 0.45f);

    [Tooltip("Breath pitch in Faith and Calm (x), Worry (y) and Despair (z). 1 = the clip's own speed")]
    [SerializeField] private Vector3 breathPitches = new Vector3(1f, 1.05f, 1.35f);

    [Tooltip("Seconds of hysteresis around every threshold. Prevents flicker between tiers")]
    [Min(0f)]
    [SerializeField] private float hysteresis = 1.5f;

    private Hope hope;
    private ContinuousHope continuous;

    /// <summary>The current tier, cached so listeners can poll without recomputing.</summary>
    public HopeTier Current { get; private set; }

    /// <summary>Fires when the tier changes, with the new tier.</summary>
    public event Action<HopeTier> TierChanged;

    private void Awake()
    {
        hope = GetComponent<Hope>();
        continuous = GetComponent<ContinuousHope>();
    }

    private void OnEnable()
    {
        hope.Changed += OnHopeChangedInt;
        if (continuous != null)
        {
            continuous.ValueChanged += OnHopeChangedFloat;
        }
    }

    private void OnDisable()
    {
        hope.Changed -= OnHopeChangedInt;
        if (continuous != null)
        {
            continuous.ValueChanged -= OnHopeChangedFloat;
        }
    }

    private void Start()
    {
        Current = TierOf(ReadHope(), Current, hysteresis);
        ApplyBreath(Current);
        ApplyPulse(Current);
    }

    private void OnHopeChangedInt(int value)
    {
        Recompute();
    }

    private void OnHopeChangedFloat(float oldValue, float newValue)
    {
        Recompute();
    }

    private void Recompute()
    {
        HopeTier next = TierOf(ReadHope(), Current, hysteresis);
        if (next == Current)
        {
            return;
        }

        Current = next;
        ApplyBreath(next);
        ApplyPulse(next);
        TierChanged?.Invoke(next);
    }

    private float ReadHope()
    {
        return continuous != null ? continuous.Value : hope.Current;
    }

    /// <summary>
    /// Pure function: translates a decimal hope value to a tier, with hysteresis around the
    /// threshold so a hover near a border doesn't flicker. Testable without a scene.
    /// </summary>
    public static HopeTier TierOf(float value, HopeTier current, float hysteresis)
    {
        float faithLine = current == HopeTier.Faith ? FaithThreshold - hysteresis : FaithThreshold;
        float calmLine = current == HopeTier.Calm ? CalmThreshold - hysteresis : CalmThreshold;
        float worryLine = current == HopeTier.Worry ? WorryThreshold - hysteresis : WorryThreshold;

        if (value >= faithLine)
        {
            return HopeTier.Faith;
        }

        if (value >= calmLine)
        {
            return HopeTier.Calm;
        }

        if (value >= worryLine)
        {
            return HopeTier.Worry;
        }

        return HopeTier.Despair;
    }

    private void ApplyBreath(HopeTier tier)
    {
        if (breathSource == null)
        {
            return;
        }

        AudioClip next = tier switch
        {
            HopeTier.Faith => quietBreathClip,
            HopeTier.Calm => quietBreathClip,
            HopeTier.Worry => audibleBreathClip,
            HopeTier.Despair => fastBreathClip,
            _ => quietBreathClip,
        };

        // The same clip can serve more than one tier, so the volume and the speed carry the difference too.
        bool worried = tier == HopeTier.Worry;
        bool despairing = tier == HopeTier.Despair;
        breathSource.volume = despairing ? breathVolumes.z : worried ? breathVolumes.y : breathVolumes.x;
        breathSource.pitch = despairing ? breathPitches.z : worried ? breathPitches.y : breathPitches.x;

        if (next == null || (breathSource.clip == next && breathSource.isPlaying))
        {
            return;
        }

        breathSource.clip = next;
        breathSource.loop = true;
        breathSource.Play();
    }

    private void ApplyPulse(HopeTier tier)
    {
        if (pulseSource == null)
        {
            return;
        }

        float gain = tier == HopeTier.Despair ? despairPulseGain : normalPulseGain;
        pulseSource.volume = Mathf.Clamp01(pulseBaseVolume * gain); // An AudioSource's volume stops at 1.
    }
}
