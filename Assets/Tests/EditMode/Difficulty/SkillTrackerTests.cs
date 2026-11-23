using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Covers every signal and threshold of the skill score, as listed in section 1.3 of the design document.
/// Each test triggers the right event once and compares the resulting CurrentScore to the designed delta.
/// </summary>
public class SkillTrackerTests : EditModeTest
{
    private SkillTracker tracker;
    private CaughtEvent caught;
    private PlayerIdleSignal idle;
    private DigSessionEndedEvent digEnded;
    private ListenActivatedEvent listen;
    private HopeChangedEvent hope;
    private QuietPlaceEnteredEvent quietEntered;
    private QuietPlaceExitedEvent quietExited;
    private StageStartedEvent stageStart;
    private StageEndedEvent stageEnd;

    private SkillTracker MakeTracker()
    {
        caught = Track(ScriptableObject.CreateInstance<CaughtEvent>());
        idle = Track(ScriptableObject.CreateInstance<PlayerIdleSignal>());
        digEnded = Track(ScriptableObject.CreateInstance<DigSessionEndedEvent>());
        listen = Track(ScriptableObject.CreateInstance<ListenActivatedEvent>());
        hope = Track(ScriptableObject.CreateInstance<HopeChangedEvent>());
        quietEntered = Track(ScriptableObject.CreateInstance<QuietPlaceEnteredEvent>());
        quietExited = Track(ScriptableObject.CreateInstance<QuietPlaceExitedEvent>());
        stageStart = Track(ScriptableObject.CreateInstance<StageStartedEvent>());
        stageEnd = Track(ScriptableObject.CreateInstance<StageEndedEvent>());

        tracker = Make("Tracker").AddComponent<SkillTracker>();
        TestUtils.Set(tracker, "caught", caught);
        TestUtils.Set(tracker, "playerIdle", idle);
        TestUtils.Set(tracker, "digSessionEnded", digEnded);
        TestUtils.Set(tracker, "listenActivated", listen);
        TestUtils.Set(tracker, "hopeChanged", hope);
        TestUtils.Set(tracker, "quietPlaceEntered", quietEntered);
        TestUtils.Set(tracker, "quietPlaceExited", quietExited);
        TestUtils.Set(tracker, "stageStarted", stageStart);
        TestUtils.Set(tracker, "stageEnded", stageEnd);
        TestUtils.Call(tracker, "OnEnable");
        return tracker;
    }

    private void StartStage()
    {
        stageStart.Raise();
    }

    [Test]
    public void StartingScore_IsZero()
    {
        MakeTracker();
        StartStage();

        Assert.AreEqual(0f, tracker.CurrentScore);
    }

    [Test]
    public void Caught_LosesTwoPoints()
    {
        MakeTracker();
        StartStage();

        caught.Raise();

        Assert.AreEqual(-2f, tracker.CurrentScore);
    }

    [Test]
    public void IdleFor30Seconds_LosesOnePoint()
    {
        MakeTracker();
        StartStage();

        idle.Raise(30f);

        Assert.AreEqual(-1f, tracker.CurrentScore);
    }

    [Test]
    public void IdleBelow30Seconds_DoesNothing()
    {
        MakeTracker();
        StartStage();

        idle.Raise(29f);

        Assert.AreEqual(0f, tracker.CurrentScore);
    }

    [Test]
    public void IdlePenalty_FiresOncePerIdleStreak()
    {
        MakeTracker();
        StartStage();

        idle.Raise(30f);
        idle.Raise(31f);
        idle.Raise(45f);

        Assert.AreEqual(-1f, tracker.CurrentScore);
    }

    [Test]
    public void IdlePenalty_FiresAgainAfterTheIdleResets()
    {
        MakeTracker();
        StartStage();

        idle.Raise(30f);
        idle.Raise(0f); // The player moved; the next long idle penalty is allowed again.
        idle.Raise(30f);

        Assert.AreEqual(-2f, tracker.CurrentScore);
    }

    [Test]
    public void DigEnded_LongSessionNoCatch_AddsOnePoint()
    {
        MakeTracker();
        StartStage();

        digEnded.Raise(2.5f, false);

        Assert.AreEqual(1f, tracker.CurrentScore);
    }

    [Test]
    public void DigEnded_ShortSessionNoCatch_SubtractsHalfPoint()
    {
        MakeTracker();
        StartStage();

        digEnded.Raise(0.49f, false);

        Assert.AreEqual(-0.5f, tracker.CurrentScore);
    }

    [Test]
    public void DigEnded_MiddleSessionNoCatch_DoesNothing()
    {
        MakeTracker();
        StartStage();

        digEnded.Raise(1.5f, false);

        Assert.AreEqual(0f, tracker.CurrentScore);
    }

    [Test]
    public void DigEnded_EndedByCatch_NeverRewardsOrPenalizes()
    {
        MakeTracker();
        StartStage();

        digEnded.Raise(3f, true);
        digEnded.Raise(0.1f, true);

        Assert.AreEqual(0f, tracker.CurrentScore);
    }

    [Test]
    public void Listen_AwardsOnePointPerActivation()
    {
        MakeTracker();
        StartStage();

        listen.Raise();
        listen.Raise();

        Assert.AreEqual(2f, tracker.CurrentScore);
    }

    [Test]
    public void Listen_CapsAtThreePointsPerStage()
    {
        MakeTracker();
        StartStage();

        for (int i = 0; i < 5; i++)
        {
            listen.Raise();
        }

        Assert.AreEqual(3f, tracker.CurrentScore);
    }

    [Test]
    public void StageEnd_ZeroListens_SubtractsTwoPoints()
    {
        MakeTracker();
        StartStage();

        stageEnd.Raise();

        Assert.AreEqual(-2f, tracker.CurrentScore);
    }

    [Test]
    public void StageEnd_AnyListenAvoidsTheNoListenPenalty()
    {
        MakeTracker();
        StartStage();

        listen.Raise();
        stageEnd.Raise();

        Assert.AreEqual(1f, tracker.CurrentScore);
    }

    [Test]
    public void StageEnd_HopeKeptAbove60NoMemory_AddsOnePoint()
    {
        MakeTracker();
        StartStage();

        hope.Raise(50, 61);
        hope.Raise(61, 70);
        listen.Raise(); // So the no-listen penalty doesn't fire and confuse the score.
        stageEnd.Raise();

        Assert.AreEqual(2f, tracker.CurrentScore); // +1 listen, +1 hope kept
    }

    [Test]
    public void StageEnd_HopeKeptAbove60_ButMemoryUsed_NoBonus()
    {
        MakeTracker();
        StartStage();

        hope.Raise(50, 65);
        tracker.NoteMemoryUsed();
        listen.Raise();
        stageEnd.Raise();

        Assert.AreEqual(1f, tracker.CurrentScore); // only +1 listen, no hope-kept bonus
    }

    [Test]
    public void StageEnd_HopeDroppedBelow60_NoBonus()
    {
        MakeTracker();
        StartStage();

        hope.Raise(70, 55);
        listen.Raise();
        stageEnd.Raise();

        Assert.AreEqual(1f, tracker.CurrentScore); // only listen, no bonus
    }

    [Test]
    public void StageEnd_HopeBelow25_SubtractsOnePoint()
    {
        MakeTracker();
        StartStage();

        hope.Raise(30, 20);
        listen.Raise();
        stageEnd.Raise();

        Assert.AreEqual(0f, tracker.CurrentScore); // +1 listen, -1 low hope
    }

    [Test]
    public void StageEnd_HopeAt25_NoLowHopePenalty()
    {
        MakeTracker();
        StartStage();

        hope.Raise(30, 25);
        listen.Raise();
        stageEnd.Raise();

        Assert.AreEqual(1f, tracker.CurrentScore); // only listen
    }

    [Test]
    public void QuietPlace_AfterTenSecondsNoPrayer_SubtractsHalfPoint()
    {
        MakeTracker();
        StartStage();

        // Enter the quiet place and backdate the enter time so Update sees it as long ago.
        quietEntered.Raise();
        TestUtils.Set(tracker, "quietPlaceEnteredAt", Time.time - 11f);

        TestUtils.Call(tracker, "Update");

        Assert.AreEqual(-0.5f, tracker.CurrentScore);
    }

    [Test]
    public void QuietPlace_PrayerStarted_NoPenalty()
    {
        MakeTracker();
        StartStage();

        quietEntered.Raise();
        tracker.NotePrayerStarted();
        TestUtils.Set(tracker, "quietPlaceEnteredAt", Time.time - 11f);

        TestUtils.Call(tracker, "Update");

        Assert.AreEqual(0f, tracker.CurrentScore);
    }

    [Test]
    public void QuietPlace_PenaltyFiresOncePerVisit()
    {
        MakeTracker();
        StartStage();

        quietEntered.Raise();
        TestUtils.Set(tracker, "quietPlaceEnteredAt", Time.time - 11f);
        TestUtils.Call(tracker, "Update");
        TestUtils.Call(tracker, "Update");
        TestUtils.Call(tracker, "Update");

        Assert.AreEqual(-0.5f, tracker.CurrentScore);
    }

    [Test]
    public void StageStart_ResetsTheScore()
    {
        MakeTracker();
        StartStage();

        caught.Raise();
        caught.Raise();
        Assert.AreEqual(-4f, tracker.CurrentScore);

        StartStage();

        Assert.AreEqual(0f, tracker.CurrentScore);
    }

    [Test]
    public void Score_ClampedToMinusTen()
    {
        MakeTracker();
        StartStage();

        for (int i = 0; i < 20; i++)
        {
            caught.Raise();
        }

        Assert.AreEqual(-10f, tracker.CurrentScore);
    }

    [Test]
    public void Score_ClampedToPlusTen()
    {
        MakeTracker();
        StartStage();

        for (int i = 0; i < 20; i++)
        {
            listen.Raise();
            digEnded.Raise(3f, false);
        }

        Assert.AreEqual(10f, tracker.CurrentScore);
    }

    [Test]
    public void ScoreChanged_FiresOnEveryChange()
    {
        MakeTracker();
        StartStage();
        int calls = 0;
        tracker.ScoreChanged += value => calls++;

        caught.Raise();
        caught.Raise();

        Assert.AreEqual(2, calls);
    }

    [Test]
    public void ScoreChanged_DoesNotFireWhenValueClampedUnchanged()
    {
        MakeTracker();
        StartStage();
        for (int i = 0; i < 5; i++)
        {
            caught.Raise();
        }
        int callsAfterClamp = 0;
        tracker.ScoreChanged += value => callsAfterClamp++;

        caught.Raise(); // Already at -10, no change.

        Assert.AreEqual(0, callsAfterClamp);
    }
}
