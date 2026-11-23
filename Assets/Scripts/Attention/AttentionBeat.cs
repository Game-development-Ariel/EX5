using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One moment in a scene where the player has a single bright goal. A beat names the targets
/// that should light up when the beat is entered, and the targets from the previous beat that
/// should dim. It is a plain serializable value, not a ScriptableObject, so a level builder
/// writes the full sequence as a list on the AttentionDirector in the Inspector.
/// From design 2.1: in each beat the goal is the brightest object on screen, and the rest is dark.
/// </summary>
[Serializable]
public class AttentionBeat
{
    [Tooltip("Reads as the goal of this beat in the Inspector, like 'Bottle lit' or 'Mattress lit'")]
    [SerializeField] private string name;

    [Tooltip("The targets that become bright when the beat is entered. The goal of the beat is among them")]
    [SerializeField] private List<HighlightTarget> toLight = new List<HighlightTarget>();

    [Tooltip("The targets that go dark when the beat is entered. Usually the lights from the beat before")]
    [SerializeField] private List<HighlightTarget> toDim = new List<HighlightTarget>();

    [Tooltip("Multiplies the ambient boost of the director in this beat. 1 = no change, less than 1 = darker ambient")]
    [SerializeField] private float ambientBoostMultiplier = 1f;

    /// <summary>The name that reads in the Inspector and in logs.</summary>
    public string Name => name;

    /// <summary>The targets that light up when this beat is entered.</summary>
    public IReadOnlyList<HighlightTarget> ToLight => toLight;

    /// <summary>The targets that go dark when this beat is entered.</summary>
    public IReadOnlyList<HighlightTarget> ToDim => toDim;

    /// <summary>A multiplier that lets a beat raise or lower the ambient boost. 1 keeps it the same.</summary>
    public float AmbientBoostMultiplier => ambientBoostMultiplier;

    public AttentionBeat()
    {
    }

    /// <summary>Lets code and tests build a beat without the Inspector.</summary>
    public AttentionBeat(string name, List<HighlightTarget> toLight, List<HighlightTarget> toDim, float ambientBoostMultiplier = 1f)
    {
        this.name = name;
        this.toLight = toLight ?? new List<HighlightTarget>();
        this.toDim = toDim ?? new List<HighlightTarget>();
        this.ambientBoostMultiplier = ambientBoostMultiplier;
    }
}
