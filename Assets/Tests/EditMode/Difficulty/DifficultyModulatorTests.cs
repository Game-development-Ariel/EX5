using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Covers every guard of the modulator from section 1.3 of the design document: it only runs on Medium,
/// the mode comes from the right score band, it is quiet in the warm-up, during scripted sequences
/// and during long continuous digs.
/// </summary>
public class DifficultyModulatorTests : EditModeTest
{
    private DifficultyModulator modulator;
    private SkillTracker tracker;
    private DifficultyProfile mediumProfile;
    private StageStartedEvent stageStart;
    private List<GameEvent> sequenceStarts;
    private List<GameEvent> sequenceEnds;

    private DifficultyModulator MakeModulator(Difficulty forPreset = Difficulty.Medium, params GameEvent[] scriptedSequencePairs)
    {
        if (forPreset == Difficulty.Easy)
        {
            mediumProfile = Track(DifficultyProfileFactory.CreateEasy());
        }
        else if (forPreset == Difficulty.Hard)
        {
            mediumProfile = Track(DifficultyProfileFactory.CreateHard());
        }
        else
        {
            mediumProfile = Track(DifficultyProfileFactory.CreateMedium());
        }

        stageStart = Track(ScriptableObject.CreateInstance<StageStartedEvent>());

        sequenceStarts = new List<GameEvent>();
        sequenceEnds = new List<GameEvent>();
        for (int i = 0; i + 1 < scriptedSequencePairs.Length; i += 2)
        {
            sequenceStarts.Add(scriptedSequencePairs[i]);
            sequenceEnds.Add(scriptedSequencePairs[i + 1]);
        }

        // Build a tracker that the modulator can read.
        tracker = Make("Tracker").AddComponent<SkillTracker>();
        TestUtils.Set(tracker, "caught", Track(ScriptableObject.CreateInstance<CaughtEvent>()));
        TestUtils.Set(tracker, "playerIdle", Track(ScriptableObject.CreateInstance<PlayerIdleSignal>()));
        TestUtils.Set(tracker, "digSessionEnded", Track(ScriptableObject.CreateInstance<DigSessionEndedEvent>()));
        TestUtils.Set(tracker, "listenActivated", Track(ScriptableObject.CreateInstance<ListenActivatedEvent>()));
        TestUtils.Set(tracker, "hopeChanged", Track(ScriptableObject.CreateInstance<HopeChangedEvent>()));
        TestUtils.Set(tracker, "quietPlaceEntered", Track(ScriptableObject.CreateInstance<QuietPlaceEnteredEvent>()));
        TestUtils.Set(tracker, "quietPlaceExited", Track(ScriptableObject.CreateInstance<QuietPlaceExitedEvent>()));
        TestUtils.Set(tracker, "stageStarted", stageStart);
        TestUtils.Set(tracker, "stageEnded", Track(ScriptableObject.CreateInstance<StageEndedEvent>()));
        TestUtils.Call(tracker, "OnEnable");

        modulator = Make("Modulator").AddComponent<DifficultyModulator>();
        TestUtils.Set(modulator, "skillTracker", tracker);
        TestUtils.Set(modulator, "mediumProfile", mediumProfile);
        TestUtils.Set(modulator, "stageStarted", stageStart);
        TestUtils.Set(modulator, "scriptedSequenceStarts", sequenceStarts);
        TestUtils.Set(modulator, "scriptedSequenceEnds", sequenceEnds);
        TestUtils.Call(modulator, "Awake");
        TestUtils.Call(modulator, "OnEnable");
        return modulator;
    }

    private void SetScore(float value)
    {
        TestUtils.Set(tracker, "score", value);
    }

    private void SimulateStageStarted(float warmupPassedSeconds)
    {
        stageStart.Raise();
        // Backdate when the stage started so the warm-up is either done or still running.
        TestUtils.Set(modulator, "stageStartedAt", Time.time - warmupPassedSeconds);
    }

    [Test]
    public void OnEasy_StaysOnDefaultEvenWithLowScore()
    {
        MakeModulator(Difficulty.Easy);
        SimulateStageStarted(60f);
        SetScore(-9f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Default, modulator.CurrentMode);
        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)));
    }

    [Test]
    public void OnHard_StaysOnDefaultEvenWithHighScore()
    {
        MakeModulator(Difficulty.Hard);
        SimulateStageStarted(60f);
        SetScore(9f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Default, modulator.CurrentMode);
        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)));
    }

    [TestCase(-10f, ModulationMode.Mercy)]
    [TestCase(-7f, ModulationMode.Mercy)]
    [TestCase(-6f, ModulationMode.Ease)]
    [TestCase(-3f, ModulationMode.Ease)]
    [TestCase(-2f, ModulationMode.Default)]
    [TestCase(0f, ModulationMode.Default)]
    [TestCase(2f, ModulationMode.Default)]
    [TestCase(3f, ModulationMode.Tighten)]
    [TestCase(6f, ModulationMode.Tighten)]
    [TestCase(7f, ModulationMode.Challenge)]
    [TestCase(10f, ModulationMode.Challenge)]
    public void ScoreMapsToExpectedMode(float score, ModulationMode expectedMode)
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(score);

        modulator.Tick();

        Assert.AreEqual(expectedMode, modulator.CurrentMode);
    }

    [Test]
    public void FirstThirtySeconds_StayOnDefaultNoMatterTheScore()
    {
        MakeModulator();
        SimulateStageStarted(5f);
        SetScore(-10f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Default, modulator.CurrentMode);
    }

    [Test]
    public void AfterTheWarmup_ScoreTakesEffect()
    {
        MakeModulator();
        SimulateStageStarted(45f); // Past the 30-second warm-up.
        SetScore(-10f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Mercy, modulator.CurrentMode);
    }

    [Test]
    public void ScriptedSequenceActive_FreezesTheMode()
    {
        GameEvent seqStart = Track(TestData.Event("ScriptedStart"));
        GameEvent seqEnd = Track(TestData.Event("ScriptedEnd"));
        MakeModulator(Difficulty.Medium, seqStart, seqEnd);
        SimulateStageStarted(60f);
        SetScore(-10f);

        // Reach Mercy first.
        modulator.Tick();
        Assert.AreEqual(ModulationMode.Mercy, modulator.CurrentMode);

        // Scripted sequence starts; ticks that happen while it is active should be ignored.
        seqStart.Raise();
        SetScore(10f);
        modulator.Tick();

        Assert.AreEqual(ModulationMode.Mercy, modulator.CurrentMode, "Mode changed during a scripted sequence");
        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)),
            "A multiplier was returned during a scripted sequence");
    }

    [Test]
    public void AfterTheScriptedSequenceEnds_ModulationResumes()
    {
        GameEvent seqStart = Track(TestData.Event("ScriptedStart"));
        GameEvent seqEnd = Track(TestData.Event("ScriptedEnd"));
        MakeModulator(Difficulty.Medium, seqStart, seqEnd);
        SimulateStageStarted(60f);
        SetScore(10f);

        seqStart.Raise();
        seqEnd.Raise();
        modulator.Tick();

        Assert.AreEqual(ModulationMode.Challenge, modulator.CurrentMode);
    }

    [Test]
    public void ContinuousDigLongerThanFifteen_FreezesTheMultiplier()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(-10f);

        modulator.SetDigging(true);
        // Advance the modulator's dig counter past the freeze threshold.
        TestUtils.Set(modulator, "continuousDigSeconds", 16f);
        modulator.Tick();

        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)));
    }

    [Test]
    public void ShortDig_LeavesTheMultiplierActive()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(-10f);

        modulator.SetDigging(true);
        TestUtils.Set(modulator, "continuousDigSeconds", 5f);
        modulator.Tick();

        Assert.AreNotEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)));
    }

    [Test]
    public void StopDigging_ResetsTheContinuousCounter()
    {
        MakeModulator();
        SimulateStageStarted(60f);

        modulator.SetDigging(true);
        TestUtils.Set(modulator, "continuousDigSeconds", 20f);
        modulator.SetDigging(false);

        Assert.AreEqual(0f, TestUtils.Get<float>(modulator, "continuousDigSeconds"));
    }

    [Test]
    public void MercyMode_RaisesGuardCheckMultiplier()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(-10f);

        modulator.Tick();

        Assert.Greater(modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)), 1f);
        Assert.Less(modulator.Multiplier(nameof(IDifficultyProfile.CameraConeDegrees)), 1f);
        Assert.Less(modulator.Multiplier(nameof(IDifficultyProfile.HopeLostWhenCaught)), 1f);
    }

    [Test]
    public void EaseMode_RaisesGuardCheckAndBattery()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(-5f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Ease, modulator.CurrentMode);
        Assert.Greater(modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)), 1f);
        Assert.Greater(modulator.Multiplier(nameof(IDifficultyProfile.BatterySeconds)), 1f);
    }

    [Test]
    public void DefaultMode_HasNoMultipliers()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(0f);

        modulator.Tick();

        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)));
        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.BatterySeconds)));
        Assert.AreEqual(1f, modulator.Multiplier(nameof(IDifficultyProfile.CameraConeDegrees)));
    }

    [Test]
    public void TightenMode_LowersGuardCheck()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(5f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Tighten, modulator.CurrentMode);
        Assert.Less(modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)), 1f);
    }

    [Test]
    public void ChallengeMode_LowersGuardCheckAndRaisesCameraCount()
    {
        MakeModulator();
        SimulateStageStarted(60f);
        SetScore(8f);

        modulator.Tick();

        Assert.AreEqual(ModulationMode.Challenge, modulator.CurrentMode);
        Assert.Less(modulator.Multiplier(nameof(IDifficultyProfile.GuardCheckMinSeconds)), 1f);
        Assert.Greater(modulator.Multiplier(nameof(IDifficultyProfile.CameraCount)), 1f);
    }

    [Test]
    public void ModifiersFor_EveryMode_IsReadable()
    {
        MakeModulator();

        foreach (ModulationMode mode in System.Enum.GetValues(typeof(ModulationMode)))
        {
            Assert.IsNotNull(modulator.ModifiersFor(mode), $"No modifier table for {mode}");
        }
    }
}
