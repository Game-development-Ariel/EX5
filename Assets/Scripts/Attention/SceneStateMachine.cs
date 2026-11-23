using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Walks the scene through its named phases (opening, bottle picked up, bottle delivered, verse begun...),
/// one at a time, as GameEvents are raised. Other components, like the AttentionDirector, listen to
/// StateChanged and translate a state into the beat it belongs to. This separation keeps the story
/// events on one list and the lights on another, so a designer can change either without touching the other.
/// </summary>
public class SceneStateMachine : MonoBehaviour
{
    [Tooltip("The phases of this scene in order. The first state is the one at the start")]
    [SerializeField] private List<SceneState> states = new List<SceneState>();

    /// <summary>The state the scene is in now.</summary>
    public SceneState Current { get; private set; }

    /// <summary>The index of the current state in the list.</summary>
    public int CurrentIndex { get; private set; }

    /// <summary>The list of states the machine walks through.</summary>
    public IReadOnlyList<SceneState> States => states;

    /// <summary>Raised after the state changed to the given one.</summary>
    public event Action<SceneState> StateChanged;

    // Pairs of (state, handler) that OnEnable subscribed, so OnDisable can undo exactly those.
    private readonly List<(SceneState, Action)> subscriptions = new List<(SceneState, Action)>();

    private void OnEnable()
    {
        subscriptions.Clear();
        for (int i = 0; i < states.Count; i++)
        {
            int index = i;
            SceneState state = states[i];
            if (state.OnEnter != null)
            {
                Action handler = () => GoTo(index);
                state.OnEnter.Subscribe(handler);
                subscriptions.Add((state, handler));
            }
        }
        if (states.Count > 0)
        {
            CurrentIndex = 0;
            Current = states[0];
            StateChanged?.Invoke(Current);
        }
    }

    private void OnDisable()
    {
        foreach ((SceneState state, Action handler) in subscriptions)
        {
            state.OnEnter.Unsubscribe(handler);
        }
        subscriptions.Clear();
    }

    /// <summary>Moves the scene to the state at the given index. Raises StateChanged.</summary>
    public void GoTo(int index)
    {
        if (index < 0 || index >= states.Count)
        {
            return;
        }
        CurrentIndex = index;
        Current = states[index];
        StateChanged?.Invoke(Current);
    }
}
