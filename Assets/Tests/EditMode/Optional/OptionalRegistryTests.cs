using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class OptionalRegistryTests : EditModeTest
{
    private OptionalRegistry registry;

    [SetUp]
    public void SetUp()
    {
        registry = Track(ScriptableObject.CreateInstance<OptionalRegistry>());
    }

    private void FillStarters()
    {
        TestUtils.Call(registry, "FillStarters");
    }

    [Test]
    public void Starters_HaveAllNineteenIds()
    {
        FillStarters();

        string[] expected =
        {
            "prev-sacks-soft-corner", "prev-dug-hands-short", "prev-watches-corners",
            "wall-count-marks", "wall-no-sleep",
            "guard-room-watch", "guard-room-cups", "guard-room-flower",
            "bracelet-festival",
            "guards-talk-home", "radio-families-wait",
            "sandbag-hostage-write", "count-wall-mark", "journal-i-want-to-remember", "flashlight-empty-room",
            "listen-at-distance", "prayer-spot-sit", "door-crack-look", "journal-reread-shabbat",
        };
        string[] got = registry.Entries.Select(e => e.Id).ToArray();

        CollectionAssert.AreEquivalent(expected, got);
    }

    [Test]
    public void Starters_HaveUniqueIds()
    {
        FillStarters();

        string[] ids = registry.Entries.Select(e => e.Id).ToArray();

        CollectionAssert.AllItemsAreUnique(ids);
    }

    [Test]
    public void Starters_CoverAllThreeLayers()
    {
        FillStarters();

        HashSet<OptionalKind> kinds = new HashSet<OptionalKind>(registry.Entries.Select(e => e.Kind));

        Assert.Contains(OptionalKind.Discovery, kinds.ToArray());
        Assert.Contains(OptionalKind.Expression, kinds.ToArray());
        Assert.Contains(OptionalKind.Observation, kinds.ToArray());
    }

    [Test]
    public void Find_ReturnsTheEntryWithThatId()
    {
        FillStarters();

        OptionalEntry entry = registry.Find("sandbag-hostage-write");

        Assert.IsNotNull(entry);
        Assert.AreEqual(OptionalKind.Expression, entry.Kind);
        Assert.AreEqual("4", entry.StageId);
    }

    [Test]
    public void Find_UnknownId_ReturnsNull()
    {
        FillStarters();

        Assert.IsNull(registry.Find("not-a-real-id"));
    }

    [Test]
    public void Find_NullOrEmpty_ReturnsNull()
    {
        FillStarters();

        Assert.IsNull(registry.Find(null));
        Assert.IsNull(registry.Find(""));
    }

    [Test]
    public void Entries_StartEmpty_WhenRegistryIsFresh()
    {
        Assert.AreEqual(0, registry.Entries.Count);
    }
}
