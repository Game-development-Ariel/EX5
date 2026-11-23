using System.Collections;
using UnityEngine;

/// <summary>
/// Holds the opening (the title card) until the start menu is done. If the player asked to skip
/// the opening scene (design 3.3), the title card never plays and the level starts at once.
/// </summary>
public class OpeningGate : MonoBehaviour
{
    [Tooltip("The opening. It waits, turned off, until the start menu is done")]
    [SerializeField] private TitleCard titleCard;

    [Tooltip("Knows if the player asked to skip the opening")]
    [SerializeField] private OpeningSkipRouter skipRouter;

    [Header("Listens to")]
    [SerializeField] private GameEvent startChosen;

    [Tooltip("Raised by the skip router when the player asked to skip the opening")]
    [SerializeField] private GameEvent skipOpening;

    [Header("Raises")]
    [Tooltip("Raised here only when the opening is skipped. Otherwise the title card raises it")]
    [SerializeField] private GameEvent openingFinished;

    // A component that is off when the scene starts doesn't run its Start until it is turned on.
    private void Awake()
    {
        titleCard.enabled = false;
        skipRouter.enabled = false;
    }

    private bool skipped;

    private void OnEnable()
    {
        startChosen.Subscribe(OnStartChosen);
        skipOpening.Subscribe(OnSkipOpening);
    }

    private void OnDisable()
    {
        startChosen.Unsubscribe(OnStartChosen);
        skipOpening.Unsubscribe(OnSkipOpening);
    }

    private void OnSkipOpening()
    {
        skipped = true;
    }

    private void OnStartChosen()
    {
        StartCoroutine(Open());
    }

    private IEnumerator Open()
    {
        skipRouter.enabled = true;
        yield return null; // The router decides in its Start, which runs before the next frame, and raises skipOpening.

        if (skipped)
        {
            openingFinished.Raise();
        }
        else
        {
            titleCard.enabled = true;
        }
    }
}
