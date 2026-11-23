using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The first screens of the game, before the hero wakes up. First the content warning (design 3.3),
/// only until the player accepts it once. Then the three difficulty levels (design 1.2).
/// Picking a level tells the DifficultyManager and raises startChosen, and the opening begins.
/// </summary>
public class StartMenu : MonoBehaviour
{
    // The same layer as the content warning: above the HUD, below the end screen.
    private const int CanvasSortingOrder = 50;

    // A dark, nearly opaque backdrop, the same as the warning's, so the two screens feel like one.
    private static readonly Color BackdropColor = new Color(0.02f, 0.02f, 0.03f, 0.95f);

    // Layout in UI pixels, from the middle of the screen. Up is plus, down is minus.
    private const float TextWidth = 1400f;
    private const float QuestionHeight = 90f;
    private const int QuestionFontSize = 56;
    private const float QuestionY = 330f;
    private static readonly Vector2 ButtonSize = new Vector2(620f, 84f);
    private const int ButtonFontSize = 38;
    private const float FirstButtonY = 170f;
    private const float ButtonGap = 190f;
    private const float NoteHeight = 50f;
    private const int NoteFontSize = 26;
    private const float NoteBelowButton = 68f;
    private const float KeysHintY = -470f;

    [SerializeField] private ContentWarningScreen warning;
    [SerializeField] private ContentIntensityManager intensityManager;
    [SerializeField] private DifficultyManager difficulty;

    [Tooltip("The live config. The levels are offered only after it is ready, so the chosen level gets its numbers")]
    [SerializeField] private LiveConfigLoader liveConfig;

    [Header("The words")]
    [SerializeField] private string question = "How heavy are the days?";
    [SerializeField] private string[] levelNames = { "Day One", "The Tunnel", "I Lost the Festival" };
    [SerializeField]
    private string[] levelNotes =
    {
        "More hope to start with. The eyes on the walls see less, and the guard walks slowly.",
        "The way it was meant to be. It watches how I do, and gives or takes a little.",
        "Little hope, and every capture takes much of it. The eyes see wide.",
    };
    [SerializeField] private string keysHint = "O  options        F3  what the game is doing";

    [Header("Listens to")]
    [SerializeField] private GameEvent warningAccepted;
    [SerializeField] private GameEvent warningDeclined;
    [SerializeField] private GameEvent liveConfigLoaded;

    [Header("Raises")]
    [Tooltip("Raised after the player picked a difficulty. The opening starts on it")]
    [SerializeField] private GameEvent startChosen;

    private GameObject picker;
    private bool warningDone;
    private bool pickerShown;

    private void OnEnable()
    {
        warningAccepted.Subscribe(OnWarningAccepted);
        warningDeclined.Subscribe(OnWarningDeclined);
        liveConfigLoaded.Subscribe(ShowPickerWhenReady);
    }

    private void OnDisable()
    {
        warningAccepted.Unsubscribe(OnWarningAccepted);
        warningDeclined.Unsubscribe(OnWarningDeclined);
        liveConfigLoaded.Unsubscribe(ShowPickerWhenReady);
    }

    private void Start()
    {
        // The saved answer and not the asset's field: in the editor the asset remembers the last play,
        // and a player who never accepted the warning must see it.
        if (PlayerPrefs.GetInt(ContentIntensityManager.ContentWarningShownKey, 0) != 0)
        {
            warning.gameObject.SetActive(false); // Accepted in an earlier run.
            warningDone = true;
            ShowPickerWhenReady();
        }
    }

    private void OnWarningAccepted()
    {
        intensityManager.Save(); // The warning screen changed the settings; this remembers them.
        warning.gameObject.SetActive(false);
        warningDone = true;
        ShowPickerWhenReady();
    }

    // The levels wait for two things: the warning is behind us, and the live config is ready
    // (downloaded or given up on). Whichever comes last calls this and the picker shows.
    private void ShowPickerWhenReady()
    {
        if (!warningDone || !liveConfig.IsReady || pickerShown)
        {
            return;
        }
        pickerShown = true;
        ShowPicker();
    }

    // There is no menu to go back to, so "Back" leaves the game. In the editor and in a browser it does nothing.
    private void OnWarningDeclined()
    {
        Application.Quit();
    }

    private void ShowPicker()
    {
        UiFactory.EnsureEventSystem();
        Transform canvas = UiFactory.CreateCanvas("Difficulty Picker", CanvasSortingOrder).transform;
        canvas.SetParent(transform, false);
        picker = canvas.gameObject;

        Image backdrop = UiFactory.CreateImage(canvas, "Backdrop", null, BackdropColor);
        UiFactory.Stretch(backdrop.rectTransform);
        backdrop.raycastTarget = true; // Clicks don't reach the level behind.

        Text title = UiFactory.CreateTitle(canvas, "Question", UiFactory.Center, new Vector2(TextWidth, QuestionHeight), QuestionFontSize, UiTheme.TextColor);
        Center(title.rectTransform, QuestionY);
        title.text = question;

        Difficulty[] levels = { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard };
        for (int i = 0; i < levels.Length; i++)
        {
            Difficulty level = levels[i]; // A copy for the click below: the loop's own variable changes.
            float y = FirstButtonY - i * ButtonGap;

            Button button = UiFactory.CreateButton(canvas, levelNames[i], UiFactory.Center, ButtonSize, ButtonFontSize, Color.white);
            Center((RectTransform)button.transform, y);
            button.onClick.AddListener(() => Choose(level));

            Text note = UiFactory.CreateText(canvas, levelNames[i] + " Note", UiFactory.Center, new Vector2(TextWidth, NoteHeight), NoteFontSize, TextAnchor.MiddleCenter);
            Center(note.rectTransform, y - NoteBelowButton);
            note.color = UiTheme.Muted;
            note.text = levelNotes[i];
        }

        Text keys = UiFactory.CreateText(canvas, "Keys", UiFactory.Center, new Vector2(TextWidth, NoteHeight), NoteFontSize, TextAnchor.MiddleCenter);
        Center(keys.rectTransform, KeysHintY);
        keys.color = UiTheme.Muted;
        keys.text = keysHint;
    }

    private void Choose(Difficulty level)
    {
        difficulty.Choose(level);
        Destroy(picker);
        startChosen.Raise();
    }

    private static void Center(RectTransform rect, float y)
    {
        rect.pivot = UiFactory.Center;
        rect.anchoredPosition = new Vector2(0f, y);
    }
}
