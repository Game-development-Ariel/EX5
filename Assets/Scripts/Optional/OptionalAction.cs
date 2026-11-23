using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// A hold-to-interact that records an optional moment after a few seconds of holding still.
/// Used for the expression layer: writing "hostage" on the white sandbag, adding a line on
/// the counting wall, lighting the flashlight in the empty room. If the player moves out of
/// a small radius around the spot, the hold cancels and must be started again, so the moment
/// takes a real choice to make.
/// From design 3.1: the sandbag is the game's big choice; the player holds for five seconds
/// to write, can skip at no cost, and if written, the news ticker at the end mentions the sack.
/// </summary>
public class OptionalAction : MonoBehaviour
{
    [Tooltip("The optional moment to record. Must match an id in the OptionalRegistry")]
    [SerializeField] private string entryId;

    [Tooltip("Seconds the player must hold still at the spot for the action to complete. Design 3.1 asks for five")]
    [Min(0f)]
    [SerializeField] private float holdSeconds = 5f;

    [Tooltip("The player must stay within this many world units of the spot, or the hold cancels")]
    [Min(0f)]
    [SerializeField] private float cancelRadius = 0.6f;

    [Tooltip("The player object. The hold starts when he is inside the radius and cancels when he leaves")]
    [SerializeField] private Transform player;

    [Tooltip("Hold this key or button to act. Default: the keyboard E key")]
    [SerializeField]
    private InputAction interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");

    [Tooltip("Called once, when the hold completes. Used to play the writing sound, swap the sprite and so on")]
    [SerializeField] private UnityEvent onCompleted = new UnityEvent();

    private float heldSeconds;
    private bool completed;

    /// <summary>How far along the current hold is, from 0 (not holding) to 1 (done). The UI meter reads this.</summary>
    public float Progress => holdSeconds <= 0f ? 1f : Mathf.Clamp01(heldSeconds / holdSeconds);

    /// <summary>True from the frame the action completes until the component is destroyed. A completed action does not re-fire.</summary>
    public bool Completed => completed;

    private void OnEnable()
    {
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
        heldSeconds = 0f;
    }

    private void Update()
    {
        if (completed)
        {
            return;
        }
        if (player == null || !IsPlayerClose() || !interactAction.IsPressed())
        {
            heldSeconds = 0f; // Walking out of the radius or letting go of the key cancels the hold.
            return;
        }
        heldSeconds += Time.deltaTime;
        if (heldSeconds >= holdSeconds)
        {
            Complete();
        }
    }

    private bool IsPlayerClose()
    {
        Vector3 offset = player.position - transform.position;
        return offset.sqrMagnitude <= cancelRadius * cancelRadius;
    }

    private void Complete()
    {
        completed = true;
        if (!string.IsNullOrEmpty(entryId) && OptionalContent.Instance != null)
        {
            OptionalContent.Instance.Record(entryId);
        }
        onCompleted.Invoke();
    }
}
