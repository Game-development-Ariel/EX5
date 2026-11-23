using System;
using UnityEngine;

/// <summary>
/// The hero's hope, which is the "health" of this game. Memories add to it and being caught takes from it.
/// </summary>
public class Hope : MonoBehaviour
{
    [SerializeField] private TunnelLevelSettings settings;

    public int Current { get; private set; }
    public int Max => settings.maxHope;
    public bool IsEmpty => Current <= 0;

    /// <summary>Happens every time the hope changes. Gives the new value.</summary>
    public event Action<int> Changed;

    private void Awake()
    {
        Current = Mathf.Min(settings.startHope, settings.maxHope);
    }

    /// <summary>True only while StartAt runs, so listeners can tell a new start from a real gain or loss.</summary>
    public bool IsStarting { get; private set; }

    /// <summary>Sets the hope the level starts with. The difficulty calls it before the player can move.</summary>
    public void StartAt(int value)
    {
        IsStarting = true;
        Set(value);
        IsStarting = false;
    }

    public void Add(int amount)
    {
        Set(Current + amount);
    }

    public void Lose(int amount)
    {
        Set(Current - amount);
    }

    private void Set(int value)
    {
        Current = Mathf.Clamp(value, 0, settings.maxHope);
        Changed?.Invoke(Current);
    }
}
