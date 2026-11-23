using UnityEngine;

/// <summary>
/// A trigger that records an optional moment the first time the player is inside it.
/// Used for the discovery and observation layers: a page on the floor, a detail in the
/// guard room, the door crack, the prayer spot. The zone stays in the scene; a repeat
/// visit is silent because OptionalContent only raises its event the first time.
/// From design 3.1: an optional item is seen once as a moment, and not again as a reward.
/// The game moves the player with a CharacterController, so the trigger is a 3D collider.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OptionalZone : MonoBehaviour
{
    [Tooltip("The optional moment to record. Must match an id in the OptionalRegistry")]
    [SerializeField] private string entryId;

    [Tooltip("Raised when the id is recorded for the first time. A caption listens to it. Optional")]
    [SerializeField] private GameEvent whenRecorded;

    [Tooltip("If set, the moment is recorded only while the scene is calm: nothing hot, and the player standing. Optional")]
    [SerializeField] private SlowMomentsDirector calm;

    [Tooltip("Raised with the thought below when the moment is recorded. The hint UI listens to it. Optional")]
    [SerializeField] private TextEvent thoughtRequested;

    [TextArea]
    [SerializeField] private string thought;

    // Stay and not Enter: a zone that waits for calm records only after the player stood in it for a while.
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() == null || string.IsNullOrEmpty(entryId))
        {
            return;
        }

        if (calm != null && !calm.CalmEnough)
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
        if (thoughtRequested != null && !string.IsNullOrEmpty(thought))
        {
            thoughtRequested.Raise(thought);
        }
    }
}
