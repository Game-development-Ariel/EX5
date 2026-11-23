using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// One beat of the end-of-game sequence. A beat is a frame in a slideshow: a prefab is
/// spawned, a narrator clip plays, an ambient clip plays, a subtitle is shown (in Hebrew or
/// English, picked at run time), and an optional UnityEvent fires - that is where the ending
/// director hooks the per-mission tweaks like "render constellations" or "sandbag on the news".
/// From an early draft of the ending: stars, capture, chair week, long count 141 to 738, release, narrator finale.
/// The design doc (topic 7) is now the source; only the tests build this full sequence.
/// </summary>
[Serializable]
public class EndingBeat
{
    [Tooltip("Human-readable name, used in logs and in the inspector. For example 'Capture', 'Long Count'")]
    [SerializeField] private string beatName;

    [Tooltip("The prefab spawned for this beat. Null means the beat is audio-only")]
    [SerializeField] private GameObject uiPrefab;

    [Tooltip("How long the beat stays on screen before the next one starts, in seconds")]
    [SerializeField] private float duration = 2f;

    [Tooltip("The narrator line played at the start of the beat. Null means no narrator on this beat")]
    [SerializeField] private AudioClip narratorClip;

    [Tooltip("Looped ambient audio for this beat. Null means the previous beat's ambience continues")]
    [SerializeField] private AudioClip ambientClip;

    [Tooltip("Subtitle in Hebrew, shown when the project language is Hebrew. From design 3.3 and the end credits")]
    [TextArea]
    [SerializeField] private string subtitleTextHe;

    [Tooltip("Subtitle in English, shown when the project language is English. From design 3.3 and the end credits")]
    [TextArea]
    [SerializeField] private string subtitleTextEn;

    [Tooltip("Fires when the beat starts, after the prefab is spawned. The director uses this hook " +
             "to flip the variant - memory stars, news film with sandbag, bracelet on the wrist")]
    [SerializeField] private UnityEvent onEnter = new UnityEvent();

    /// <summary>Human-readable name, used in logs and in the inspector.</summary>
    public string Name => beatName;

    /// <summary>The prefab spawned for this beat. Null means the beat is audio-only.</summary>
    public GameObject UiPrefab => uiPrefab;

    /// <summary>How long the beat stays on screen before the next one starts, in seconds.</summary>
    public float Duration => duration;

    /// <summary>The narrator line played at the start of the beat. Null means no narrator on this beat.</summary>
    public AudioClip NarratorClip => narratorClip;

    /// <summary>Looped ambient audio for this beat. Null means the previous beat's ambience continues.</summary>
    public AudioClip AmbientClip => ambientClip;

    /// <summary>Subtitle in Hebrew. The director picks the language at run time.</summary>
    public string SubtitleTextHe => subtitleTextHe;

    /// <summary>Subtitle in English. The director picks the language at run time.</summary>
    public string SubtitleTextEn => subtitleTextEn;

    /// <summary>Fires when the beat starts, so the director can attach variants.</summary>
    public UnityEvent OnEnter => onEnter;

    public EndingBeat()
    {
    }

    /// <summary>Lets factory code and tests build a beat without the inspector.</summary>
    public EndingBeat(string name, float duration, string subtitleHe = "", string subtitleEn = "")
    {
        this.beatName = name;
        this.duration = duration;
        this.subtitleTextHe = subtitleHe;
        this.subtitleTextEn = subtitleEn;
    }

    /// <summary>
    /// The subtitle in the picked language. Falls back to the other language if the chosen one is
    /// empty, so a beat that only has one language still shows something.
    /// </summary>
    public string Subtitle(EndingLanguage language)
    {
        if (language == EndingLanguage.Hebrew)
        {
            return string.IsNullOrEmpty(subtitleTextHe) ? subtitleTextEn : subtitleTextHe;
        }
        return string.IsNullOrEmpty(subtitleTextEn) ? subtitleTextHe : subtitleTextEn;
    }
}
