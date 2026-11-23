using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remembers which optional moments the player has recorded in this run.
/// Lives on a scene object so a reload clears it the way every other level state is cleared.
/// Zones, actions and the EndingDirector reach it through the static Instance.
/// From design 3.1: an optional item is recorded once; seeing it again is still a quiet moment,
/// but it does not re-raise anything or re-award hope.
/// From design 3.3: the EndingDirector reads the recorded list to compose the end film and the journal.
/// </summary>
public class OptionalContent : MonoBehaviour, IOptionalRecorder
{
    /// <summary>The single OptionalContent in the active scene. Null before Awake and after the scene unloads.</summary>
    public static OptionalContent Instance { get; private set; }

    [Tooltip("The registry of all optional moments. Used to look up the stage and the hope reward of a recorded id")]
    [SerializeField] private OptionalRegistry registry;

    private readonly HashSet<string> recorded = new HashSet<string>();

    /// <summary>Raised the first time an id is recorded. The second record of the same id is silent.</summary>
    public event Action<string> Recorded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Marks this id as recorded. The first call raises the Recorded event; later calls with the same id
    /// are silent, so a zone the player walks over twice does not fire twice.
    /// </summary>
    public void Record(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        if (!recorded.Add(id))
        {
            return; // Already recorded. The second visit is still a quiet moment, but it is not an event.
        }
        Recorded?.Invoke(id);
    }

    /// <summary>True once Record was called for this id in this run, false otherwise.</summary>
    public bool WasRecorded(string id)
    {
        return !string.IsNullOrEmpty(id) && recorded.Contains(id);
    }

    /// <summary>
    /// The ids recorded in this run that belong to the given stage. Used by the end journal and
    /// by per-stage ending beats (for example "did the player write on the sandbag in stage 4?").
    /// </summary>
    public IReadOnlyList<string> RecordedThisStage(string stageId)
    {
        var matches = new List<string>();
        if (registry == null)
        {
            return matches;
        }
        foreach (OptionalEntry entry in registry.Entries)
        {
            if (entry.StageId == stageId && recorded.Contains(entry.Id))
            {
                matches.Add(entry.Id);
            }
        }
        return matches;
    }

    /// <summary>Every id recorded in this run, in no particular order. Used by the end screen's full journal view.</summary>
    public IReadOnlyCollection<string> AllRecorded()
    {
        return recorded;
    }
}
