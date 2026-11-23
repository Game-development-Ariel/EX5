using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws the chevrons for the visual-cue-for-sound mode. One UI canvas sitting above the HUD;
/// each SoundCueRaised event spawns a short-lived chevron at the edge of the screen pointing from
/// the hero toward the sound, sized by loudness and fading over 0.8s. Multiple chevrons stack,
/// so three sounds in a moment show three arrows at once. The overlay is passive: if the mode is
/// off the event does not fire, so the canvas stays empty.
/// </summary>
public class VisualCueOverlay : MonoBehaviour
{
    // Above the HUD (10), below HintToast (20). Chevrons are information, not thoughts.
    private const int CanvasSortingOrder = 15;

    // Distance from the screen centre to the chevron, as a fraction of the shorter screen side.
    private const float EdgeRadius = 0.38f;

    private const float FadeSeconds = 0.8f;
    private const float MinChevronSize = 32f;
    private const float MaxChevronSize = 72f;

    [Tooltip("The event the overlay listens to. Same asset as the emitters")]
    [SerializeField] private SoundCueRaisedEvent soundCueRaised;

    [Tooltip("Max chevrons visible at the same time. The oldest fade first")]
    [SerializeField] private int maxChevrons = 6;

    private RectTransform root;
    private readonly List<Chevron> live = new List<Chevron>();

    private struct Chevron
    {
        public RectTransform Rect;
        public Image Arrow;
        public Text Label;
        public float Age;
    }

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("Visual Cue Canvas", CanvasSortingOrder);
        canvas.transform.SetParent(transform, false);
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        root = canvas.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (soundCueRaised != null)
        {
            soundCueRaised.Subscribe(OnCue);
        }
    }

    private void OnDisable()
    {
        if (soundCueRaised != null)
        {
            soundCueRaised.Unsubscribe(OnCue);
        }
    }

    private void Update()
    {
        // Fade live chevrons. Walk backward so a removal does not skip a neighbour.
        for (int i = live.Count - 1; i >= 0; i--)
        {
            Chevron c = live[i];
            c.Age += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(c.Age / FadeSeconds);
            Color arrowColor = c.Arrow.color;
            arrowColor.a = t;
            c.Arrow.color = arrowColor;
            if (c.Label != null)
            {
                Color labelColor = c.Label.color;
                labelColor.a = t;
                c.Label.color = labelColor;
            }
            if (c.Age >= FadeSeconds)
            {
                Destroy(c.Rect.gameObject);
                live.RemoveAt(i);
                continue;
            }
            live[i] = c;
        }
    }

    private void OnCue(SoundCue cue)
    {
        // The mode might have been turned off between emit and receive — stay quiet in that case.
        if (!AccessibilitySettings.Current.VisualCuesForSound)
        {
            return;
        }
        if (live.Count >= maxChevrons)
        {
            // Drop the oldest so the newest cue is always visible.
            Chevron oldest = live[0];
            Destroy(oldest.Rect.gameObject);
            live.RemoveAt(0);
        }
        live.Add(BuildChevron(cue));
    }

    private Chevron BuildChevron(SoundCue cue)
    {
        Vector2 dir = cue.Direction.sqrMagnitude > 0.0001f ? cue.Direction.normalized : Vector2.up;

        // Reference resolution is 1920x1080 inside UiFactory. The canvas is already scaled for us,
        // so EdgeRadius is in local units of the canvas, not pixels on the real screen.
        float shortSide = Mathf.Min(root.rect.width, root.rect.height);
        Vector2 position = dir * (shortSide * EdgeRadius);

        float loudness = Mathf.Clamp01(cue.Loudness);
        float size = Mathf.Lerp(MinChevronSize, MaxChevronSize, loudness);

        Image arrow = UiFactory.CreateImage(root, "Chevron", UiTheme.Sprite("Chevron"), UiTheme.TextColor);
        UiFactory.Place(arrow.rectTransform, UiFactory.Center, position, new Vector2(size, size));
        // Point the chevron in the sound's direction. Unity UI rotates clockwise from +X.
        float angleDegrees = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angleDegrees - 90f);

        Text label = null;
        if (!string.IsNullOrEmpty(cue.Label))
        {
            label = UiFactory.CreateText(arrow.transform, "Label", UiFactory.Center, new Vector2(140f, 32f), 20, TextAnchor.MiddleCenter);
            label.rectTransform.anchoredPosition = new Vector2(0f, -size);
            label.text = cue.Label;
            // Rotate the label back upright, independent of the chevron's rotation.
            label.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -(angleDegrees - 90f));
        }

        return new Chevron
        {
            Rect = arrow.rectTransform,
            Arrow = arrow,
            Label = label,
            Age = 0f,
        };
    }

    /// <summary>Used by tests to look at the chevrons without reaching into UI internals.</summary>
    public int LiveChevronCount => live.Count;
}
