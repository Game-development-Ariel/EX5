using UnityEngine;

/// <summary>
/// Decides when the level is played: the player can't move during the opening or for a moment after being caught,
/// and when the level ends the player and the watchers stop at once, and the level stops after the fade to black.
/// This is game logic, so the UI only shows these moments and never turns the player on or off itself.
/// </summary>
public class LevelFreeze : MonoBehaviour
{
    [SerializeField] private TunnelLevelSettings settings;
    [SerializeField] private PlayerMovement player;

    [Tooltip("The parent of everything that can see the player. Its VisionSensors stop looking when the level ends")]
    [SerializeField] private Transform watchers;

    [Tooltip("Turned off when the screen is black after the level ends: the player, the watchers, the sounds")]
    [SerializeField] private GameObject[] stopWhenLevelEnds;

    [Header("Listens to")]
    [SerializeField] private GameEvent openingFinished;
    [SerializeField] private GameEvent playerCaught;
    [SerializeField] private GameEvent levelWon;
    [SerializeField] private GameEvent levelLost;

    // The reasons this class gives the movement lock. Other systems hold the player with their own reasons.
    private const string OpeningReason = "opening";
    private const string CaughtReason = "caught";
    private const string EndedReason = "level ended";

    private PlayerMovementLock movementLock;
    private float freeAt;

    private void Awake()
    {
        // The lock decides if the player moves, so two systems never fight over PlayerMovement.enabled.
        movementLock = player.GetComponent<PlayerMovementLock>();
        if (movementLock == null)
        {
            movementLock = player.gameObject.AddComponent<PlayerMovementLock>();
        }
        movementLock.Hold(OpeningReason); // Until the opening ends.
    }

    private void OnEnable()
    {
        openingFinished.Subscribe(OnOpeningFinished);
        playerCaught.Subscribe(OnPlayerCaught);
        levelWon.Subscribe(OnLevelEnded);
        levelLost.Subscribe(OnLevelEnded);
    }

    private void OnDisable()
    {
        openingFinished.Unsubscribe(OnOpeningFinished);
        playerCaught.Unsubscribe(OnPlayerCaught);
        levelWon.Unsubscribe(OnLevelEnded);
        levelLost.Unsubscribe(OnLevelEnded);
    }

    // The capture holds the player until freeAt. Holding or releasing a reason twice changes nothing.
    private void Update()
    {
        if (Time.time < freeAt)
        {
            movementLock.Hold(CaughtReason);
        }
        else
        {
            movementLock.Release(CaughtReason);
        }
    }

    private void OnOpeningFinished()
    {
        movementLock.Release(OpeningReason);
    }

    private void OnPlayerCaught()
    {
        freeAt = Time.time + settings.caughtFreezeSeconds;
    }

    private void OnLevelEnded()
    {
        movementLock.Hold(EndedReason);
        foreach (VisionSensor sensor in watchers.GetComponentsInChildren<VisionSensor>())
        {
            sensor.enabled = false;
        }
        Invoke(nameof(StopLevel), settings.endFadeSeconds);
    }

    private void StopLevel()
    {
        foreach (GameObject levelObject in stopWhenLevelEnds)
        {
            levelObject.SetActive(false);
        }
    }
}
