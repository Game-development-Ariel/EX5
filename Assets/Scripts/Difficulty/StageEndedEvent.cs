using System;
using UnityEngine;

/// <summary>
/// Raised when a stage finishes. The SkillTracker applies end-of-stage rules from section 1.3 on this:
/// the no-listen penalty, the hope-kept bonus, and the low-hope penalty.
/// </summary>
[CreateAssetMenu(menuName = "Events/Stage Ended Event")]
public class StageEndedEvent : ScriptableObject
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
