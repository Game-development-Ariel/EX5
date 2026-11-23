using UnityEngine;

/// <summary>
/// Holds the opening (the title card) until the start menu is done, then lets it play.
/// The skip, from the second time on, belongs to the title card itself (design 3.3).
/// </summary>
public class OpeningGate : MonoBehaviour
{
    [Tooltip("The opening. It waits, turned off, until the start menu is done")]
    [SerializeField] private TitleCard titleCard;

    [Header("Listens to")]
    [SerializeField] private GameEvent startChosen;

    [Tooltip("Raised by a live event with a new line for the opening, to show under the day's title")]
    [SerializeField] private TextEvent openingLineChanged;

    // A component that is off when the scene starts doesn't run its Start until it is turned on.
    private void Awake()
    {
        titleCard.enabled = false;
    }

    private void OnEnable()
    {
        startChosen.Subscribe(OnStartChosen);
        openingLineChanged.Subscribe(OnOpeningLineChanged);
    }

    private void OnDisable()
    {
        startChosen.Unsubscribe(OnStartChosen);
        openingLineChanged.Unsubscribe(OnOpeningLineChanged);
    }

    // The title card is still off and waiting, so the gate hands it the line.
    private void OnOpeningLineChanged(string line)
    {
        titleCard.SetSubtitle(line);
    }

    private void OnStartChosen()
    {
        titleCard.enabled = true;
    }
}
