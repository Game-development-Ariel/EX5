using UnityEngine;

/// <summary>
/// Writes what happens in the level into the RunState the optional missions read (design 3.3):
/// how many memories there are and how many were found, and how many times the hero was caught.
/// The optional moments themselves reach the missions through the OptionalContent recorder.
/// </summary>
public class RunStateTracker : MonoBehaviour
{
    [SerializeField] private OptionalMissions missions;

    [Header("Listens to")]
    [SerializeField] private TextEvent memoryFound;
    [SerializeField] private GameEvent playerCaught;

    private void OnEnable()
    {
        memoryFound.Subscribe(OnMemoryFound);
        playerCaught.Subscribe(OnPlayerCaught);
    }

    private void OnDisable()
    {
        memoryFound.Unsubscribe(OnMemoryFound);
        playerCaught.Unsubscribe(OnPlayerCaught);
    }

    private void Start()
    {
        // A quiet place raises the same "memory found" event as a memory, so both are counted.
        int memories = FindObjectsByType<MemoryZone>(FindObjectsSortMode.None).Length;
        int quietPlaces = FindObjectsByType<QuietPlace>(FindObjectsSortMode.None).Length;
        missions.State.totalMemories = memories + quietPlaces;
    }

    private void OnMemoryFound(string memory)
    {
        missions.State.memoriesFound++;
    }

    private void OnPlayerCaught()
    {
        missions.State.capturesThisStage++;
    }
}
