using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The options of the game (design 3.3): the eight accessibility settings and the content intensity.
/// A key opens and closes it at any moment. While it is open the game stands still. Every row is a
/// button that shows its state in words, so nothing depends on color alone.
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    // Above everything else, the end screen and the subtitles too.
    private const int CanvasSortingOrder = 70;

    private static readonly Color BackdropColor = new Color(0.02f, 0.02f, 0.03f, 0.9f);

    // Layout in UI pixels, from the middle of the screen. Up is plus, down is minus.
    private static readonly Vector2 TitleSize = new Vector2(1000f, 90f);
    private const int TitleFontSize = 60;
    private const float TitleY = 440f;
    private static readonly Vector2 RowSize = new Vector2(900f, 68f);
    private const int RowFontSize = 30;
    private const float FirstRowY = 340f;
    private const float RowGap = 78f;

    // The brightness floor goes through these steps. The setting allows 0 to 0.3.
    private static readonly float[] BrightnessSteps = { 0f, 0.15f, 0.3f };

    // The reason this menu gives the movement lock while it is open.
    private const string LockReason = "options";

    [Tooltip("The hero's lock. He can't walk while the menu is open")]
    [SerializeField] private PlayerMovementLock movementLock;

    [Tooltip("Remembers the intensity when it changes here")]
    [SerializeField] private ContentIntensityManager intensityManager;

    [Tooltip("Opens and closes the menu. Default: the keyboard O key")]
    [SerializeField]
    private InputAction toggleAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/o");

    private GameObject panel;
    private readonly Text[] labels = new Text[RowCount];
    private float timeScaleBefore = 1f;

    // The rows, top to bottom.
    private const int SubtitlesRow = 0;
    private const int VisualCuesRow = 1;
    private const int HighContrastRow = 2;
    private const int ColorblindRow = 3;
    private const int TapRow = 4;
    private const int FlashingRow = 5;
    private const int ShakeRow = 6;
    private const int BrightnessRow = 7;
    private const int IntensityRow = 8;
    private const int CloseRow = 9;
    private const int RowCount = 10;

    /// <summary>True while the menu is on the screen.</summary>
    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Transform canvas = UiFactory.CreateCanvas("Options", CanvasSortingOrder).transform;
        canvas.SetParent(transform, false);
        panel = canvas.gameObject;

        Image backdrop = UiFactory.CreateImage(canvas, "Backdrop", null, BackdropColor);
        UiFactory.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true; // Clicks don't reach what is behind.

        Text title = UiFactory.CreateTitle(canvas, "Title", UiFactory.Center, TitleSize, TitleFontSize, UiTheme.TextColor);
        title.rectTransform.pivot = UiFactory.Center;
        title.rectTransform.anchoredPosition = new Vector2(0f, TitleY);
        title.text = "Options";

        for (int row = 0; row < RowCount; row++)
        {
            int clicked = row; // A copy for the click below: the loop's own variable changes.
            Button button = UiFactory.CreateButton(canvas, "Row " + row, UiFactory.Center, RowSize, RowFontSize, Color.white);
            ((RectTransform)button.transform).anchoredPosition = new Vector2(0f, FirstRowY - row * RowGap);
            button.onClick.AddListener(() => Click(clicked));
            labels[row] = button.GetComponentInChildren<Text>();
        }

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        toggleAction.Disable();
        if (IsOpen)
        {
            Close(); // Never leave the game standing still.
        }
    }

    private void Update()
    {
        if (!toggleAction.WasPressedThisFrame())
        {
            return;
        }

        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Open()
    {
        UiFactory.EnsureEventSystem();
        Refresh();
        panel.SetActive(true);
        timeScaleBefore = Time.timeScale;
        Time.timeScale = 0f;
        if (movementLock != null)
        {
            movementLock.Hold(LockReason);
        }
    }

    private void Close()
    {
        panel.SetActive(false);
        Time.timeScale = timeScaleBefore;
        if (movementLock != null)
        {
            movementLock.Release(LockReason);
        }
    }

    private void Click(int row)
    {
        if (row == CloseRow)
        {
            Close();
            return;
        }

        if (row == IntensityRow)
        {
            ContentIntensity intensity = ContentIntensityManager.Current;
            if (intensity != null)
            {
                intensity.Mode = intensity.Mode == IntensityLevel.Implied ? IntensityLevel.Direct : IntensityLevel.Implied;
                intensityManager.Save();
            }
            Refresh();
            return;
        }

        AccessibilitySettings s = AccessibilitySettings.Current;
        s.Set(
            row == VisualCuesRow ? !s.VisualCuesForSound : s.VisualCuesForSound,
            row == HighContrastRow ? !s.HighContrast : s.HighContrast,
            row == ColorblindRow ? !s.ColorblindFriendly : s.ColorblindFriendly,
            row == TapRow ? !s.TapInsteadOfHold : s.TapInsteadOfHold,
            row == FlashingRow ? !s.ReducedFlashing : s.ReducedFlashing,
            row == ShakeRow ? !s.NoScreenShake : s.NoScreenShake,
            row == SubtitlesRow ? !s.SubtitlesEnabled : s.SubtitlesEnabled,
            row == BrightnessRow ? NextBrightness(s.MinimumBrightness) : s.MinimumBrightness);
        s.Save(); // Saved here too, so it is kept even in a scene with no AccessibilityManager.
        Refresh();
    }

    private static float NextBrightness(float current)
    {
        for (int i = 0; i < BrightnessSteps.Length; i++)
        {
            if (BrightnessSteps[i] > current + Mathf.Epsilon)
            {
                return BrightnessSteps[i];
            }
        }
        return BrightnessSteps[0]; // After the last step comes the first again.
    }

    private void Refresh()
    {
        AccessibilitySettings s = AccessibilitySettings.Current;
        ContentIntensity intensity = ContentIntensityManager.Current;

        labels[SubtitlesRow].text = "Subtitles: " + OnOff(s.SubtitlesEnabled);
        labels[VisualCuesRow].text = "Arrows for sounds: " + OnOff(s.VisualCuesForSound);
        labels[HighContrastRow].text = "High contrast: " + OnOff(s.HighContrast);
        labels[ColorblindRow].text = "Shapes for hope (colorblind): " + OnOff(s.ColorblindFriendly);
        labels[TapRow].text = "Tap instead of hold: " + OnOff(s.TapInsteadOfHold);
        labels[FlashingRow].text = "Reduced flashing: " + OnOff(s.ReducedFlashing);
        labels[ShakeRow].text = "No screen shake: " + OnOff(s.NoScreenShake);
        labels[BrightnessRow].text = "Brightness floor: " + BrightnessName(s.MinimumBrightness);
        labels[IntensityRow].text = "Hard scenes: " + (intensity != null && intensity.Mode == IntensityLevel.Direct ? "shown" : "implied");
        labels[CloseRow].text = "Close";
    }

    private static string BrightnessName(float floor)
    {
        if (floor <= BrightnessSteps[0])
        {
            return "off";
        }
        return floor < BrightnessSteps[BrightnessSteps.Length - 1] ? "medium" : "high";
    }

    private static string OnOff(bool value)
    {
        return value ? "on" : "off";
    }
}
