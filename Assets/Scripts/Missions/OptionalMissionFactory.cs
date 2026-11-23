using UnityEngine;

/// <summary>
/// Builds the six OptionalMission assets at runtime, so a scene that wants the default set can
/// get it without a designer wiring six .asset files. Each factory method names the mission with
/// a stable id matching design 3.3. Tests use these to assemble an OptionalMissions component
/// without importing anything from disk.
/// </summary>
public static class OptionalMissionFactory
{
    /// <summary>"All memory moments found this run". Lets the ending draw memory-shaped stars.</summary>
    public static OptionalMission AllMemoriesMission()
    {
        return Build("all-memories", "All Memories", MissionKind.AllMemories);
    }

    /// <summary>"Every page from those before found". Thickens the end-sequence journal prop.</summary>
    public static OptionalMission AllPreviousNotesMission()
    {
        return Build("all-previous-notes", "All Previous Notes", MissionKind.AllPreviousNotes);
    }

    /// <summary>"Finished a stage without being caught". The narrator adds a quiet half-line.</summary>
    public static OptionalMission NoCapturesDayMission()
    {
        return Build("no-captures-day", "A Day Without Being Seen", MissionKind.NoCapturesDay);
    }

    /// <summary>"Wrote on the white sandbag". The news film mentions the sandbag.</summary>
    public static OptionalMission SandbagWriteMission()
    {
        return Build("sandbag-write", "Wrote On The Sandbag", MissionKind.SandbagWrite);
    }

    /// <summary>"The festival bracelet appeared". It stays on the wrist in the ending.</summary>
    public static OptionalMission BraceletFoundMission()
    {
        return Build("bracelet-found", "Bracelet Seen", MissionKind.BraceletFound);
    }

    /// <summary>"Added a tally mark on the counting wall". A small nod in the ending.</summary>
    public static OptionalMission CountWallMarkMission()
    {
        return Build("count-wall-mark", "A Mark On The Wall", MissionKind.CountWallMark);
    }

    /// <summary>The six missions in order. Used by scenes that want the full default set.</summary>
    public static OptionalMission[] DefaultSet()
    {
        return new[]
        {
            AllMemoriesMission(),
            AllPreviousNotesMission(),
            NoCapturesDayMission(),
            SandbagWriteMission(),
            BraceletFoundMission(),
            CountWallMarkMission(),
        };
    }

    private static OptionalMission Build(string id, string displayName, MissionKind kind)
    {
        OptionalMission mission = ScriptableObject.CreateInstance<OptionalMission>();
        mission.name = id;
        mission.Configure(id, displayName, kind);
        return mission;
    }
}
