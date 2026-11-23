using System;
using System.Collections.Generic;

/// <summary>
/// A plain data snapshot of what the player did in the current stage or run, used by the mission
/// strategies to decide whether their condition is satisfied. Kept serializable so a future save
/// system can round-trip it, and kept free of Unity types so edit-mode tests can build one in a line.
/// From design 3.3: optional missions are not a checklist with rewards, they are small conditions
/// that nudge the ending; the state behind them is a handful of counters and flags.
/// </summary>
[Serializable]
public class RunState
{
    /// <summary>How many memory moments the player has recorded this run.</summary>
    public int memoriesFound;

    /// <summary>How many memory moments exist in the whole run. Zero means the mission cannot be met.</summary>
    public int totalMemories;

    /// <summary>How many "those before" journal pages the player has found this run.</summary>
    public int previousNotesFound;

    /// <summary>How many "those before" journal pages exist in the whole run.</summary>
    public int totalPreviousNotes;

    /// <summary>How many times the player was caught in the current stage. Resets at the start of each stage.</summary>
    public int capturesThisStage;

    /// <summary>True after the player chose to write "I was here" on the white sandbag in stage 4.</summary>
    public bool sandbagWritten;

    /// <summary>True after the player has ever seen the festival bracelet appear during a high-hope moment.</summary>
    public bool braceletFound;

    /// <summary>True after the player added a tally mark on the counting wall in stage 2.</summary>
    public bool countWallMarked;

    /// <summary>
    /// Ids of every optional moment the player has recorded this run. Mirrors what the scene's
    /// OptionalContent recorder holds, so the mission system can read it without a scene reference.
    /// </summary>
    public HashSet<string> optionalEntriesRecorded = new HashSet<string>();
}
