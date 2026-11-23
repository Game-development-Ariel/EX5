using System;
using UnityEngine;

/// <summary>
/// One phase of the scene. Each state names the GameEvent that enters it and the one that exits it,
/// so a level builder connects story events (bottle delivered, verse completed) to attention beats
/// without the states needing to know about each other.
/// </summary>
[Serializable]
public class SceneState
{
    [Tooltip("The name that reads in the Inspector and in logs")]
    [SerializeField] private string name;

    [Tooltip("The event that puts the scene into this state. Leave empty on the first state")]
    [SerializeField] private GameEvent onEnter;

    [Tooltip("The event that ends this state. Leave empty on the last state")]
    [SerializeField] private GameEvent onExit;

    /// <summary>The state's name.</summary>
    public string Name => name;

    /// <summary>The event that transitions the scene into this state.</summary>
    public GameEvent OnEnter => onEnter;

    /// <summary>The event that transitions the scene out of this state.</summary>
    public GameEvent OnExit => onExit;

    public SceneState()
    {
    }

    /// <summary>Lets code and tests build a state without the Inspector.</summary>
    public SceneState(string name, GameEvent onEnter, GameEvent onExit)
    {
        this.name = name;
        this.onEnter = onEnter;
        this.onExit = onExit;
    }
}
