using System;
using UnityEngine;

/// <summary>
/// Raised when a dig session ends: the player released the dig key, or was caught during the dig.
/// Carries the meters dug this session and whether a capture ended it, so the tracker can award
/// or deduct the right amount from the skill score.
/// </summary>
[CreateAssetMenu(menuName = "Events/Dig Session Ended Event")]
public class DigSessionEndedEvent : ScriptableObject
{
    private event Action<float, bool> Raised;

    /// <summary>meters dug this session, and whether it ended because of a capture.</summary>
    public void Raise(float meters, bool endedByCatch)
    {
        Raised?.Invoke(meters, endedByCatch);
    }

    public void Subscribe(Action<float, bool> listener)
    {
        Raised += listener;
    }

    public void Unsubscribe(Action<float, bool> listener)
    {
        Raised -= listener;
    }
}
