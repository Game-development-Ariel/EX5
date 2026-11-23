using System;
using UnityEngine;

/// <summary>
/// Raised when the player activates the listen key (holds F) to hear footsteps through the walls.
/// One raise per activation, which the SkillTracker caps at three points per stage.
/// </summary>
[CreateAssetMenu(menuName = "Events/Listen Activated Event")]
public class ListenActivatedEvent : ScriptableObject
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
