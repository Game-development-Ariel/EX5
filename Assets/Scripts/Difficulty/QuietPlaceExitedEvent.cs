using System;
using UnityEngine;

/// <summary>
/// Raised when the player leaves a quiet place. The SkillTracker uses it to stop watching for the
/// "did not pray" penalty, so a quick visit without a prayer is not held against the player forever.
/// </summary>
[CreateAssetMenu(menuName = "Events/Quiet Place Exited Event")]
public class QuietPlaceExitedEvent : ScriptableObject
{
    private event Action Raised;

    public void Raise()
    {
        Raised?.Invoke();
    }

    public void Subscribe(Action listener)
    {
        Raised += listener;
    }

    public void Unsubscribe(Action listener)
    {
        Raised -= listener;
    }
}
