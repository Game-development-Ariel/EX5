using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for EndingSequence and EndingBeat. The focus is iteration order and the
/// timing math the director relies on; the director's own tests cover the variant hooks.
/// </summary>
public class EndingSequenceTests : EditModeTest
{
    [Test]
    public void DefaultSequence_HasTheSixNamedBeatsInOrder()
    {
        EndingSequence sequence = Track(EndingSequence.CreateDefault());

        var names = new List<string>();
        foreach (EndingBeat beat in sequence.Beats)
        {
            names.Add(beat.Name);
        }

        CollectionAssert.AreEqual(
            new[]
            {
                EndingSequence.BeatNames.Stars,
                EndingSequence.BeatNames.Capture,
                EndingSequence.BeatNames.ChairWeek,
                EndingSequence.BeatNames.LongCount,
                EndingSequence.BeatNames.Release,
                EndingSequence.BeatNames.NarratorFinale,
            },
            names);
    }

    [Test]
    public void DefaultSequence_TotalDurationIsThePerBeatSum()
    {
        EndingSequence sequence = Track(EndingSequence.CreateDefault());

        float expected = 0f;
        foreach (EndingBeat beat in sequence.Beats)
        {
            expected += beat.Duration;
        }

        Assert.AreEqual(expected, sequence.TotalDuration(), 0.001f);
    }

    [Test]
    public void TotalDuration_IsZeroWhenThereAreNoBeats()
    {
        EndingSequence sequence = Track(ScriptableObject.CreateInstance<EndingSequence>());

        Assert.AreEqual(0f, sequence.TotalDuration());
    }

    [Test]
    public void UseBeats_ReplacesTheList()
    {
        EndingSequence sequence = Track(EndingSequence.CreateDefault());

        sequence.UseBeats(new[]
        {
            new EndingBeat("only", 3f, "אחד", "one"),
        });

        Assert.AreEqual(1, sequence.Count);
        Assert.AreEqual("only", sequence.At(0).Name);
    }

    [Test]
    public void At_ReturnsTheBeatAtThePosition()
    {
        EndingSequence sequence = Track(EndingSequence.CreateDefault());

        Assert.AreEqual(EndingSequence.BeatNames.Stars, sequence.At(0).Name);
        Assert.AreEqual(EndingSequence.BeatNames.NarratorFinale, sequence.At(sequence.Count - 1).Name);
    }

    [Test]
    public void Beat_DurationIsTheSerializedField()
    {
        EndingBeat beat = new EndingBeat("Capture", 4.5f);

        Assert.AreEqual(4.5f, beat.Duration);
    }

    [Test]
    public void Beat_SubtitleReturnsHebrew_WhenLanguageIsHebrew()
    {
        EndingBeat beat = new EndingBeat("Stars", 1f, "כוכבים", "Stars");

        Assert.AreEqual("כוכבים", beat.Subtitle(EndingLanguage.Hebrew));
    }

    [Test]
    public void Beat_SubtitleReturnsEnglish_WhenLanguageIsEnglish()
    {
        EndingBeat beat = new EndingBeat("Stars", 1f, "כוכבים", "Stars");

        Assert.AreEqual("Stars", beat.Subtitle(EndingLanguage.English));
    }

    [Test]
    public void Beat_SubtitleFallsBack_WhenThePickedLanguageIsEmpty()
    {
        EndingBeat heOnly = new EndingBeat("HeOnly", 1f, "בלבד", string.Empty);
        EndingBeat enOnly = new EndingBeat("EnOnly", 1f, string.Empty, "Only");

        Assert.AreEqual("בלבד", heOnly.Subtitle(EndingLanguage.English));
        Assert.AreEqual("Only", enOnly.Subtitle(EndingLanguage.Hebrew));
    }

    [Test]
    public void Beat_OnEnterStartsEmptyAndAcceptsListeners()
    {
        int fired = 0;
        EndingBeat beat = new EndingBeat("Test", 1f);

        beat.OnEnter.AddListener(() => fired++);
        beat.OnEnter.Invoke();

        Assert.AreEqual(1, fired);
    }

    [Test]
    public void Sequence_IteratesTopToBottom()
    {
        EndingSequence sequence = Track(EndingSequence.CreateDefault());

        var visited = new List<string>();
        for (int i = 0; i < sequence.Count; i++)
        {
            visited.Add(sequence.At(i).Name);
        }

        CollectionAssert.AreEqual(new[]
        {
            EndingSequence.BeatNames.Stars,
            EndingSequence.BeatNames.Capture,
            EndingSequence.BeatNames.ChairWeek,
            EndingSequence.BeatNames.LongCount,
            EndingSequence.BeatNames.Release,
            EndingSequence.BeatNames.NarratorFinale,
        }, visited);
    }

    [Test]
    public void BonusNarrationLibrary_DefaultHasTheStage1NoCapturesLine()
    {
        BonusNarrationLibrary library = Track(BonusNarrationLibrary.CreateDefault());

        Assert.IsTrue(library.Has("stage1", BonusNarrationLibrary.Conditions.NoCaptures));
        Assert.AreEqual("ובערב הוא ישן טוב.",
            library.Lookup("stage1", BonusNarrationLibrary.Conditions.NoCaptures, EndingLanguage.Hebrew));
    }

    [Test]
    public void BonusNarrationLibrary_LookupPicksTheLanguage()
    {
        BonusNarrationLibrary library = Track(ScriptableObject.CreateInstance<BonusNarrationLibrary>());
        library.Add(new BonusNarrationLibrary.Entry("stage2.no-captures", "עברית", "English"));

        Assert.AreEqual("עברית", library.Lookup("stage2", "no-captures", EndingLanguage.Hebrew));
        Assert.AreEqual("English", library.Lookup("stage2", "no-captures", EndingLanguage.English));
    }

    [Test]
    public void BonusNarrationLibrary_LookupReturnsEmptyForUnknownKey()
    {
        BonusNarrationLibrary library = Track(BonusNarrationLibrary.CreateDefault());

        Assert.AreEqual(string.Empty, library.Lookup("stage9", "unknown-condition", EndingLanguage.Hebrew));
    }
}
