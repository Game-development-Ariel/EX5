using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The director of the player's eye, from design 2.1. At every moment there is one bright object
/// on screen that is the player's goal; everything else is dark. When a goal is reached, the next
/// object becomes bright. The screen never shows more than the AttentionPolicy budget (four lights).
///
/// The director holds the sequence of beats. It starts at beat 0 and advances each time the
/// advanceBeat event is raised; a level can also jump to a chosen beat with SetBeat.
/// </summary>
public class AttentionDirector : MonoBehaviour
{
    // The time a highlight takes to come up or fade. Small so the switch feels like an eye adjusting.
    private const float HighlightLerpSeconds = 0.5f;

    [Tooltip("The sequence of beats of the scene. The scene starts at beat 0")]
    [SerializeField] private List<AttentionBeat> beats = new List<AttentionBeat>();

    [Tooltip("Raised by the story to move to the next beat (bottle picked up, bottle delivered)")]
    [SerializeField] private GameEvent advanceBeat;

    [Tooltip("The scene state machine that maps a story state to a beat. Optional")]
    [SerializeField] private SceneStateMachine sceneStateMachine;

    /// <summary>The index of the beat the scene is in now. Starts at 0.</summary>
    public int CurrentBeatIndex { get; private set; }

    /// <summary>The beat the scene is in now, or null if there are no beats.</summary>
    public AttentionBeat CurrentBeat => (CurrentBeatIndex >= 0 && CurrentBeatIndex < beats.Count) ? beats[CurrentBeatIndex] : null;

    /// <summary>The list of beats the director walks through.</summary>
    public IReadOnlyList<AttentionBeat> Beats => beats;

    /// <summary>Raised after the beat changed to the given index.</summary>
    public event Action<int> BeatChanged;

    private void OnEnable()
    {
        if (advanceBeat != null)
        {
            advanceBeat.Subscribe(Advance);
        }
        if (sceneStateMachine != null)
        {
            sceneStateMachine.StateChanged += OnSceneStateChanged;
        }
    }

    private void OnDisable()
    {
        if (advanceBeat != null)
        {
            advanceBeat.Unsubscribe(Advance);
        }
        if (sceneStateMachine != null)
        {
            sceneStateMachine.StateChanged -= OnSceneStateChanged;
        }
    }

    private void Start()
    {
        if (beats.Count > 0)
        {
            CurrentBeatIndex = 0;
            ApplyBeat(beats[0]);
            BeatChanged?.Invoke(0);
        }
    }

    /// <summary>Moves to the next beat. Called by the advanceBeat event. Stops at the last beat.</summary>
    public void Advance()
    {
        int next = CurrentBeatIndex + 1;
        if (next >= beats.Count)
        {
            return;
        }
        SetBeat(next);
    }

    /// <summary>Jumps to the beat at the given index. Lights its toLight targets, dims its toDim targets.</summary>
    public void SetBeat(int index)
    {
        if (index < 0 || index >= beats.Count)
        {
            return;
        }
        CurrentBeatIndex = index;
        ApplyBeat(beats[index]);
        BeatChanged?.Invoke(index);
    }

    private void OnSceneStateChanged(SceneState state)
    {
        // The scene state machine advances in step with the director's beats; a level that wants
        // a different mapping can call SetBeat from its own listener instead of using this.
        if (sceneStateMachine != null)
        {
            SetBeat(sceneStateMachine.CurrentIndex);
        }
    }

    private void ApplyBeat(AttentionBeat beat)
    {
        if (beat == null)
        {
            return;
        }

        foreach (HighlightTarget target in beat.ToDim)
        {
            if (target != null)
            {
                target.SetHighlighted(false, HighlightLerpSeconds);
            }
        }

        List<HighlightTarget> toLight = AttentionPolicy.ThrottleHighlights(AttentionPolicy.MaxBrightTargets, beat.ToLight);
        foreach (HighlightTarget target in toLight)
        {
            target.SetHighlighted(true, HighlightLerpSeconds);
        }
    }
}
