using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// The director keeps the player's eye on one goal at a time, advances through beats
/// on the advanceBeat event, and never lights more than the policy budget.
/// </summary>
public class AttentionDirectorTests : EditModeTest
{
    private const int LargeBeatToLightCount = 10;

    private AttentionDirector director;
    private GameEvent advance;
    private List<HighlightTarget> beatZeroLights;
    private List<HighlightTarget> beatOneLights;

    [SetUp]
    public void SetUp()
    {
        advance = Track(TestData.Event("advance"));

        beatZeroLights = new List<HighlightTarget> { MakeTarget("Bottle") };
        beatOneLights = new List<HighlightTarget> { MakeTarget("Mattress") };

        var beats = new List<AttentionBeat>
        {
            new AttentionBeat("Bottle lit", beatZeroLights, new List<HighlightTarget>()),
            new AttentionBeat("Mattress lit", beatOneLights, beatZeroLights),
        };

        director = Make("Director").AddComponent<AttentionDirector>();
        TestUtils.Set(director, "advanceBeat", advance);
        TestUtils.Set(director, "beats", beats);
        TestUtils.Call(director, "OnEnable");
        TestUtils.Call(director, "Start");
    }

    private HighlightTarget MakeTarget(string name)
    {
        HighlightTarget target = Make(name).AddComponent<HighlightTarget>();
        TestUtils.Call(target, "Awake");
        return target;
    }

    [Test]
    public void AtStart_TheDirectorIsAtBeatZero()
    {
        Assert.AreEqual(0, director.CurrentBeatIndex);
    }

    [Test]
    public void AtStart_TheFirstBeatsLightsAreOn_AndTheOthersAreOff()
    {
        Assert.IsTrue(beatZeroLights[0].IsHighlighted, "Bottle should be bright at beat 0");
        Assert.IsFalse(beatOneLights[0].IsHighlighted, "Mattress should be dark at beat 0");
    }

    [Test]
    public void AdvanceBeat_MovesToBeatOne()
    {
        advance.Raise();

        Assert.AreEqual(1, director.CurrentBeatIndex);
    }

    [Test]
    public void AdvanceBeat_LightsTheNewBeatsToLightTargets()
    {
        advance.Raise();

        Assert.IsTrue(beatOneLights[0].IsHighlighted);
    }

    [Test]
    public void AdvanceBeat_DimsTheOldBeatsLights()
    {
        advance.Raise();

        Assert.IsFalse(beatZeroLights[0].IsHighlighted, "The old bottle light must go dark at beat 1");
    }

    [Test]
    public void AdvanceBeat_RaisesBeatChangedWithTheNewIndex()
    {
        int seen = -1;
        director.BeatChanged += index => seen = index;

        advance.Raise();

        Assert.AreEqual(1, seen);
    }

    [Test]
    public void AdvanceBeat_PastTheLastBeat_StaysAtTheLastBeat()
    {
        advance.Raise();
        advance.Raise();
        advance.Raise();

        Assert.AreEqual(1, director.CurrentBeatIndex);
    }

    [Test]
    public void SetBeat_JumpsDirectlyToThatBeat()
    {
        director.SetBeat(1);

        Assert.AreEqual(1, director.CurrentBeatIndex);
        Assert.IsTrue(beatOneLights[0].IsHighlighted);
    }

    [Test]
    public void SetBeat_OutOfRange_IsIgnored()
    {
        director.SetBeat(-1);
        director.SetBeat(999);

        Assert.AreEqual(0, director.CurrentBeatIndex);
    }

    [Test]
    public void ABeatWithTooManyLights_NeverShinesMoreThanTheBudget()
    {
        var many = new List<HighlightTarget>();
        for (int i = 0; i < LargeBeatToLightCount; i++)
        {
            many.Add(MakeTarget("Extra " + i));
        }
        var greedyBeat = new AttentionBeat("Too many", many, new List<HighlightTarget>());
        var beats = new List<AttentionBeat> { greedyBeat };

        AttentionDirector greedy = Make("Greedy").AddComponent<AttentionDirector>();
        TestUtils.Set(greedy, "advanceBeat", advance);
        TestUtils.Set(greedy, "beats", beats);
        TestUtils.Call(greedy, "OnEnable");
        TestUtils.Call(greedy, "Start");

        int lit = 0;
        foreach (HighlightTarget target in many)
        {
            if (target.IsHighlighted)
            {
                lit++;
            }
        }
        Assert.LessOrEqual(lit, AttentionPolicy.MaxBrightTargets);
        Assert.AreEqual(AttentionPolicy.MaxBrightTargets, lit, "The policy should fill the budget when enough targets are available");
    }

    [Test]
    public void AttentionPolicy_ThrottleHighlights_ReturnsAtMostTheBudget()
    {
        var targets = new List<HighlightTarget>();
        for (int i = 0; i < LargeBeatToLightCount; i++)
        {
            targets.Add(MakeTarget("Target " + i));
        }

        List<HighlightTarget> kept = AttentionPolicy.ThrottleHighlights(AttentionPolicy.MaxBrightTargets, targets);

        Assert.AreEqual(AttentionPolicy.MaxBrightTargets, kept.Count);
    }

    [Test]
    public void AttentionPolicy_ThrottleHighlights_SkipsNullTargets()
    {
        var targets = new List<HighlightTarget> { null, MakeTarget("One"), null, MakeTarget("Two") };

        List<HighlightTarget> kept = AttentionPolicy.ThrottleHighlights(AttentionPolicy.MaxBrightTargets, targets);

        Assert.AreEqual(2, kept.Count);
    }
}
