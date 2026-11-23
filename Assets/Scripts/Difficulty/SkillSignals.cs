using UnityEngine;

/// <summary>
/// Translates what happens in the level into the signals of the SkillTracker (section 1.3 of the design).
/// The level speaks in its own events (player caught, opening finished, level won) and the tracker listens
/// to typed signals, so this is the one place that knows both. The tutorial has no digging and no listening,
/// so those two signals are never raised here.
/// </summary>
public class SkillSignals : MonoBehaviour
{
    // The idle signal is raised this often while the hero stands, in seconds.
    private const float IdleReportSeconds = 1f;

    [SerializeField] private Hope hope;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private SkillTracker tracker;

    [Header("Listens to")]
    [SerializeField] private GameEvent playerCaught;
    [SerializeField] private GameEvent openingFinished;
    [SerializeField] private GameEvent levelWon;
    [SerializeField] private GameEvent levelLost;
    [SerializeField] private TextEvent memoryFound;

    [Header("Raises")]
    [SerializeField] private CaughtEvent caught;
    [SerializeField] private StageStartedEvent stageStarted;
    [SerializeField] private StageEndedEvent stageEnded;
    [SerializeField] private HopeChangedEvent hopeChanged;
    [SerializeField] private PlayerIdleSignal playerIdle;

    private bool stageRunning;
    private int lastHope;
    private float idleSeconds;
    private float nextIdleReport;

    private void OnEnable()
    {
        playerCaught.Subscribe(OnPlayerCaught);
        openingFinished.Subscribe(OnOpeningFinished);
        levelWon.Subscribe(OnLevelEnded);
        levelLost.Subscribe(OnLevelEnded);
        memoryFound.Subscribe(OnMemoryFound);
        hope.Changed += OnHopeChanged;
    }

    private void OnDisable()
    {
        playerCaught.Unsubscribe(OnPlayerCaught);
        openingFinished.Unsubscribe(OnOpeningFinished);
        levelWon.Unsubscribe(OnLevelEnded);
        levelLost.Unsubscribe(OnLevelEnded);
        memoryFound.Unsubscribe(OnMemoryFound);
        hope.Changed -= OnHopeChanged;
    }

    private void Update()
    {
        if (!stageRunning)
        {
            return;
        }

        if (player.IsWalking)
        {
            if (idleSeconds > 0f)
            {
                idleSeconds = 0f;
                playerIdle.Raise(0f); // Tells the tracker the idle ended, so the next long one counts again.
            }
            return;
        }

        idleSeconds += Time.deltaTime;
        if (Time.time >= nextIdleReport)
        {
            nextIdleReport = Time.time + IdleReportSeconds;
            playerIdle.Raise(idleSeconds);
        }
    }

    private void OnOpeningFinished()
    {
        stageRunning = true;
        idleSeconds = 0f;
        lastHope = hope.Current;
        stageStarted.Raise();
    }

    private void OnLevelEnded()
    {
        if (!stageRunning)
        {
            return;
        }
        stageRunning = false;
        stageEnded.Raise();
    }

    private void OnPlayerCaught()
    {
        if (stageRunning)
        {
            caught.Raise();
        }
    }

    private void OnMemoryFound(string memory)
    {
        tracker.NoteMemoryUsed();
    }

    private void OnHopeChanged(int newHope)
    {
        hopeChanged.Raise(lastHope, newHope);
        lastHope = newHope;
    }
}
