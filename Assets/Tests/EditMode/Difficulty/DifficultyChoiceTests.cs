using NUnit.Framework;
using UnityEngine;

/// <summary>
/// What the difficulty does to the level when it is chosen: the hope a memory gives, the price of a capture,
/// and the hope the level starts with (which is a start, and not a gain the hero feels).
/// </summary>
public class DifficultyChoiceTests : EditModeTest
{
    private const int TunedMemoryHope = 15;
    private const int HardMemoryHope = 10;
    private const int StartHope = 30;
    private const int EasyStartHope = 70;

    [TearDown]
    public void ClearTheDifficulty()
    {
        DifficultyManager.ResetForTests();
    }

    private Hope MakeHope()
    {
        TunnelLevelSettings settings = Track(ScriptableObject.CreateInstance<TunnelLevelSettings>());
        settings.startHope = StartHope;
        Hope hope = Make("Hero").AddComponent<Hope>();
        TestUtils.Set(hope, "settings", settings);
        TestUtils.Call(hope, "Awake");
        return hope;
    }

    [Test]
    public void WithNoDifficulty_AMemoryGivesItsTunedHope()
    {
        Assert.AreEqual(TunedMemoryHope, DifficultyManager.ScaleMemoryHope(TunedMemoryHope));
    }

    [Test]
    public void OnHard_AMemoryGivesLess()
    {
        DifficultyManager.Override(Track(DifficultyProfileFactory.CreateHard()));

        Assert.AreEqual(HardMemoryHope, DifficultyManager.ScaleMemoryHope(TunedMemoryHope));
    }

    [Test]
    public void OnMedium_AMemoryGivesItsTunedHope()
    {
        DifficultyManager.Override(Track(DifficultyProfileFactory.CreateMedium()));

        Assert.AreEqual(TunedMemoryHope, DifficultyManager.ScaleMemoryHope(TunedMemoryHope));
    }

    [Test]
    public void StartAt_SetsTheHope_AndSaysItIsAStart()
    {
        Hope hope = MakeHope();
        bool wasStarting = false;
        hope.Changed += value => wasStarting = hope.IsStarting;

        hope.StartAt(EasyStartHope);

        Assert.AreEqual(EasyStartHope, hope.Current);
        Assert.IsTrue(wasStarting, "Listeners must see IsStarting while the start value arrives");
        Assert.IsFalse(hope.IsStarting, "After the start, a change is a real change again");
    }

    [Test]
    public void ContinuousHope_FollowsAChangeMadeToTheWholeHope()
    {
        Hope hope = MakeHope();
        ContinuousHope continuous = hope.gameObject.AddComponent<ContinuousHope>();
        TestUtils.Call(continuous, "Awake");
        TestUtils.Call(continuous, "OnEnable");

        hope.Add(TunedMemoryHope);

        Assert.AreEqual(StartHope + TunedMemoryHope, continuous.Value);
        TestUtils.Call(continuous, "OnDisable");
    }
}
