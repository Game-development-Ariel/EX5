using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows the lines of the EndingDirector as subtitles, one after the other. The director raises a beat's
/// line and all its bonus lines in the same moment, and the subtitle manager keeps only a short queue,
/// so the presenter holds the lines and hands them over one at a time.
/// </summary>
public class EndingPresenter : MonoBehaviour
{
    [SerializeField] private EndingDirector director;
    [SerializeField] private SubtitleManager subtitles;

    [Tooltip("Seconds each line of the ending stays on the screen")]
    [SerializeField] private float lineSeconds = 4f;

    private readonly Queue<string> lines = new Queue<string>();
    private Coroutine showing;

    private void OnEnable()
    {
        director.SubtitleShown.AddListener(Add);
        director.BonusNarrationPlayed.AddListener(Add);
    }

    private void OnDisable()
    {
        director.SubtitleShown.RemoveListener(Add);
        director.BonusNarrationPlayed.RemoveListener(Add);
    }

    private void Add(string line)
    {
        lines.Enqueue(line);
        if (showing == null)
        {
            showing = StartCoroutine(ShowLines());
        }
    }

    private IEnumerator ShowLines()
    {
        while (lines.Count > 0)
        {
            subtitles.Say(lines.Dequeue(), lineSeconds);
            yield return null; // The manager starts showing in the same frame; one frame later IsShowing is true.
            while (subtitles.IsShowing)
            {
                yield return null;
            }
        }
        showing = null;
    }
}
