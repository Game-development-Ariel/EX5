using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The content warning that is shown the first time the game is opened. It says in Hebrew and English
/// what the game is about and what the player will hear. The player presses Continue to play or Back
/// to go to the main menu. There is no skip here: the opening must be watched once (design 3.3), and
/// the TitleCard offers the skip from the second time. Once accepted, the ContentIntensity remembers it was shown.
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

        Button continueButton = UiFactory.CreateButton(canvas, text != null ? text.ContinueLabel : "Continue", UiFactory.Center, ButtonSize, ButtonFontSize, Color.white);
        ((RectTransform)continueButton.transform).anchoredPosition = new Vector2(ButtonsSpacing * 0.5f, ButtonsY);
        continueButton.onClick.AddListener(OnContinue);

        Button backButton = UiFactory.CreateButton(canvas, text != null ? text.BackLabel : "Back", UiFactory.Center, ButtonSize, ButtonFontSize, Color.white);
        ((RectTransform)backButton.transform).anchoredPosition = new Vector2(-ButtonsSpacing * 0.5f, ButtonsY);
        backButton.onClick.AddListener(OnBack);
    }

    private void OnContinue()
    {
        ContentIntensity current = Target();
        if (current != null)
        {
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
