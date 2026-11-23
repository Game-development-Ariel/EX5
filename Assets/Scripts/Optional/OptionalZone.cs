using UnityEngine;

/// <summary>
/// A trigger that records an optional moment the first time the player walks into it.
/// Used for the discovery and observation layers: a page on the floor, a detail in the
/// guard room, the door crack, the prayer spot. The zone stays in the scene; a repeat
/// visit is silent because OptionalContent only raises its event the first time.
/// From design 3.1: an optional item is seen once as a moment, and not again as a reward.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class OptionalZone : MonoBehaviour
{
    [Tooltip("The optional moment to record. Must match an id in the OptionalRegistry")]
    [SerializeField] private string entryId;

    [Tooltip("Raised when the id is recorded for the first time. A caption listens to it. Optional")]
    [SerializeField] private GameEvent whenRecorded;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(entryId))
        {
            return;
        }
        IOptionalRecorder recorder = OptionalContent.Instance;
        if (recorder == null || recorder.WasRecorded(entryId))
        {
            return; // Already recorded this run, or no recorder in the scene yet.
        }
        recorder.Record(entryId);
        if (whenRecorded != null)
        {
            whenRecorded.Raise();
        }
    }
}
