using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds a shape tag beside mood-coloured cues, so colourblind players read the mood from a
/// shape rather than a colour. The design (section 2.2) ties mood to colour (faith = magenta,
/// despair = grey). This component overlays a corner icon per mood: calm = circle, worry =
/// square, despair = triangle. Non-colourblind players get the same corner icon invisibly at
/// zero alpha, so layout never jumps when a player toggles the mode.
///
/// The static ColorblindPatternFor can be called by sprite systems that choose a texture at
/// spawn time (for example a memory icon), so colour-only cues get a shape automatically.
/// </summary>
public class ColorblindFilter : MonoBehaviour
{
    /// <summary>The shapes the filter maps moods onto. Enum values are the Resources/UI sprite names.</summary>
    public enum Pattern
    {
        Circle,
        Square,
        Triangle,
    }

    // Each mood maps to one shape. The names come from the design (2.2 hope-as-body moods).
    private static readonly string[] Moods = { "faith", "steady", "worry", "despair", "calm" };
    private static readonly Pattern[] MoodShapes = { Pattern.Circle, Pattern.Circle, Pattern.Square, Pattern.Triangle, Pattern.Circle };

    [Tooltip("Where the shape tag sits on the screen. Top-right corner by default so it never covers the subtitles")]
    [SerializeField] private Vector2 anchor = new Vector2(1f, 1f);

    [Tooltip("Pixel offset from the anchor corner")]
    [SerializeField] private Vector2 offset = new Vector2(-48f, -48f);

    [Tooltip("Size of the shape tag, in UI pixels")]
    [SerializeField] private float size = 56f;

    [Tooltip("Hope component to read the mood from. Optional — without it the filter waits for SetMood calls")]
    [SerializeField] private Hope hope;

    [Tooltip("Hope effects, used to read the steady and faith thresholds. Optional — falls back to 40 and 70")]
    [SerializeField] private HopeEffects hopeEffects;

    private Image icon;
    private int lastHope = int.MinValue;

    /// <summary>Picks the shape tag for a mood name. Stable across tests and runtime.</summary>
    public static Pattern ColorblindPatternFor(string mood)
    {
        if (string.IsNullOrEmpty(mood))
        {
            return Pattern.Circle;
        }
        string normalized = mood.Trim().ToLowerInvariant();
        for (int i = 0; i < Moods.Length; i++)
        {
            if (Moods[i] == normalized)
            {
                return MoodShapes[i];
            }
        }
        // Unknown moods fall back to circle: neutral, never a false alarm.
        return Pattern.Circle;
    }

    private void Awake()
    {
        Canvas canvas = UiFactory.CreateCanvas("Colorblind Filter", 12);
        canvas.transform.SetParent(transform, false);

        icon = UiFactory.CreateImage(canvas.transform, "Shape Tag", UiTheme.Sprite("Shape_Circle"), UiTheme.TextColor);
        UiFactory.Place(icon.rectTransform, anchor, offset, new Vector2(size, size));
        icon.preserveAspect = true;
        icon.color = HiddenColor();
    }

    private void OnEnable()
    {
        GameEvent changed = AccessibilitySettings.Current.SettingsChanged;
        if (changed != null)
        {
            changed.Subscribe(RefreshVisibility);
        }
        if (hope != null)
        {
            hope.Changed += OnHopeChanged;
            OnHopeChanged(hope.Current);
        }
        RefreshVisibility();
    }

    private void OnDisable()
    {
        GameEvent changed = AccessibilitySettings.Current.SettingsChanged;
        if (changed != null)
        {
            changed.Unsubscribe(RefreshVisibility);
        }
        if (hope != null)
        {
            hope.Changed -= OnHopeChanged;
        }
    }

    private void OnHopeChanged(int newHope)
    {
        if (newHope == lastHope)
        {
            return;
        }
        lastHope = newHope;
        SetMood(MoodForHope(newHope));
    }

    /// <summary>Public so other systems (memory caption, hope bar) can push a mood by name.</summary>
    public void SetMood(string mood)
    {
        Pattern pattern = ColorblindPatternFor(mood);
        icon.sprite = SpriteFor(pattern);
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        icon.color = AccessibilitySettings.Current.ColorblindFriendly ? UiTheme.TextColor : HiddenColor();
    }

    private string MoodForHope(int currentHope)
    {
        int steady = hopeEffects != null ? hopeEffects.steadyHope : 40;
        int faith = hopeEffects != null ? hopeEffects.faithHope : 70;
        if (currentHope >= faith)
        {
            return "faith";
        }
        if (currentHope >= steady)
        {
            return "steady";
        }
        if (currentHope >= 25)
        {
            return "worry";
        }
        return "despair";
    }

    private static Sprite SpriteFor(Pattern pattern)
    {
        // Sprite names follow Assets/Resources/UI conventions. If one is missing, the fallback is the circle.
        Sprite sprite = UiTheme.Sprite("Shape_" + pattern);
        return sprite != null ? sprite : UiTheme.Sprite("Shape_Circle");
    }

    private static Color HiddenColor()
    {
        Color c = UiTheme.TextColor;
        c.a = 0f;
        return c;
    }
}
