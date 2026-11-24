using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The ordered list of ending beats, saved as an asset so designers can swap or reorder them
/// without opening the director. The director plays the list top to bottom. A separate asset
/// per ending variant would let two sequences live side-by-side; today the game ships one.
/// From an early draft of the ending (the long arc): stars, capture, chair week, day count 141-738, release, narrator finale.
/// The design doc (topic 7) is now the source; only the tests build this full sequence.
/// </summary>
[CreateAssetMenu(menuName = "Tunnel/Ending Sequence")]
public class EndingSequence : ScriptableObject
{
    // Seconds each beat of the full game's ending stays, in order.
    private const float StarsSeconds = 6f;
    private const float CaptureSeconds = 4f;
    private const float ChairWeekSeconds = 5f;
    private const float LongCountSeconds = 8f;
    private const float ReleaseSeconds = 4f;
    private const float NarratorFinaleSeconds = 10f;

    [Tooltip("The ordered beats of the ending. The director plays them one after another")]
    [SerializeField] private List<EndingBeat> beats = new List<EndingBeat>();

    /// <summary>The ordered beats of the ending.</summary>
    public IReadOnlyList<EndingBeat> Beats => beats;

    /// <summary>How many beats this sequence holds. Convenience for the tests.</summary>
    public int Count => beats.Count;

    /// <summary>The beat at the given position in the list. Zero-based.</summary>
    public EndingBeat At(int index)
    {
        return beats[index];
    }

    /// <summary>
    /// The sum of every beat's Duration, in seconds. Used by the director to size its progress
    /// bar in the editor and by tests that assert the ending fits in a reasonable window.
    /// </summary>
    public float TotalDuration()
    {
        float total = 0f;
        foreach (EndingBeat beat in beats)
        {
            if (beat != null)
            {
                total += beat.Duration;
            }
        }
        return total;
    }

    /// <summary>Lets the factory and tests assemble a sequence without the inspector.</summary>
    public void UseBeats(IEnumerable<EndingBeat> newBeats)
    {
        beats.Clear();
        if (newBeats == null)
        {
            return;
        }
        beats.AddRange(newBeats);
    }

    /// <summary>
    /// Convenience: builds the default ending from the long arc of the early draft.
    /// The named beats are the keys the EndingDirector switches on; the strings are the
    /// Hebrew and English subtitles agreed with the writers in the end-credits file.
    /// </summary>
    public static EndingSequence CreateDefault()
    {
        EndingSequence sequence = ScriptableObject.CreateInstance<EndingSequence>();
        sequence.beats.AddRange(new[]
        {
            new EndingBeat(BeatNames.Stars, StarsSeconds,
                "הַבֶּט נָא הַשָּׁמַיְמָה וּסְפֹר הַכּוֹכָבִים.",
                "Look up at the sky and count the stars."),
            new EndingBeat(BeatNames.Capture, CaptureSeconds,
                "ידיים תופסות את הקרסוליים.",
                "Hands grab his ankles."),
            new EndingBeat(BeatNames.ChairWeek, ChairWeekSeconds,
                "שבוע.",
                "A week."),
            new EndingBeat(BeatNames.LongCount, LongCountSeconds,
                "יום מאה ארבעים ואחת. יום שבע מאות שלושים ושמונה.",
                "Day 141. Day 738."),
            new EndingBeat(BeatNames.Release, ReleaseSeconds,
                "דלת. אור לבן.",
                "A door. White light."),
            new EndingBeat(BeatNames.NarratorFinale, NarratorFinaleSeconds,
                "וביום השבע מאות שלושים ושמונה, הבור נפתח. הוא ספר, והם היו יותר מהימים.",
                "On the seven hundred thirty-eighth day, the pit opened. He counted, and they were more than the days."),
        });
        return sequence;
    }

    /// <summary>The canonical beat names the director reads. Kept here so a typo is caught at compile time.</summary>
    public static class BeatNames
    {
        public const string Stars = "Stars";
        public const string Capture = "Capture";
        public const string ChairWeek = "ChairWeek";
        public const string LongCount = "LongCount";
        public const string Release = "Release";
        public const string NarratorFinale = "NarratorFinale";
    }
}
