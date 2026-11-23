using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A page of text that shows what the hidden systems are doing: the difficulty and its dynamic mode,
/// the skill score, the hope and its tier, the calm, the optional moments and the missions.
/// The player never needs it (the design keeps all of this silent), so it is off until its key is pressed.
/// It is for the team, to check the systems and to present them.
/// </summary>
public class DevOverlay : MonoBehaviour
{
    // Above the level's UI, below the options.
    private const int CanvasSortingOrder = 68;

    // In UI pixels, from the top left corner of the screen.
    private static readonly Vector2 PageOffset = new Vector2(30f, -220f);
    private static readonly Vector2 PageSize = new Vector2(760f, 640f);
    private const int FontSize = 22;
    private static readonly Color PageColor = new Color(0f, 0f, 0f, 0.6f);

    [SerializeField] private Hope hope;
    [SerializeField] private ContinuousHope continuousHope;
    [SerializeField] private HopeBody body;
    [SerializeField] private PlayerMovementLock movementLock;
    [SerializeField] private SkillTracker tracker;
    [SerializeField] private DifficultyModulator modulator;
    [SerializeField] private SlowMomentsDirector calm;
    [SerializeField] private AttentionDirector attention;
    [SerializeField] private LiveConfigLoader liveConfig;
    [SerializeField] private PowerOutage powerOutage;

    [Tooltip("Shows and hides the page. Default: the keyboard F3 key")]
    [SerializeField]
    private InputAction toggleAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/f3");

    private GameObject page;
    private Text text;
    private readonly StringBuilder lines = new StringBuilder();

    private void Awake()
    {
        Transform canvas = UiFactory.CreateCanvas("Dev Overlay", CanvasSortingOrder).transform;
        canvas.SetParent(transform, false);
        page = canvas.gameObject;

        Image background = UiFactory.CreateImage(canvas, "Page", null, PageColor);
        UiFactory.Place(background.rectTransform, UiFactory.TopLeft, PageOffset, PageSize);

        text = UiFactory.CreateText(background.transform, "Text", UiFactory.TopLeft, PageSize, FontSize, TextAnchor.UpperLeft);
        page.SetActive(false);
    }

    private void OnEnable()
    {
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        toggleAction.Disable();
    }

    private void Update()
    {
        if (toggleAction.WasPressedThisFrame())
        {
            page.SetActive(!page.activeSelf);
        }

        if (page.activeSelf)
        {
            text.text = Describe();
        }
    }

    /// <summary>The page as text. Public so a play test can read the same numbers the page shows.</summary>
    public string Describe()
    {
        lines.Clear();
        IDifficultyProfile profile = DifficultyManager.Current;
        if (profile != null)
        {
            lines.AppendLine($" difficulty: {profile.Difficulty}   mode: {modulator.CurrentMode}   skill score: {tracker.CurrentScore:0.0}");
            lines.AppendLine($" capture costs {profile.HopeLostWhenCaught} hope   decay {profile.HopeNaturalDecayPerMinute:0.#}/min");
            lines.AppendLine($" camera cone {profile.CameraConeDegrees:0}   guard loop {profile.GuardPatrolCycleSeconds:0.#}s");
        }
        lines.AppendLine($" hope: {continuousHope.Value:0.0} ({hope.Current})   body: {body.Current}");
        lines.AppendLine($" calm: {calm.CalmEnough}   goal light: {(attention.CurrentBeat != null ? attention.CurrentBeat.Name : "-")}");
        lines.AppendLine($" held by: {(movementLock.IsFree ? "nothing" : string.Join(", ", movementLock.ActiveReasons))}");

        if (OptionalContent.Instance != null)
        {
            lines.AppendLine($" optional moments: {string.Join(", ", OptionalContent.Instance.AllRecorded())}");
        }
        if (OptionalMissions.Instance != null)
        {
            RunState state = OptionalMissions.Instance.State;
            lines.AppendLine($" memories {state.memoriesFound}/{state.totalMemories}   captures {state.capturesThisStage}");
            lines.Append(" missions done:");
            foreach (OptionalMission mission in OptionalMissions.Instance.CompletedMissions())
            {
                lines.Append(' ').Append(mission.Id);
            }
            lines.AppendLine();
        }

        lines.AppendLine($" live config: {(liveConfig.CameFromTheFile ? "from the file, version " + liveConfig.Config.version : "not loaded")}   power outage: {powerOutage.IsOn}");

        AccessibilitySettings s = AccessibilitySettings.Current;
        lines.AppendLine($" subtitles {s.SubtitlesEnabled}  arrows {s.VisualCuesForSound}  contrast {s.HighContrast}  shapes {s.ColorblindFriendly}");
        lines.AppendLine($" tap {s.TapInsteadOfHold}  less flashing {s.ReducedFlashing}  no shake {s.NoScreenShake}  floor {s.MinimumBrightness:0.00}");
        return lines.ToString();
    }
}
