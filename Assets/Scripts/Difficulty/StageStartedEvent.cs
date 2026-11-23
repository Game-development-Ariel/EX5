using System;
using UnityEngine;

/// <summary>
/// Raised at the start of every stage. The SkillTracker resets the skill score to zero on this, and the
/// DifficultyModulator notes the stage start time so it can quiet down for the first 30 seconds.
/// </summary>
[CreateAssetMenu(menuName = "Events/Stage Started Event")]
public class StageStartedEvent : ScriptableObject
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
