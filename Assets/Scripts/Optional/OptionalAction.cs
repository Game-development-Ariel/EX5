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

    [Tooltip("Raised with the texts below: the hint when the player first comes close, the thought when the action is done. Optional")]
    [SerializeField] private TextEvent thoughtRequested;

    [TextArea]
    [SerializeField] private string hint;

    [TextArea]
    [SerializeField] private string completedThought;

    private float heldSeconds;
    private bool completed;
    private bool hintShown;

    // The key of this action's taps in InputMode, for players who tap instead of holding.
    private string TapId => "optional-action:" + entryId;

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
        if (player == null || !IsPlayerClose())
        {
            heldSeconds = 0f; // Walking out of the radius cancels the hold.
            InputMode.Reset(TapId);
            return;
        }

        if (!hintShown && thoughtRequested != null && !string.IsNullOrEmpty(hint))
        {
            hintShown = true;
            thoughtRequested.Raise(hint);
        }

        // With "tap instead of hold" every tap adds a little time. InputMode ignores taps when the setting is off.
        if (interactAction.WasPressedThisFrame())
        {
            InputMode.RegisterTap(TapId);
        }

        // Letting go of the key cancels the hold.
        heldSeconds = interactAction.IsPressed() ? heldSeconds + Time.deltaTime : 0f;
        if (InputMode.IsHoldFulfilled(TapId, holdSeconds, heldSeconds))
        {
            Complete();
        }
    }

    private bool IsPlayerClose()
    {
        Vector2 offset = player.position - transform.position; // On the flat map only x and y matter.
        return offset.sqrMagnitude <= cancelRadius * cancelRadius;
    }

    private void Complete()
    {
        completed = true;
        if (!string.IsNullOrEmpty(entryId) && OptionalContent.Instance != null)
        {
            OptionalContent.Instance.Record(entryId);
        }
        if (thoughtRequested != null && !string.IsNullOrEmpty(completedThought))
        {
            thoughtRequested.Raise(completedThought);
        }
        onCompleted.Invoke();
    }
}
