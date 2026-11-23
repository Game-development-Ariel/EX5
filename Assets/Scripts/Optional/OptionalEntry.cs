using System;
using UnityEngine;

/// <summary>
/// One optional moment the player can find, make or sit with. A plain serializable value,
/// listed on the OptionalRegistry asset. The id is the stable key the rest of the game
/// uses; the display name is for the inspector and for the end-of-run journal.
/// From design 3.1: an optional moment is not a mission with a reward, it is a small
/// anchor the player chose to notice.
/// </summary>
[Serializable]
public class OptionalEntry
{
    [Tooltip("Stable key used by zones, actions and the recorder. For example 'prev-sacks-soft-corner'")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name, shown in the inspector and in the end journal")]
    [SerializeField] private string displayName;

    [Tooltip("Which of the three layers this moment belongs to: discovery, expression, observation")]
    [SerializeField] private OptionalKind kind;

    [Tooltip("The stage this moment lives in. '1'..'5' by convention; empty if it fits any stage")]
    [SerializeField] private string stageId;

    [Tooltip("Hope granted when the moment is recorded. Usually 0: optional moments are not a hope faucet")]
    [SerializeField] private int hopeReward;

    /// <summary>The stable key used by zones, actions and the recorder.</summary>
    public string Id => id;

    /// <summary>The human-readable name, shown in the inspector and in the end journal.</summary>
    public string DisplayName => displayName;

    /// <summary>Which of the three layers this moment belongs to.</summary>
    public OptionalKind Kind => kind;

    /// <summary>The stage the moment lives in.</summary>
    public string StageId => stageId;

    /// <summary>How much hope is granted when the moment is recorded. Usually 0.</summary>
    public int HopeReward => hopeReward;

    public OptionalEntry()
    {
    }

    /// <summary>Lets code and tests build an entry without the Inspector.</summary>
    public OptionalEntry(string id, string displayName, OptionalKind kind, string stageId, int hopeReward = 0)
    {
        this.id = id;
        this.displayName = displayName;
        this.kind = kind;
        this.stageId = stageId;
        this.hopeReward = hopeReward;
    }
}
