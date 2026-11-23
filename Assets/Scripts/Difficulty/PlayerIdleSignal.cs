using System;
using UnityEngine;

/// <summary>
/// Raised repeatedly by the PlayerIdleTimer whenever the player has been idle for at least one second,
/// carrying the running idle seconds. The SkillTracker penalises the player when the value reaches 30.
/// </summary>
[CreateAssetMenu(menuName = "Events/Player Idle Signal")]
public class PlayerIdleSignal : ScriptableObject
{
    private event Action<float> Raised;

    /// <summary>Seconds the player has been continuously idle so far.</summary>
    public void Raise(float idleSeconds)
    {
        Raised?.Invoke(idleSeconds);
    }

    public void Subscribe(Action<float> listener)
    {
        Raised += listener;
    }

    public void Unsubscribe(Action<float> listener)
    {
        Raised -= listener;
    }
}
