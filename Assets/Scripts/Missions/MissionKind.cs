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
