using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The scene singleton that holds the six OptionalMission assets and the current RunState,
/// and listens to the optional-recorded event so the state updates as the player plays.
/// The EndingDirector asks it which missions are completed; nothing else reads from here.
/// From design 3.3: the missions are quiet - there is no HUD, no check list, no "6/14" count.
/// </summary>
[DefaultExecutionOrder(OptionalContent.ExecutionOrder + 1)]
public class OptionalMissions : MonoBehaviour
{
    /// <summary>The registry ids of the optional moments that complete a mission by themselves.</summary>
    public const string CountWallMarkEntryId = "count-wall-mark";
    public const string SandbagWriteEntryId = "sandbag-hostage-write";
    public const string BraceletEntryId = "bracelet-festival";

    /// <summary>The OptionalMissions in the active scene. Null before Awake and after the scene unloads.</summary>
    public static OptionalMissions Instance { get; private set; }

    [Tooltip("The six missions this scene tracks. Usually the OptionalMissionFactory default set")]
    [SerializeField] private OptionalMission[] missions;

    [Tooltip("Raised whenever an optional moment is recorded. The OptionalContent raises this in its event; " +
             "an asset is used so the missions do not depend on a scene singleton for testing")]
    [SerializeField] private GameEvent optionalRecorded;

    [Tooltip("The recorder to read the ids from. If left empty, OptionalContent.Instance is used at Awake")]
    private IOptionalRecorder recorder;

    private readonly RunState state = new RunState();
    private Action<string> onRecorded;

    /// <summary>The live run state. Mission zones write to the counters; the ending reads them.</summary>
    public RunState State => state;

    /// <summary>The missions assigned in the inspector, exposed for the ending director.</summary>
    public IReadOnlyList<OptionalMission> Missions => missions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        Instance = this;
        if (missions == null || missions.Length == 0)
        {
            missions = OptionalMissionFactory.DefaultSet(); // No assets wired: the six from the design.
        }
        if (recorder == null && OptionalContent.Instance != null)
        {
            recorder = OptionalContent.Instance;
        }
    }

    private void OnEnable()
    {
        if (optionalRecorded != null)
        {
            optionalRecorded.Subscribe(OnOptionalRecordedEvent);
        }
        if (recorder != null)
        {
            onRecorded = id => Record(id);
            recorder.Recorded += onRecorded;
        }
    }

    private void OnDisable()
    {
        if (optionalRecorded != null)
        {
            optionalRecorded.Unsubscribe(OnOptionalRecordedEvent);
        }
        if (recorder != null && onRecorded != null)
        {
            recorder.Recorded -= onRecorded;
            onRecorded = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Lets tests and other agents inject a recorder before Awake binds to OptionalContent.Instance.
    /// Call this before OnEnable to swap the real singleton out.
    /// </summary>
    public void UseRecorder(IOptionalRecorder newRecorder)
    {
        if (recorder != null && onRecorded != null)
        {
            recorder.Recorded -= onRecorded;
        }
        recorder = newRecorder;
        if (recorder != null)
        {
            onRecorded = id => Record(id);
            recorder.Recorded += onRecorded;
        }
    }

    /// <summary>
    /// Replaces the mission list. Used by scenes that assemble the default set at runtime through
    /// the factory, and by tests that want a smaller list than the full six.
    /// </summary>
    public void UseMissions(params OptionalMission[] newMissions)
    {
        missions = newMissions ?? Array.Empty<OptionalMission>();
    }

    /// <summary>
    /// Marks an optional id as recorded for the current run. Idempotent: the same id a second
    /// time is a no-op, matching the recorder's own contract. Zones call this through the event;
    /// tests call it directly to set up the state.
    /// </summary>
    public void Record(string entryId)
    {
        if (string.IsNullOrEmpty(entryId))
        {
            return;
        }
        state.optionalEntriesRecorded.Add(entryId);

        // Three of the missions are a single optional moment each, so recording it completes them.
        if (entryId == CountWallMarkEntryId)
        {
            state.countWallMarked = true;
        }
        else if (entryId == SandbagWriteEntryId)
        {
            state.sandbagWritten = true;
        }
        else if (entryId == BraceletEntryId)
        {
            state.braceletFound = true;
        }
    }

    /// <summary>True if any mission of this kind is in the list and its condition is met.</summary>
    public bool IsCompleted(MissionKind kind)
    {
        if (missions == null)
        {
            return false;
        }
        foreach (OptionalMission mission in missions)
        {
            if (mission != null && mission.Kind == kind && mission.CheckCompleted(state))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Every mission from this scene's list whose condition is met. The EndingDirector loops over
    /// this list to add its small details. Order follows the inspector; duplicates are kept.
    /// </summary>
    public IReadOnlyList<OptionalMission> CompletedMissions()
    {
        var completed = new List<OptionalMission>();
        if (missions == null)
        {
            return completed;
        }
        foreach (OptionalMission mission in missions)
        {
            if (mission != null && mission.CheckCompleted(state))
            {
                completed.Add(mission);
            }
        }
        return completed;
    }

    // Kept private so only the subscribed GameEvent reaches it. It fires without an id payload,
    // so the handler only pulls the latest id from the recorder when one is available.
    private void OnOptionalRecordedEvent()
    {
        // The GameEvent asset is payload-less, so the recorder is still consulted for ids;
        // the common case uses the C# event on IOptionalRecorder, which passes the id directly.
    }
}
