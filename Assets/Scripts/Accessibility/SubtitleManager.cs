using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The subtitles canvas. Draws one line of text at the bottom of the screen, slightly above the
/// HintToast band, and keeps a short queue (two entries) for lines that arrive while another is
/// still showing. Section 2.3's exception for accessibility: the design forbids pop-ups, but
/// subtitles are an ethical baseline and the only on-screen text that may appear during live play.
/// With subtitlesEnabled off the manager still queues (so systems can be simple) but draws nothing.
/// </summary>
public class SubtitleManager : MonoBehaviour
{
    // Above the HUD (10) and the visual cue overlay (15), below the HintToast band (20).
    private const int CanvasSortingOrder = 18;

    private const int MaxQueueLength = 2;
    private const int FontSize = 28;
    private const float FadeSeconds = 0.25f;

    // Just above the HintToast band (40 + 150) so the two never overlap.
    private static readonly Vector2 Offset = new Vector2(0f, 210f);
    private static readonly Vector2 Size = new Vector2(1500f, 72f);

    [Tooltip("The event other systems raise to show a subtitle")]
    [SerializeField] private SubtitleRequestedEvent subtitleRequested;

    private CanvasGroup group;
    private Text message;
    private readonly Queue<SubtitleRequest> pending = new Queue<SubtitleRequest>();
    private Coroutine showing;

    /// <summary>Public so tests can look at the queue without reflection.</summary>
    public int QueueLength => pending.Count;

    /// <summary>True while a subtitle is being drawn or fading. Public so tests can check the state.</summary>
    public bool IsShowing => showing != null;

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("Subtitle Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);
        group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        Image band = UiFactory.CreateImage(canvas.transform, "Subtitle Band", UiTheme.Sprite("Band"), new Color(0f, 0f, 0f, 0.55f));
        UiFactory.Place(band.rectTransform, UiFactory.BottomCenter, Offset, Size);

        message = UiFactory.CreateText(band.transform, "Subtitle", UiFactory.Center, Size, FontSize, TextAnchor.MiddleCenter);
        message.rectTransform.pivot = UiFactory.Center;
    }

    private void OnEnable()
    {
        if (subtitleRequested != null)
        {
            subtitleRequested.Subscribe(Enqueue);
        }
    }

    private void OnDisable()
    {
        if (subtitleRequested != null)
        {
            subtitleRequested.Unsubscribe(Enqueue);
        }
    }

    /// <summary>Public so other code can show a subtitle without the event.</summary>
    public void Enqueue(SubtitleRequest request)
    {
        // Keep the queue short. If a new line arrives when two are waiting, the oldest is dropped.
        while (pending.Count >= MaxQueueLength)
        {
            pending.Dequeue();
        }
        pending.Enqueue(request);
        if (showing == null && isActiveAndEnabled)
        {
            showing = StartCoroutine(ShowNext());
        }
    }

    private IEnumerator ShowNext()
    {
        while (pending.Count > 0)
        {
            SubtitleRequest request = pending.Dequeue();
            if (!AccessibilitySettings.Current.SubtitlesEnabled)
            {
                // The player turned subtitles off; drop quietly and loop.
                continue;
            }
            yield return ShowOne(request);
        }
        showing = null;
    }

    private IEnumerator ShowOne(SubtitleRequest request)
    {
        message.text = FormatText(request);
        message.alignment = request.Language == SubtitleLanguage.He ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
        yield return UiAnim.Fade(group, 1f, FadeSeconds);
        yield return new WaitForSeconds(Mathf.Max(0.5f, request.Duration));
        yield return UiAnim.Fade(group, 0f, FadeSeconds);
        message.text = string.Empty;
    }

    private static string FormatText(SubtitleRequest request)
    {
        if (string.IsNullOrEmpty(request.Speaker))
        {
            return request.Text;
        }
        return "<" + request.Speaker + "> " + request.Text;
    }
}
