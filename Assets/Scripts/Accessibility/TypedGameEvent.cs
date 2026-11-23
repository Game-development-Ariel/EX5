using System;
using UnityEngine;

/// <summary>
/// A GameEvent that carries a payload of type T. Mirrors the pattern of GameEvent and TextEvent,
/// so any script that can raise or subscribe to those can do the same with any data.
/// The accessibility modes use it for sound cues (direction + loudness) and subtitles.
/// </summary>
public class TypedGameEvent<T> : ScriptableObject
{
    private event Action<T> Raised;

    public void Raise(T payload)
    {
        Raised?.Invoke(payload);
    }

    public void Subscribe(Action<T> listener)
    {
        Raised += listener;
    }

    public void Unsubscribe(Action<T> listener)
    {
        Raised -= listener;
    }
}
