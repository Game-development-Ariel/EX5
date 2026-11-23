using UnityEngine;

/// <summary>
/// Applies the slow natural decay of hope over time, as described in section 1.2 of the
/// dramatic-elements doc. Operates on <see cref="ContinuousHope"/> so the decay lands in
/// fractions and does not snap the integer <see cref="Hope"/> every frame.
///
/// The rate per minute is read from the current difficulty profile when
/// <see cref="DifficultyManager"/> is in the scene, and falls back to the serialized
/// <see cref="defaultRatePerMinute"/> otherwise so the component is useful in tests.
/// </summary>
[RequireComponent(typeof(ContinuousHope))]
public class HopeNaturalDecay : MonoBehaviour
{
    private const float SecondsPerMinute = 60f;

    [Tooltip("Rate in hope points per minute when no difficulty profile is active")]
    [Min(0f)]
    [SerializeField] private float defaultRatePerMinute = 2f;

    [Tooltip("Parameter key on the difficulty profile that holds the rate. Must match a profile property name")]
    [SerializeField] private string difficultyParameterName = "HopeNaturalDecayPerMinute";

    private ContinuousHope hope;
    private PlayerMovementLock movementLock;

    private void Awake()
    {
        hope = GetComponent<ContinuousHope>();
        movementLock = GetComponent<PlayerMovementLock>(); // Optional.
    }

    private void Update()
    {
        if (!enabled)
        {
            return;
        }

        // Time the player can't act in (the opening, the dark after a capture, a menu) doesn't cost hope.
        if (movementLock != null && !movementLock.IsFree)
        {
            return;
        }

        float perMinute = ReadRatePerMinute();
        if (perMinute <= 0f)
        {
            return;
        }

        float perSecond = perMinute / SecondsPerMinute;
        hope.Lose(perSecond * Time.deltaTime);
    }

    private float ReadRatePerMinute()
    {
        IDifficultyProfile profile = DifficultyManager.Current;
        if (profile == null)
        {
            return defaultRatePerMinute;
        }

        float fromProfile = profile.CurrentValue(difficultyParameterName);
        return fromProfile > 0f ? fromProfile : defaultRatePerMinute;
    }
}
