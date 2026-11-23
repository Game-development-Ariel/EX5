using UnityEngine;

/// <summary>
/// The text of the content warning, in Hebrew and English, as written in the design document
/// (topic 3.3). The words are saved as an asset so they can be edited without touching code.
/// </summary>
[CreateAssetMenu(menuName = "Intensity/Content Warning Text")]
public class ContentWarningText : ScriptableObject
{
    [TextArea]
    [Tooltip("The warning in Hebrew")]
    [SerializeField]
    private string hebrew = "המשחק עוסק בשבי ובאלימות. הפתיחה כוללת צלילים של התקפה, ובמשחק יש צלילים של מכות.";

    [TextArea]
    [Tooltip("The warning in English")]
    [SerializeField]
    private string english = "This game is about captivity and violence. The opening has sounds of an attack, and the game has sounds of beatings.";

    [Tooltip("The label of the continue button")]
    [SerializeField] private string continueLabel = "Continue";

    [Tooltip("The label of the back button")]
    [SerializeField] private string backLabel = "Back";

    /// <summary>The warning in Hebrew.</summary>
    public string Hebrew => hebrew;

    /// <summary>The warning in English.</summary>
    public string English => english;

    /// <summary>The label of the continue button.</summary>
    public string ContinueLabel => continueLabel;

    /// <summary>The label of the back button.</summary>
    public string BackLabel => backLabel;
}
