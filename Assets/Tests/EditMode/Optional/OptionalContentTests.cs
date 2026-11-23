using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class OptionalContentTests : EditModeTest
{
    private OptionalContent content;
    private OptionalRegistry registry;

    [SetUp]
    public void SetUp()
    {
        registry = Track(ScriptableObject.CreateInstance<OptionalRegistry>());
        TestUtils.Set(registry, "entries", new List<OptionalEntry>
        {
            new OptionalEntry("prev-note", "A note", OptionalKind.Discovery, "2"),
            new OptionalEntry("wall-mark", "A wall mark", OptionalKind.Expression, "2"),
            new OptionalEntry("crack-look", "Door crack", OptionalKind.Observation, "1"),
        });

        content = Make("Optional Content").AddComponent<OptionalContent>();
        TestUtils.Set(content, "registry", registry);
        TestUtils.Call(content, "Awake");
    }

    [Test]
    public void StartsEmpty()
    {
        Assert.IsFalse(content.WasRecorded("prev-note"));
        Assert.AreEqual(0, content.AllRecorded().Count);
    }

    [Test]
    public void Record_RemembersTheId()
    {
        content.Record("prev-note");

        Assert.IsTrue(content.WasRecorded("prev-note"));
    }

    [Test]
    public void Record_LeavesOtherIdsAlone()
    {
        content.Record("prev-note");

        Assert.IsFalse(content.WasRecorded("wall-mark"));
    }

    [Test]
    public void Record_RaisesTheEventTheFirstTime()
    {
        int calls = 0;
        string seen = null;
        content.Recorded += id =>
        {
            calls++;
            seen = id;
        };

        content.Record("prev-note");

        Assert.AreEqual(1, calls);
        Assert.AreEqual("prev-note", seen);
    }

    [Test]
    public void Record_IsSilentTheSecondTime()
    {
        int calls = 0;
        content.Recorded += _ => calls++;

        content.Record("prev-note");
        content.Record("prev-note");
        content.Record("prev-note");

        Assert.AreEqual(1, calls);
    }

    [Test]
    public void Record_DeduplicatesInAllRecorded()
    {
        content.Record("prev-note");
        content.Record("prev-note");

        Assert.AreEqual(1, content.AllRecorded().Count);
    }

    [Test]
    public void Record_NullOrEmpty_IsSilentAndNotStored()
    {
        int calls = 0;
        content.Recorded += _ => calls++;

        content.Record(null);
        content.Record("");

        Assert.AreEqual(0, calls);
        Assert.AreEqual(0, content.AllRecorded().Count);
    }

    [Test]
    public void RecordedThisStage_ReturnsOnlyIdsFromThatStage()
    {
        content.Record("prev-note");
        content.Record("wall-mark");
        content.Record("crack-look");

        IReadOnlyList<string> stage2 = content.RecordedThisStage("2");

        CollectionAssert.AreEquivalent(new[] { "prev-note", "wall-mark" }, stage2);
    }

    [Test]
    public void RecordedThisStage_IgnoresIdsNotYetRecorded()
    {
        content.Record("prev-note");

        IReadOnlyList<string> stage2 = content.RecordedThisStage("2");

        CollectionAssert.AreEquivalent(new[] { "prev-note" }, stage2);
    }

    [Test]
    public void Instance_IsSetFromAwake_AndClearedOnDestroy()
    {
        Assert.AreSame(content, OptionalContent.Instance);

        Object.DestroyImmediate(content);

        Assert.IsNull(OptionalContent.Instance);
    }

    [Test]
    public void IOptionalRecorder_IsImplemented()
    {
        IOptionalRecorder recorder = content;
        recorder.Record("prev-note");

        Assert.IsTrue(recorder.WasRecorded("prev-note"));
    }
}
