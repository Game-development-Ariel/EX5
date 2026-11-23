using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Edit-mode tests for the EndingDirector. The director is driven one beat at a time with
/// EnterBeat and ApplyVariants, so no coroutines are needed; the beat sequence is built in
/// memory and the missions are swapped in with a tiny helper.
/// </summary>
public class EndingDirectorTests : EditModeTest
{
    private EndingDirector director;
    private EndingSequence sequence;
    private BonusNarrationLibrary library;
    private OptionalMissions missions;
    private ContentIntensity intensity;

    [SetUp]
    public void SetUp()
    {
        sequence = Track(ScriptableObject.CreateInstance<EndingSequence>());
        sequence.UseBeats(new List<EndingBeat>
        {
            new EndingBeat(EndingSequence.BeatNames.Stars, 0f, "כוכבים", "Stars"),
            new EndingBeat(EndingSequence.BeatNames.Capture, 0f, "ידיים", "Hands"),
            new EndingBeat(EndingSequence.BeatNames.ChairWeek, 0f, "שבוע", "A week"),
            new EndingBeat(EndingSequence.BeatNames.LongCount, 0f, "ספירה", "Count"),
            new EndingBeat(EndingSequence.BeatNames.Release, 0f, "אור", "Light"),
            new EndingBeat(EndingSequence.BeatNames.NarratorFinale, 0f, "והוא ספר", "And he counted"),
        });

        library = Track(BonusNarrationLibrary.CreateDefault());

        intensity = Track(ScriptableObject.CreateInstance<ContentIntensity>());

        missions = Make("OptionalMissions").AddComponent<OptionalMissions>();
        missions.UseMissions(OptionalMissionFactory.DefaultSet());

        director = Make("Director").AddComponent<EndingDirector>();
        TestUtils.Set(director, "sequence", sequence);
        TestUtils.Set(director, "bonusLibrary", library);
        TestUtils.Set(director, "intensity", intensity);
        TestUtils.Set(director, "stageId", "stage1");
        director.Missions = missions;
    }

    [Test]
    public void NoMissions_ApplyVariantsFiresNothing()
    {
        var counter = HookAll(director);

        director.ApplyVariants();

        Assert.AreEqual(0, counter.Constellations);
        Assert.AreEqual(0, counter.Sandbag);
        Assert.AreEqual(0, counter.ThickJournal);
        Assert.AreEqual(0, counter.Bracelet);
    }

    [Test]
    public void AllMemories_ApplyVariantsFiresRenderConstellations()
    {
        var counter = HookAll(director);
        missions.State.totalMemories = 4;
        missions.State.memoriesFound = 4;

        director.ApplyVariants();

        Assert.AreEqual(1, counter.Constellations);
    }

    [Test]
    public void SandbagWrite_ApplyVariantsFiresRenderSandbagInNews()
    {
        var counter = HookAll(director);
        missions.State.sandbagWritten = true;

        director.ApplyVariants();

        Assert.AreEqual(1, counter.Sandbag);
    }

    [Test]
    public void AllPreviousNotes_ApplyVariantsFiresRenderThickJournal()
    {
        var counter = HookAll(director);
        missions.State.totalPreviousNotes = 7;
        missions.State.previousNotesFound = 7;

        director.ApplyVariants();

        Assert.AreEqual(1, counter.ThickJournal);
    }

    [Test]
    public void BraceletFound_ApplyVariantsFiresRenderBraceletOnWrist()
    {
        var counter = HookAll(director);
        missions.State.braceletFound = true;

        director.ApplyVariants();

        Assert.AreEqual(1, counter.Bracelet);
    }

    [Test]
    public void ImpliedIntensity_SkipsTheChairWeekBeat()
    {
        intensity.Mode = IntensityLevel.Implied;
        EndingBeat chair = FindBeat(EndingSequence.BeatNames.ChairWeek);

        Assert.IsTrue(director.ShouldSkip(chair));
    }

    [Test]
    public void DirectIntensity_PlaysTheChairWeekBeat()
    {
        intensity.Mode = IntensityLevel.Direct;
        EndingBeat chair = FindBeat(EndingSequence.BeatNames.ChairWeek);

        Assert.IsFalse(director.ShouldSkip(chair));
    }

    [Test]
    public void ImpliedIntensity_PlaysEveryOtherBeat()
    {
        intensity.Mode = IntensityLevel.Implied;

        foreach (string name in new[]
                 {
                     EndingSequence.BeatNames.Stars,
                     EndingSequence.BeatNames.Capture,
                     EndingSequence.BeatNames.LongCount,
                     EndingSequence.BeatNames.Release,
                     EndingSequence.BeatNames.NarratorFinale,
                 })
        {
            Assert.IsFalse(director.ShouldSkip(FindBeat(name)), name);
        }
    }

    [Test]
    public void EnterBeat_InvokesTheBeatsOnEnterHook()
    {
        int enters = 0;
        EndingBeat beat = new EndingBeat("Hooked", 0f);
        beat.OnEnter.AddListener(() => enters++);

        director.EnterBeat(beat);

        Assert.AreEqual(1, enters);
    }

    [Test]
    public void EnterBeat_PublishesTheBeatsName()
    {
        List<string> names = HookBeatNames(director);

        director.EnterBeat(FindBeat(EndingSequence.BeatNames.Stars));
        director.EnterBeat(FindBeat(EndingSequence.BeatNames.Release));

        CollectionAssert.AreEqual(new[] { EndingSequence.BeatNames.Stars, EndingSequence.BeatNames.Release }, names);
    }

    [Test]
    public void SubtitleShown_UsesTheHebrewText_WhenLanguageIsHebrew()
    {
        List<string> subtitles = HookSubtitles(director);
        TestUtils.Set(director, "language", EndingLanguage.Hebrew);

        director.EnterBeat(FindBeat(EndingSequence.BeatNames.Stars));

        Assert.AreEqual("כוכבים", subtitles[0]);
    }

    [Test]
    public void SubtitleShown_UsesTheEnglishText_WhenLanguageIsEnglish()
    {
        List<string> subtitles = HookSubtitles(director);
        TestUtils.Set(director, "language", EndingLanguage.English);

        director.EnterBeat(FindBeat(EndingSequence.BeatNames.Stars));

        Assert.AreEqual("Stars", subtitles[0]);
    }

    [Test]
    public void NoCapturesDay_AddsTheBonusNarrationLineOnTheFinale()
    {
        List<string> lines = HookBonusNarration(director);
        missions.State.capturesThisStage = 0;

        director.EnterBeat(FindBeat(EndingSequence.BeatNames.NarratorFinale));
        TestUtils.Call(director, "PlayBonusFor", FindBeat(EndingSequence.BeatNames.NarratorFinale));

        Assert.Contains("ובערב הוא ישן טוב.", lines);
    }

    [Test]
    public void NoCapturesDay_BonusDoesNotPlayOnEarlierBeats()
    {
        List<string> lines = HookBonusNarration(director);
        missions.State.capturesThisStage = 0;

        TestUtils.Call(director, "PlayBonusFor", FindBeat(EndingSequence.BeatNames.Stars));
        TestUtils.Call(director, "PlayBonusFor", FindBeat(EndingSequence.BeatNames.LongCount));

        CollectionAssert.IsEmpty(lines);
    }

    [Test]
    public void OnEnable_SubscribesToTheEndingTriggeredEvent()
    {
        GameEvent endingTriggered = Track(TestData.Event("EndingTriggered"));
        TestUtils.Set(director, "endingTriggered", endingTriggered);
        TestUtils.Call(director, "OnEnable");

        Assert.DoesNotThrow(() => endingTriggered.Raise());
    }

    [Test]
    public void StartsEnding_FiresEveryVariantHookForTheCompletedMissions()
    {
        var counter = HookAll(director);
        missions.State.totalMemories = 4;
        missions.State.memoriesFound = 4;
        missions.State.sandbagWritten = true;
        missions.State.braceletFound = true;
        missions.State.totalPreviousNotes = 3;
        missions.State.previousNotesFound = 3;

        director.ApplyVariants();

        Assert.AreEqual(1, counter.Constellations);
        Assert.AreEqual(1, counter.Sandbag);
        Assert.AreEqual(1, counter.Bracelet);
        Assert.AreEqual(1, counter.ThickJournal);
    }

    private EndingBeat FindBeat(string name)
    {
        foreach (EndingBeat beat in sequence.Beats)
        {
            if (beat != null && beat.Name == name)
            {
                return beat;
            }
        }
        Assert.Fail("No beat named " + name);
        return null;
    }

    private static VariantCounter HookAll(EndingDirector director)
    {
        var counter = new VariantCounter();
        AddUnityEventListener(director, "renderConstellations", () => counter.Constellations++);
        AddUnityEventListener(director, "renderSandbagInNews", () => counter.Sandbag++);
        AddUnityEventListener(director, "renderThickJournal", () => counter.ThickJournal++);
        AddUnityEventListener(director, "renderBraceletOnWrist", () => counter.Bracelet++);
        return counter;
    }

    private static List<string> HookBeatNames(EndingDirector director)
    {
        var list = new List<string>();
        AddUnityEventListener<string>(director, "beatEntered", list.Add);
        return list;
    }

    private static List<string> HookSubtitles(EndingDirector director)
    {
        var list = new List<string>();
        AddUnityEventListener<string>(director, "subtitleShown", list.Add);
        return list;
    }

    private static List<string> HookBonusNarration(EndingDirector director)
    {
        var list = new List<string>();
        AddUnityEventListener<string>(director, "bonusNarrationPlayed", list.Add);
        return list;
    }

    private static void AddUnityEventListener(EndingDirector director, string field, UnityAction listener)
    {
        UnityEvent evt = TestUtils.Get<UnityEvent>(director, field);
        evt.AddListener(listener);
    }

    private static void AddUnityEventListener<T>(EndingDirector director, string field, UnityAction<T> listener)
    {
        UnityEvent<T> evt = TestUtils.Get<UnityEvent<T>>(director, field);
        evt.AddListener(listener);
    }

    private sealed class VariantCounter
    {
        public int Constellations;
        public int Sandbag;
        public int ThickJournal;
        public int Bracelet;
    }
}
