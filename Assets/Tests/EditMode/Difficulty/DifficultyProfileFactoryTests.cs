using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Pins the exact numbers of section 1.2 of the design document. A factory change that moves away from the spec
/// will light up here before it reaches a build.
/// </summary>
public class DifficultyProfileFactoryTests : EditModeTest
{
    private DifficultyProfile MakeEasy() => Track(DifficultyProfileFactory.CreateEasy());
    private DifficultyProfile MakeMedium() => Track(DifficultyProfileFactory.CreateMedium());
    private DifficultyProfile MakeHard() => Track(DifficultyProfileFactory.CreateHard());

    // Easy -------------------------------------------------------------------

    [Test]
    public void Easy_IsTheEasyPreset()
    {
        Assert.AreEqual(Difficulty.Easy, MakeEasy().Difficulty);
    }

    [Test]
    public void Easy_GuardCheckSeventyToNinety()
    {
        DifficultyProfile profile = MakeEasy();
        Assert.AreEqual(70f, profile.GuardCheckMinSeconds);
        Assert.AreEqual(90f, profile.GuardCheckMaxSeconds);
    }

    [Test]
    public void Easy_PatrolCycleIsTwenty()
    {
        Assert.AreEqual(20f, MakeEasy().GuardPatrolCycleSeconds);
    }

    [Test]
    public void Easy_Stage2HasOneGuard()
    {
        Assert.AreEqual(1, MakeEasy().Stage2GuardCount);
    }

    [Test]
    public void Easy_CameraCone45()
    {
        Assert.AreEqual(45f, MakeEasy().CameraConeDegrees);
    }

    [Test]
    public void Easy_CameraCountThree()
    {
        Assert.AreEqual(3, MakeEasy().CameraCount);
    }

    [Test]
    public void Easy_NoRotatingCamera()
    {
        Assert.IsFalse(MakeEasy().RotatingCameraEnabled);
    }

    [Test]
    public void Easy_StartingHopeSeventy()
    {
        Assert.AreEqual(70, MakeEasy().StartingHope);
    }

    [Test]
    public void Easy_HopeLostWhenCaughtFive()
    {
        Assert.AreEqual(5, MakeEasy().HopeLostWhenCaught);
    }

    [Test]
    public void Easy_HopeDoesNotDropWithTime()
    {
        Assert.AreEqual(0f, MakeEasy().HopeNaturalDecayPerMinute);
    }

    [Test]
    public void Easy_DigAtZeroPointTwoMetersPerSecond()
    {
        Assert.AreEqual(0.20f, MakeEasy().DigMetersPerSecond, 0.001f);
    }

    [Test]
    public void Easy_DigNoiseInOneCorridor()
    {
        Assert.AreEqual(1, MakeEasy().DigNoiseCorridors);
    }

    [Test]
    public void Easy_NoContinuousDigThreshold()
    {
        Assert.IsFalse(MakeEasy().HasContinuousDigThreshold);
    }

    [Test]
    public void Easy_BatteryNinetySeconds()
    {
        Assert.AreEqual(90f, MakeEasy().BatterySeconds);
    }

    [Test]
    public void Easy_JournalHintsAfterTwoCaptures()
    {
        DifficultyProfile profile = MakeEasy();
        Assert.IsTrue(profile.JournalHintsEnabled);
        Assert.AreEqual(2, profile.JournalHintCapturesRequired);
    }

    [Test]
    public void Easy_NoRandomListenPause()
    {
        Assert.IsFalse(MakeEasy().RandomListenPauseEnabled);
    }

    // Medium -----------------------------------------------------------------

    [Test]
    public void Medium_IsTheMediumPreset()
    {
        Assert.AreEqual(Difficulty.Medium, MakeMedium().Difficulty);
    }

    [Test]
    public void Medium_GuardCheck45To60()
    {
        DifficultyProfile profile = MakeMedium();
        Assert.AreEqual(45f, profile.GuardCheckMinSeconds);
        Assert.AreEqual(60f, profile.GuardCheckMaxSeconds);
    }

    [Test]
    public void Medium_PatrolCycleFourteen()
    {
        Assert.AreEqual(14f, MakeMedium().GuardPatrolCycleSeconds);
    }

    [Test]
    public void Medium_Stage2TwoGuards()
    {
        Assert.AreEqual(2, MakeMedium().Stage2GuardCount);
    }

    [Test]
    public void Medium_CameraConeSixty()
    {
        Assert.AreEqual(60f, MakeMedium().CameraConeDegrees);
    }

    [Test]
    public void Medium_CameraCountFive()
    {
        Assert.AreEqual(5, MakeMedium().CameraCount);
    }

    [Test]
    public void Medium_NoRotatingCamera()
    {
        Assert.IsFalse(MakeMedium().RotatingCameraEnabled);
    }

    [Test]
    public void Medium_StartingHopeFifty()
    {
        Assert.AreEqual(50, MakeMedium().StartingHope);
    }

    [Test]
    public void Medium_HopeLostWhenCaughtTen()
    {
        Assert.AreEqual(10, MakeMedium().HopeLostWhenCaught);
    }

    [Test]
    public void Medium_HopeDoesNotDropWithTime()
    {
        Assert.AreEqual(0f, MakeMedium().HopeNaturalDecayPerMinute);
    }

    [Test]
    public void Medium_MemoryGivesFifteen()
    {
        Assert.AreEqual(15, MakeMedium().MemoryHopeValue);
    }

    [Test]
    public void Medium_DigAtZeroPointOneFiveMetersPerSecond()
    {
        Assert.AreEqual(0.15f, MakeMedium().DigMetersPerSecond, 0.001f);
    }

    [Test]
    public void Medium_DigNoiseInTwoCorridors()
    {
        Assert.AreEqual(2, MakeMedium().DigNoiseCorridors);
    }

    [Test]
    public void Medium_NoContinuousDigThreshold()
    {
        Assert.IsFalse(MakeMedium().HasContinuousDigThreshold);
    }

    [Test]
    public void Medium_BatterySixtyFiveSeconds()
    {
        Assert.AreEqual(65f, MakeMedium().BatterySeconds);
    }

    [Test]
    public void Medium_NoJournalHints()
    {
        Assert.IsFalse(MakeMedium().JournalHintsEnabled);
    }

    [Test]
    public void Medium_NoRandomListenPause()
    {
        Assert.IsFalse(MakeMedium().RandomListenPauseEnabled);
    }

    // Hard -------------------------------------------------------------------

    [Test]
    public void Hard_HopeDoesNotDropWithTime()
    {
        Assert.AreEqual(0f, MakeHard().HopeNaturalDecayPerMinute);
    }

    [Test]
    public void Hard_IsTheHardPreset()
    {
        Assert.AreEqual(Difficulty.Hard, MakeHard().Difficulty);
    }

    [Test]
    public void Hard_GuardCheck30To45()
    {
        DifficultyProfile profile = MakeHard();
        Assert.AreEqual(30f, profile.GuardCheckMinSeconds);
        Assert.AreEqual(45f, profile.GuardCheckMaxSeconds);
    }

    [Test]
    public void Hard_PatrolCyclesFourteenAndSeventeen()
    {
        DifficultyProfile profile = MakeHard();
        Assert.AreEqual(14f, profile.GuardPatrolCycleSeconds);
        Assert.AreEqual(17f, profile.GuardSecondPatrolCycleSeconds);
    }

    [Test]
    public void Hard_Stage2TwoGuards()
    {
        Assert.AreEqual(2, MakeHard().Stage2GuardCount);
    }

    [Test]
    public void Hard_ListenPauseEnabled()
    {
        DifficultyProfile profile = MakeHard();
        Assert.IsTrue(profile.RandomListenPauseEnabled);
        Assert.AreEqual(0.5f, profile.RandomListenPauseDurationSeconds);
        Assert.AreEqual(20f, profile.RandomListenPauseIntervalMinSeconds);
        Assert.AreEqual(30f, profile.RandomListenPauseIntervalMaxSeconds);
    }

    [Test]
    public void Hard_CameraConeSeventyFive()
    {
        Assert.AreEqual(75f, MakeHard().CameraConeDegrees);
    }

    [Test]
    public void Hard_CameraCountSeven()
    {
        Assert.AreEqual(7, MakeHard().CameraCount);
    }

    [Test]
    public void Hard_RotatingCameraEveryEightSeconds()
    {
        DifficultyProfile profile = MakeHard();
        Assert.IsTrue(profile.RotatingCameraEnabled);
        Assert.AreEqual(8f, profile.RotatingCameraPeriodSeconds);
    }

    [Test]
    public void Hard_StartingHopeThirty()
    {
        Assert.AreEqual(30, MakeHard().StartingHope);
    }

    [Test]
    public void Hard_HopeLostWhenCaughtTwenty()
    {
        Assert.AreEqual(20, MakeHard().HopeLostWhenCaught);
    }

    [Test]
    public void Hard_MemoryGivesTen()
    {
        Assert.AreEqual(10, MakeHard().MemoryHopeValue);
    }

    [Test]
    public void Hard_DigAtZeroPointOneOneMetersPerSecond()
    {
        Assert.AreEqual(0.11f, MakeHard().DigMetersPerSecond, 0.001f);
    }

    [Test]
    public void Hard_DigNoiseInTwoCorridors()
    {
        Assert.AreEqual(2, MakeHard().DigNoiseCorridors);
    }

    [Test]
    public void Hard_ContinuousDigThresholdAtTwenty()
    {
        DifficultyProfile profile = MakeHard();
        Assert.IsTrue(profile.HasContinuousDigThreshold);
        Assert.AreEqual(20f, profile.ContinuousDigThresholdSeconds);
    }

    [Test]
    public void Hard_BatteryFortyFiveSeconds()
    {
        Assert.AreEqual(45f, MakeHard().BatterySeconds);
    }

    [Test]
    public void Hard_NoJournalHints()
    {
        Assert.IsFalse(MakeHard().JournalHintsEnabled);
    }

    // Create(enum) -----------------------------------------------------------

    [Test]
    public void Create_Easy_ReturnsEasyProfile()
    {
        DifficultyProfile profile = Track(DifficultyProfileFactory.Create(Difficulty.Easy));
        Assert.AreEqual(Difficulty.Easy, profile.Difficulty);
    }

    [Test]
    public void Create_Medium_ReturnsMediumProfile()
    {
        DifficultyProfile profile = Track(DifficultyProfileFactory.Create(Difficulty.Medium));
        Assert.AreEqual(Difficulty.Medium, profile.Difficulty);
    }

    [Test]
    public void Create_Hard_ReturnsHardProfile()
    {
        DifficultyProfile profile = Track(DifficultyProfileFactory.Create(Difficulty.Hard));
        Assert.AreEqual(Difficulty.Hard, profile.Difficulty);
    }
}
