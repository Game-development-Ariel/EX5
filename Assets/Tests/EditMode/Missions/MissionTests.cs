using System;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// One test per mission strategy, plus a few tests of the OptionalMissions component that wires
/// the strategies to a recorder. The tests build missions through the factory to stay close to
/// how a scene wires them, and build a RunState in-line because it is a plain data class.
/// </summary>
public class MissionTests : EditModeTest
{
    [Test]
    public void AllMemories_UnmetWhenSomeAreStillMissing()
    {
        OptionalMission mission = Track(OptionalMissionFactory.AllMemoriesMission());
        var state = new RunState { memoriesFound = 2, totalMemories = 4 };

        Assert.IsFalse(mission.CheckCompleted(state));
    }

    [Test]
    public void AllMemories_MetWhenEveryMemoryWasFound()
    {
        OptionalMission mission = Track(OptionalMissionFactory.AllMemoriesMission());
        var state = new RunState { memoriesFound = 4, totalMemories = 4 };

        Assert.IsTrue(mission.CheckCompleted(state));
    }

    [Test]
    public void AllMemories_UnmetWhenNoMemoriesExistInTheRun()
    {
        // Prevents the "0 out of 0" edge from being reported as completed.
        OptionalMission mission = Track(OptionalMissionFactory.AllMemoriesMission());
        var state = new RunState { memoriesFound = 0, totalMemories = 0 };

        Assert.IsFalse(mission.CheckCompleted(state));
    }

    [Test]
    public void AllPreviousNotes_MetWhenEveryPageWasFound()
    {
        OptionalMission mission = Track(OptionalMissionFactory.AllPreviousNotesMission());
        var state = new RunState { previousNotesFound = 7, totalPreviousNotes = 7 };

        Assert.IsTrue(mission.CheckCompleted(state));
    }

    [Test]
    public void AllPreviousNotes_UnmetWhenOnePageIsMissing()
    {
        OptionalMission mission = Track(OptionalMissionFactory.AllPreviousNotesMission());
        var state = new RunState { previousNotesFound = 6, totalPreviousNotes = 7 };

        Assert.IsFalse(mission.CheckCompleted(state));
    }

    [Test]
    public void NoCapturesDay_MetWhenCapturesAreZero()
    {
        OptionalMission mission = Track(OptionalMissionFactory.NoCapturesDayMission());
        var state = new RunState { capturesThisStage = 0 };

        Assert.IsTrue(mission.CheckCompleted(state));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(5)]
    public void NoCapturesDay_UnmetAfterAnyCapture(int captures)
    {
        OptionalMission mission = Track(OptionalMissionFactory.NoCapturesDayMission());
        var state = new RunState { capturesThisStage = captures };

        Assert.IsFalse(mission.CheckCompleted(state));
    }

    [Test]
    public void SandbagWrite_MetOnlyAfterTheSandbagWasWritten()
    {
        OptionalMission mission = Track(OptionalMissionFactory.SandbagWriteMission());

        Assert.IsFalse(mission.CheckCompleted(new RunState { sandbagWritten = false }));
        Assert.IsTrue(mission.CheckCompleted(new RunState { sandbagWritten = true }));
    }

    [Test]
    public void BraceletFound_MetOnlyAfterTheBraceletWasSeen()
    {
        OptionalMission mission = Track(OptionalMissionFactory.BraceletFoundMission());

        Assert.IsFalse(mission.CheckCompleted(new RunState { braceletFound = false }));
        Assert.IsTrue(mission.CheckCompleted(new RunState { braceletFound = true }));
    }

    [Test]
    public void CountWallMark_MetOnlyAfterATallyWasAdded()
    {
        OptionalMission mission = Track(OptionalMissionFactory.CountWallMarkMission());

        Assert.IsFalse(mission.CheckCompleted(new RunState { countWallMarked = false }));
        Assert.IsTrue(mission.CheckCompleted(new RunState { countWallMarked = true }));
    }

    [Test]
    public void CheckCompleted_IsSafeWithANullState()
    {
        OptionalMission mission = Track(OptionalMissionFactory.AllMemoriesMission());

        Assert.IsFalse(mission.CheckCompleted(null));
    }

    [Test]
    public void OptionalMissions_RecordAddsIdToRunState()
    {
        OptionalMissions component = BuildComponent();

        component.Record("prev-sacks-soft-corner");

        Assert.IsTrue(component.State.optionalEntriesRecorded.Contains("prev-sacks-soft-corner"));
    }

    [Test]
    public void OptionalMissions_RecordIsIdempotent()
    {
        OptionalMissions component = BuildComponent();

        component.Record("memory-piano");
        component.Record("memory-piano");

        Assert.AreEqual(1, component.State.optionalEntriesRecorded.Count);
    }

    [Test]
    public void OptionalMissions_IsCompletedReadsTheStrategyForTheKind()
    {
        OptionalMissions component = BuildComponent();
        component.UseMissions(Track(OptionalMissionFactory.SandbagWriteMission()));

        Assert.IsFalse(component.IsCompleted(MissionKind.SandbagWrite));
        component.State.sandbagWritten = true;
        Assert.IsTrue(component.IsCompleted(MissionKind.SandbagWrite));
    }

    [Test]
    public void OptionalMissions_CompletedMissionsReturnsOnlyTheMetOnes()
    {
        OptionalMissions component = BuildComponent();
        OptionalMission sandbag = Track(OptionalMissionFactory.SandbagWriteMission());
        OptionalMission bracelet = Track(OptionalMissionFactory.BraceletFoundMission());
        component.UseMissions(sandbag, bracelet);

        component.State.sandbagWritten = true;
        component.State.braceletFound = false;

        var completed = component.CompletedMissions();
        Assert.AreEqual(1, completed.Count);
        Assert.AreSame(sandbag, completed[0]);
    }

    [Test]
    public void OptionalMissions_SubscribesToTheRecorderAndRecordsItsIds()
    {
        OptionalMissions component = BuildComponent();
        var recorder = new FakeRecorder();
        component.UseRecorder(recorder);

        recorder.Fire("memory-piano");

        Assert.IsTrue(component.State.optionalEntriesRecorded.Contains("memory-piano"));
    }

    [Test]
    public void OptionalMissions_SubscribesToTheGameEventWithoutError()
    {
        GameEvent optionalRecorded = Track(TestData.Event("OptionalRecorded"));
        OptionalMissions component = BuildComponent(optionalRecorded);

        Assert.DoesNotThrow(() => optionalRecorded.Raise());
    }

    [Test]
    public void OptionalMissions_OnDisableStopsListeningToTheRecorder()
    {
        OptionalMissions component = BuildComponent();
        var recorder = new FakeRecorder();
        component.UseRecorder(recorder);
        TestUtils.Call(component, "OnDisable");

        recorder.Fire("memory-piano");

        Assert.IsFalse(component.State.optionalEntriesRecorded.Contains("memory-piano"));
    }

    private OptionalMissions BuildComponent(GameEvent optionalRecorded = null)
    {
        OptionalMissions component = Make("OptionalMissions").AddComponent<OptionalMissions>();
        component.UseMissions(Array.Empty<OptionalMission>());
        if (optionalRecorded != null)
        {
            TestUtils.Set(component, "optionalRecorded", optionalRecorded);
        }
        TestUtils.Call(component, "OnEnable");
        return component;
    }

    /// <summary>Lets the tests fire the recorder event without touching OptionalContent.Instance.</summary>
    private sealed class FakeRecorder : IOptionalRecorder
    {
        public event Action<string> Recorded;
        private readonly System.Collections.Generic.HashSet<string> seen = new System.Collections.Generic.HashSet<string>();

        public void Record(string id)
        {
            if (seen.Add(id))
            {
                Recorded?.Invoke(id);
            }
        }

        public bool WasRecorded(string id)
        {
            return seen.Contains(id);
        }

        public void Fire(string id)
        {
            Record(id);
        }
    }
}
