using UnityEngine;

/// <summary>
/// Brings the difficulty profile into the level. When a profile is loaded it sets the hope the hero starts
/// with, and on Medium it puts the dynamic layer (the DifficultyModulator) on top of the profile.
/// While the level runs it ticks the modulator and keeps the cameras' view and the guards' pace
/// at the profile's current values, so a change of mode is felt without any system asking for it.
/// The profile's numbers are for the full game, so the watchers use them as a ratio to Medium:
/// the tutorial's own tuning stays the baseline.
/// </summary>
public class DifficultyApplier : MonoBehaviour
{
    // Medium's camera cone, in degrees, and patrol loop, in seconds. A profile is compared to these.
    private const float BaselineCameraConeDegrees = 60f;
    private const float BaselinePatrolCycleSeconds = 14f;

    // The modulator reads the skill score this often, in seconds (section 1.3 of the design).
    private const float ModulationTickSeconds = 30f;

    [SerializeField] private Hope hope;

    [Tooltip("The dynamic layer. Used only on Medium")]
    [SerializeField] private DifficultyModulator modulator;

    [Tooltip("The parent of the cameras and the guards")]
    [SerializeField] private Transform watchers;

    [Header("Listens to")]
    [SerializeField] private GameEvent profileLoaded;

    private VisionSensor[] cameras;
    private GuardPatrol[] guards;
    private float nextTick;

    private void Awake()
    {
        // A guard has a VisionSensor too, but only the security cameras sweep.
        CameraSweep[] sweeps = watchers.GetComponentsInChildren<CameraSweep>();
        cameras = new VisionSensor[sweeps.Length];
        for (int i = 0; i < sweeps.Length; i++)
        {
            cameras[i] = sweeps[i].GetComponent<VisionSensor>();
        }
        guards = watchers.GetComponentsInChildren<GuardPatrol>();
    }

    private void OnEnable()
    {
        profileLoaded.Subscribe(OnProfileLoaded);
    }

    private void OnDisable()
    {
        profileLoaded.Unsubscribe(OnProfileLoaded);
    }

    private void OnProfileLoaded()
    {
        DifficultyProfile profile = DifficultyManager.CurrentAsset;
        if (profile == null)
        {
            return;
        }

        if (profile.Difficulty == Difficulty.Medium && modulator != null)
        {
            DifficultyManager.Override(new ModulatedDifficultyProfile(profile, modulator));
        }

        hope.StartAt(DifficultyManager.Current.StartingHope);
        ApplyToWatchers();
    }

    private void Update()
    {
        if (DifficultyManager.Current == null)
        {
            return;
        }

        if (modulator != null && Time.time >= nextTick)
        {
            nextTick = Time.time + ModulationTickSeconds;
            modulator.Tick();
        }
        ApplyToWatchers();
    }

    private void ApplyToWatchers()
    {
        IDifficultyProfile profile = DifficultyManager.Current;
        float coneFactor = profile.CameraConeDegrees / BaselineCameraConeDegrees;
        foreach (VisionSensor sensor in cameras)
        {
            if (sensor != null)
            {
                sensor.AngleFactor = coneFactor;
            }
        }

        // A longer loop means a slower guard.
        float paceFactor = BaselinePatrolCycleSeconds / profile.GuardPatrolCycleSeconds;
        foreach (GuardPatrol guard in guards)
        {
            guard.SpeedFactor = paceFactor;
        }
    }
}
