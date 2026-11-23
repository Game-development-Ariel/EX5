using System;
using UnityEngine;

/// <summary>
/// Gatekeeper for the "quiet" moments of the game. Optional content that must not break the
/// player's tension (a guard's chatter, a radio fragment, the slow-breath hint at the prayer
/// spot) checks CalmEnough before it plays, and listens to CalmChanged to stop as soon as the
/// scene heats up again.
/// From design 3.1: an optional event does not appear at a moment of high tension; a guard's
/// talk will not break out during an active dig but will wait for the player to be back in
/// the cell, standing still, for a short beat.
/// </summary>
public class SlowMomentsDirector : MonoBehaviour
{
    [Header("Listens to")]
    [Tooltip("Raised when the player starts digging. Not calm while this is active")]
    [SerializeField] private GameEvent digActiveEvent;

    [Tooltip("Raised when the dig ends, or is cancelled. Calmness becomes possible again")]
    [SerializeField] private GameEvent digEndedEvent;

    [Tooltip("Raised when a guard is close enough to see the player. Not calm while this is active")]
    [SerializeField] private GameEvent guardCloseEvent;

    [Tooltip("Raised when the guard leaves. Calmness becomes possible again")]
    [SerializeField] private GameEvent guardClearEvent;

    [Tooltip("Raised in scripted breaking-point beats (the torn Shabbat page, the alarm). Not calm while this is active")]
    [SerializeField] private GameEvent scriptedBreakingPointEvent;

    [Tooltip("Raised when the scripted beat ends. Calmness becomes possible again")]
    [SerializeField] private GameEvent scriptedBreakingPointEndedEvent;

    [Header("Watches")]
    [Tooltip("The player's movement. Calm needs the player to stand still for a few seconds")]
    [SerializeField] private PlayerMovement playerMovement;

    [Tooltip("How many seconds of 'nothing hot and player standing' are needed before CalmEnough turns true")]
    [Min(0f)]
    [SerializeField] private float calmSecondsRequired = 3f;

    private bool digging;
    private bool guardClose;
    private bool scriptedBeat;
    private float stillSeconds;
    private bool calm;

    /// <summary>
    /// True when nothing hot is happening and the player has been standing for calmSecondsRequired.
    /// False as soon as any hot event fires or the player starts moving.
    /// </summary>
    public bool CalmEnough => calm;

    /// <summary>Raised when CalmEnough flips. True means we just entered calm, false means we just left it.</summary>
    public event Action<bool> CalmChanged;

    private void OnEnable()
    {
        // A level with no digging (the tutorial) leaves the two dig events empty.
        if (digActiveEvent != null)
        {
            digActiveEvent.Subscribe(OnDigActive);
        }
        if (digEndedEvent != null)
        {
            digEndedEvent.Subscribe(OnDigEnded);
        }
        guardCloseEvent.Subscribe(OnGuardClose);
        guardClearEvent.Subscribe(OnGuardClear);
        scriptedBreakingPointEvent.Subscribe(OnScriptedBeat);
        scriptedBreakingPointEndedEvent.Subscribe(OnScriptedBeatEnded);
    }

    private void OnDisable()
    {
        if (digActiveEvent != null)
        {
            digActiveEvent.Unsubscribe(OnDigActive);
        }
        if (digEndedEvent != null)
        {
            digEndedEvent.Unsubscribe(OnDigEnded);
        }
        guardCloseEvent.Unsubscribe(OnGuardClose);
        guardClearEvent.Unsubscribe(OnGuardClear);
        scriptedBreakingPointEvent.Unsubscribe(OnScriptedBeat);
        scriptedBreakingPointEndedEvent.Unsubscribe(OnScriptedBeatEnded);
    }

    private void Update()
    {
        bool hot = digging || guardClose || scriptedBeat;
        bool standing = playerMovement != null && !playerMovement.IsWalking;

        if (hot || !standing)
        {
            stillSeconds = 0f;
            SetCalm(false);
            return;
        }

        stillSeconds += Time.deltaTime;
        if (stillSeconds >= calmSecondsRequired)
        {
            SetCalm(true);
        }
    }

    private void SetCalm(bool value)
    {
        if (calm == value)
        {
            return;
        }
        calm = value;
        CalmChanged?.Invoke(value);
    }

    private void OnDigActive()
    {
        digging = true;
        stillSeconds = 0f;
        SetCalm(false);
    }

    private void OnDigEnded()
    {
        digging = false;
    }

    private void OnGuardClose()
    {
        guardClose = true;
        stillSeconds = 0f;
        SetCalm(false); // The guard's arrival drops calm immediately, before Update runs.
    }

    private void OnGuardClear()
    {
        guardClose = false;
    }

    private void OnScriptedBeat()
    {
        scriptedBeat = true;
        stillSeconds = 0f;
        SetCalm(false);
    }

    private void OnScriptedBeatEnded()
    {
        scriptedBeat = false;
    }
}
