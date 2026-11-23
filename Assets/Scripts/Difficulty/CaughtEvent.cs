using System;
using UnityEngine;

/// <summary>
/// Raised when the player is seen by a guard or a camera. The design calls this signal "CaughtEvent"
/// in section 1.3. The SkillTracker subtracts two points from the score on every raise.
/// </summary>
[CreateAssetMenu(menuName = "Events/Caught Event")]
public class CaughtEvent : ScriptableObject
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
