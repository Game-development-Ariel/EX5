using UnityEngine;

/// <summary>
/// Applies a slow natural decay of hope over time. By design hope doesn't drop with time (topic 4),
/// so the rate is 0 on every level. It stays as a number Live Ops can set for a test. Operates on <see cref="ContinuousHope"/> so the decay lands in
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

    [Tooltip("Rate in hope points per minute when no difficulty profile is active. 0: hope doesn't drop with time")]
    [Min(0f)]
    [SerializeField] private float defaultRatePerMinute;

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

        return profile.CurrentValue(difficultyParameterName);
    }
}
