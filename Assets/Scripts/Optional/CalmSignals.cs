using UnityEngine;

/// <summary>
/// Tells the SlowMomentsDirector when the level is "hot" (design 3.1): a watcher is close, or the hero
/// was just caught. The level measures closeness as Tension.Danger, and the director listens to events,
/// so this is the one place that turns the number into the events.
/// </summary>
public class CalmSignals : MonoBehaviour
{
    [Tooltip("The danger measure of the level, from 0 to 1")]
    [SerializeField] private Tension tension;

    [Tooltip("How long the capture holds the hero. The scripted beat lasts exactly that long")]
    [SerializeField] private TunnelLevelSettings settings;

    [Tooltip("At this danger and above a watcher counts as close")]
    [Range(0f, 1f)]
    [SerializeField] private float closeAtDanger = 0.95f;

    [Tooltip("At this danger and below the watcher counts as gone. Lower than the first, so it doesn't flicker")]
    [Range(0f, 1f)]
    [SerializeField] private float clearAtDanger = 0.85f;

    [Header("Listens to")]
    [SerializeField] private GameEvent playerCaught;

    [Header("Raises")]
    [SerializeField] private GameEvent guardClose;
    [SerializeField] private GameEvent guardClear;
    [SerializeField] private GameEvent scriptedBeatStarted;
    [SerializeField] private GameEvent scriptedBeatEnded;

    private bool close;

    private void OnEnable()
    {
        playerCaught.Subscribe(OnPlayerCaught);
    }

    private void OnDisable()
    {
        playerCaught.Unsubscribe(OnPlayerCaught);
    }

    private void Update()
    {
        if (!close && tension.Danger >= closeAtDanger)
        {
            close = true;
            guardClose.Raise();
        }
        else if (close && tension.Danger <= clearAtDanger)
        {
            close = false;
            guardClear.Raise();
        }
    }

    private void OnPlayerCaught()
    {
        scriptedBeatStarted.Raise();
        CancelInvoke(nameof(EndBeat)); // A second capture during the dark starts the beat over.
        Invoke(nameof(EndBeat), settings.caughtFreezeSeconds);
    }

    private void EndBeat()
    {
        scriptedBeatEnded.Raise();
    }
}
