using System;
using UnityEngine;

/// <summary>
/// Raised when the player steps into a quiet place where they could pray.
/// The SkillTracker begins the ten-second grace window for the "did not pray" penalty on this raise.
/// </summary>
[CreateAssetMenu(menuName = "Events/Quiet Place Entered Event")]
public class QuietPlaceEnteredEvent : ScriptableObject
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
