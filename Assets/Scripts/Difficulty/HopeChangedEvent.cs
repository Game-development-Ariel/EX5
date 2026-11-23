using System;
using UnityEngine;

/// <summary>
/// Raised when the player's hope changes. Carries the old and the new value so the SkillTracker can tell
/// whether hope crossed the "kept" or "low" thresholds in section 1.3.
/// </summary>
[CreateAssetMenu(menuName = "Events/Hope Changed Event")]
public class HopeChangedEvent : ScriptableObject
{
    private event Action<int, int> Raised;

    /// <summary>oldValue, newValue.</summary>
    public void Raise(int oldValue, int newValue)
    {
        Raised?.Invoke(oldValue, newValue);
    }

    public void Subscribe(Action<int, int> listener)
    {
        Raised += listener;
    }

    public void Unsubscribe(Action<int, int> listener)
    {
        Raised -= listener;
    }
}
