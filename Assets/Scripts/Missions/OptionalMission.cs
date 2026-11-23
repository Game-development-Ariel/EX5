using UnityEngine;

/// <summary>
/// The six optional missions the ending reads. From design 3.3: the player never sees a checklist
/// of these, and there is no "100%" progress bar; they are quiet conditions that, when met, let
/// the EndingDirector add a small detail.
/// </summary>
public enum MissionKind
{
    /// <summary>All memory moments found in the run. Lets the star field show memory-shaped constellations.</summary>
    AllMemories,

    /// <summary>Every journal page from "those before" found. Makes the end-sequence journal look thicker.</summary>
    AllPreviousNotes,

    /// <summary>The player finished a stage without being caught once. The narrator gets an extra half-line.</summary>
    NoCapturesDay,

    /// <summary>The player chose to write "I was here" on the white sandbag in stage 4. The news film mentions it.</summary>
    SandbagWrite,

    /// <summary>The festival bracelet was seen during a high-hope moment. It lingers on the hero's wrist at the end.</summary>
    BraceletFound,

    /// <summary>The player added a tally mark on the counting wall in stage 2. A small nod in the ending.</summary>
    CountWallMark,
}

/// <summary>
/// One optional mission, saved as a ScriptableObject so designers could point to it from zones
/// and the ending. It holds its own id and display name, the kind the ending matches against,
/// and a condition evaluated over a RunState. The condition is the strategy: the mission does
/// not care how the counters got there, only whether they meet the bar.
/// From design 3.3: "the component reads the recorded list and composes the right ending".
/// </summary>
[CreateAssetMenu(menuName = "Tunnel/Optional Mission")]
public class OptionalMission : ScriptableObject
{
    [Tooltip("Stable key. For example 'all-memories'. Other code never relies on the display name")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name, shown in the inspector only. The player never sees this")]
    [SerializeField] private string displayName;

    [Tooltip("Which of the six conditions this asset represents. The EndingDirector switches on this")]
    [SerializeField] private MissionKind kind;

    /// <summary>Stable key other systems use to refer to this mission. Set in the inspector or by the factory.</summary>
    public string Id => id;

    /// <summary>Human-readable name, inspector-only.</summary>
    public string DisplayName => displayName;

    /// <summary>Which of the six optional conditions this asset represents.</summary>
    public MissionKind Kind => kind;

    /// <summary>
    /// True when the condition is met by the current run state. Called by OptionalMissions when
    /// the ending is being composed; it is pure, so tests can call it with a hand-built RunState.
    /// </summary>
    public bool CheckCompleted(RunState state)
    {
        if (state == null)
        {
            return false;
        }
        switch (kind)
        {
            case MissionKind.AllMemories:
                return state.totalMemories > 0 && state.memoriesFound >= state.totalMemories;
            case MissionKind.AllPreviousNotes:
                return state.totalPreviousNotes > 0 && state.previousNotesFound >= state.totalPreviousNotes;
            case MissionKind.NoCapturesDay:
                return state.capturesThisStage == 0;
            case MissionKind.SandbagWrite:
                return state.sandbagWritten;
            case MissionKind.BraceletFound:
                return state.braceletFound;
            case MissionKind.CountWallMark:
                return state.countWallMarked;
            default:
                return false;
        }
    }

    /// <summary>Lets the factory and tests build an asset without the inspector.</summary>
    internal void Configure(string id, string displayName, MissionKind kind)
    {
        this.id = id;
        this.displayName = displayName;
        this.kind = kind;
    }
}
