using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The content warning that is shown the first time the game is opened. It says in Hebrew and English
/// that the game is about captivity and war, that it was reviewed with a trauma center, and that an
/// Implied mode is available. The player can tick "Skip opening scene", then Continue to play or
/// Back to go to the main menu. Once accepted, the ContentIntensity remembers it was shown.
/// </summary>
public class ContentWarningScreen : MonoBehaviour
{
    // Above the HUD and most UI; below the fader and the end screen.
    private const int CanvasSortingOrder = 50;

    // A dark, nearly opaque backdrop so the warning is read calmly and not over the game.
    private static readonly Color BackdropColor = new Color(0.02f, 0.02f, 0.03f, 0.95f);

    // Layout in UI pixels, from the middle of the screen.
    private const float TextWidth = 1400f;
    private static readonly Vector2 HebrewSize = new Vector2(TextWidth, 220f);
    private const int HebrewFontSize = 36;

    // The Hebrew is broken into lines of at most this many letters before it is written right to left.
    private const int HebrewLineLength = 60;
    private const float HebrewY = 220f;
    private static readonly Vector2 EnglishSize = new Vector2(TextWidth, 220f);
    private const int EnglishFontSize = 32;
    private const float EnglishY = -40f;

    private const float ToggleY = -220f;
    private static readonly Vector2 ToggleBoxSize = new Vector2(40f, 40f);
    private static readonly Vector2 ToggleLabelSize = new Vector2(900f, 60f);
    private const int ToggleFontSize = 28;
    private const float ToggleRowHeight = 80f;

    // The space between the check box and its label, in UI pixels.
    private const float ToggleLabelGap = 16f;

    private static readonly Vector2 ButtonSize = new Vector2(320f, 80f);
    private const int ButtonFontSize = 36;
    private const float ButtonsY = -340f;
    private const float ButtonsSpacing = 400f;

    [Tooltip("The text of the warning, in Hebrew and English")]
    [SerializeField] private ContentWarningText text;

    [Tooltip("The intensity settings that remember the choice. Falls back to the manager if empty")]
    [SerializeField] private ContentIntensity intensity;

    [Header("Raises")]
    [Tooltip("Raised when the player accepts the warning and presses Continue")]
    [SerializeField] private GameEvent warningAccepted;

    [Tooltip("Optional: raised when the player presses Back. Listeners load the main menu")]
    [SerializeField] private GameEvent warningDeclined;

    private Transform canvas;
    private Toggle skipToggle;

    private void Awake()
    {
        UiFactory.EnsureEventSystem();
        canvas = UiFactory.CreateCanvas("Content Warning", CanvasSortingOrder).transform;
        canvas.SetParent(transform, false);

        Image backdrop = UiFactory.CreateImage(canvas, "Backdrop", null, BackdropColor);
        UiFactory.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true;

        Text hebrew = UiFactory.CreateTitle(canvas, "Hebrew", UiFactory.Center, HebrewSize, HebrewFontSize, UiTheme.TextColor);
        hebrew.rectTransform.anchoredPosition = new Vector2(0f, HebrewY);
        hebrew.text = text != null ? RightToLeft.ForLegacyText(text.Hebrew, HebrewLineLength) : string.Empty;
        hebrew.alignment = TextAnchor.MiddleCenter;

        Text english = UiFactory.CreateText(canvas, "English", UiFactory.Center, EnglishSize, EnglishFontSize, TextAnchor.MiddleCenter);
        english.rectTransform.anchoredPosition = new Vector2(0f, EnglishY);
        english.text = text != null ? text.English : string.Empty;

        skipToggle = CreateSkipToggle(canvas);
        ContentIntensity current = Target();
        skipToggle.isOn = current == null || current.SkipOpening;

        Button continueButton = UiFactory.CreateButton(canvas, text != null ? text.ContinueLabel : "Continue", UiFactory.Center, ButtonSize, ButtonFontSize, Color.white);
        ((RectTransform)continueButton.transform).anchoredPosition = new Vector2(ButtonsSpacing * 0.5f, ButtonsY);
        continueButton.onClick.AddListener(OnContinue);

        Button backButton = UiFactory.CreateButton(canvas, text != null ? text.BackLabel : "Back", UiFactory.Center, ButtonSize, ButtonFontSize, Color.white);
        ((RectTransform)backButton.transform).anchoredPosition = new Vector2(-ButtonsSpacing * 0.5f, ButtonsY);
        backButton.onClick.AddListener(OnBack);
    }

    private Toggle CreateSkipToggle(Transform parent)
    {
        var holder = new GameObject("Skip Opening Toggle", typeof(RectTransform));
        holder.transform.SetParent(parent, false);
        UiFactory.Place((RectTransform)holder.transform, UiFactory.Center, new Vector2(0f, ToggleY), new Vector2(TextWidth, ToggleRowHeight));

        Image box = UiFactory.CreateImage(holder.transform, "Box", UiTheme.Sprite("Button"), Color.white);
        UiFactory.Place(box.rectTransform, UiFactory.Center, new Vector2(-ToggleLabelSize.x * 0.5f, 0f), ToggleBoxSize);
        box.raycastTarget = true;

        Image check = UiFactory.CreateImage(box.transform, "Check", UiTheme.Sprite("Check"), Color.white);
        UiFactory.Stretch(check.rectTransform);
        check.raycastTarget = false;

        Text label = UiFactory.CreateText(holder.transform, "Label", UiFactory.Center, ToggleLabelSize, ToggleFontSize, TextAnchor.MiddleLeft);
        label.rectTransform.anchoredPosition = new Vector2(ToggleBoxSize.x + ToggleLabelGap, 0f);
        label.text = text != null ? (RightToLeft.ForLegacyText(text.SkipOpeningHebrew, HebrewLineLength) + "  /  " + text.SkipOpeningEnglish) : string.Empty;

        var toggle = holder.AddComponent<Toggle>();
        toggle.targetGraphic = box;
        toggle.graphic = check;
        toggle.transition = Selectable.Transition.None;
        return toggle;
    }

    private void OnContinue()
    {
        ContentIntensity current = Target();
        if (current != null)
        {
            current.SkipOpening = skipToggle != null && skipToggle.isOn;
            current.ContentWarningShown = true;
        }
        if (warningAccepted != null)
        {
            warningAccepted.Raise();
        }
    }

    private void OnBack()
    {
        if (warningDeclined != null)
        {
            warningDeclined.Raise();
        }
    }

    private ContentIntensity Target()
    {
        return intensity != null ? intensity : ContentIntensityManager.Current;
    }
}
