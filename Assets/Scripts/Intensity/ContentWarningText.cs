using UnityEngine;

/// <summary>
/// The text of the content warning, in Hebrew and English. The words are saved as an asset so
/// they can be reviewed with the trauma center and edited without touching code.
/// </summary>
[CreateAssetMenu(menuName = "Intensity/Content Warning Text")]
public class ContentWarningText : ScriptableObject
{
    [TextArea]
    [Tooltip("The warning in Hebrew. The default is the one reviewed with the trauma center")]
    [SerializeField]
    private string hebrew = "המשחק מתאר שבי, בדידות ומלחמה. הוא נבדק עם מרכז טראומה. יש אפשרות למצב מרומז שאינו מציג אלימות על המסך.";

    [TextArea]
    [Tooltip("The warning in English")]
    [SerializeField]
    private string english = "This game depicts captivity, isolation, and war. It was reviewed with a trauma center. An Implied mode is available that does not show on-screen violence.";

    [Tooltip("The label of the skip-opening toggle, in Hebrew")]
    [SerializeField] private string skipOpeningHebrew = "דלג על סצנת הפתיחה";

    [Tooltip("The label of the skip-opening toggle, in English")]
    [SerializeField] private string skipOpeningEnglish = "Skip opening scene";

    [Tooltip("The label of the continue button")]
    [SerializeField] private string continueLabel = "Continue";

    [Tooltip("The label of the back button")]
    [SerializeField] private string backLabel = "Back";

    /// <summary>The warning in Hebrew.</summary>
    public string Hebrew => hebrew;

    /// <summary>The warning in English.</summary>
    public string English => english;

    /// <summary>The skip-opening toggle label in Hebrew.</summary>
    public string SkipOpeningHebrew => skipOpeningHebrew;

    /// <summary>The skip-opening toggle label in English.</summary>
    public string SkipOpeningEnglish => skipOpeningEnglish;

    /// <summary>The label of the continue button.</summary>
    public string ContinueLabel => continueLabel;

    /// <summary>The label of the back button.</summary>
    public string BackLabel => backLabel;
}
