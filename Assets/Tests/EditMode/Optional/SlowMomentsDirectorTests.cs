using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SlowMomentsDirectorTests : EditModeTest
{
    private const float CalmSeconds = 3f;

    private SlowMomentsDirector director;
    private GameEvent digActive;
    private GameEvent digEnded;
    private GameEvent guardClose;
    private GameEvent guardClear;
    private GameEvent scripted;
    private GameEvent scriptedEnded;
    private PlayerMovement movement;
    private List<bool> changes;

    [SetUp]
    public void SetUp()
    {
        digActive = Track(TestData.Event("DigActive"));
        digEnded = Track(TestData.Event("DigEnded"));
        guardClose = Track(TestData.Event("GuardClose"));
        guardClear = Track(TestData.Event("GuardClear"));
        scripted = Track(TestData.Event("ScriptedBreakingPoint"));
        scriptedEnded = Track(TestData.Event("ScriptedBreakingPointEnded"));

        GameObject player = Make("Player");
        player.AddComponent<CharacterController>();
        movement = player.AddComponent<PlayerMovement>();
        movement.enabled = false; // IsWalking short-circuits to false when the movement is disabled.

        GameObject host = Make("Slow Moments Director");
        director = host.AddComponent<SlowMomentsDirector>();
        TestUtils.Set(director, "digActiveEvent", digActive);
        TestUtils.Set(director, "digEndedEvent", digEnded);
        TestUtils.Set(director, "guardCloseEvent", guardClose);
        TestUtils.Set(director, "guardClearEvent", guardClear);
        TestUtils.Set(director, "scriptedBreakingPointEvent", scripted);
        TestUtils.Set(director, "scriptedBreakingPointEndedEvent", scriptedEnded);
        TestUtils.Set(director, "playerMovement", movement);
        TestUtils.Set(director, "calmSecondsRequired", CalmSeconds);

        changes = new List<bool>();
        director.CalmChanged += value => changes.Add(value);

        TestUtils.Call(director, "OnEnable");
    }

    /// <summary>
    /// Simulates still time passing. Time.deltaTime is not controllable in EditMode, so the test
    /// sets stillSeconds to the target value and then calls Update, which either keeps it (and sets
    /// calm) or zeros it (because hot or walking). This models "seconds passed while we tried to be calm".
    /// </summary>
    private void Tick(float seconds)
    {
        TestUtils.Set(director, "stillSeconds", seconds);
        TestUtils.Call(director, "Update");
    }

    [Test]
    public void StartsNotCalm()
    {
        Assert.IsFalse(director.CalmEnough);
    }

    [Test]
    public void BecomesCalm_AfterTheRequiredStill()
    {
        Tick(CalmSeconds);

        Assert.IsTrue(director.CalmEnough);
        Assert.AreEqual(new[] { true }, changes);
    }

    [Test]
    public void DoesNotBecomeCalm_BeforeTheRequiredStill()
    {
        Tick(CalmSeconds - 0.5f);

        Assert.IsFalse(director.CalmEnough);
    }

    [Test]
    public void DigActive_PreventsCalm()
    {
        digActive.Raise();
        Tick(CalmSeconds + 1f);

        Assert.IsFalse(director.CalmEnough);
    }

    [Test]
    public void DigEnded_AllowsCalmAgain()
    {
        digActive.Raise();
        Tick(1f);
        digEnded.Raise();
        Tick(CalmSeconds);

        Assert.IsTrue(director.CalmEnough);
    }

    [Test]
    public void GuardClose_DropsCalmImmediately()
    {
        Tick(CalmSeconds);
        Assert.IsTrue(director.CalmEnough);

        guardClose.Raise();

        Assert.IsFalse(director.CalmEnough);
        Assert.AreEqual(new[] { true, false }, changes);
    }

    [Test]
    public void GuardClear_AllowsCalmAgain()
    {
        guardClose.Raise();
        Tick(CalmSeconds);
        Assert.IsFalse(director.CalmEnough);

        guardClear.Raise();
        Tick(CalmSeconds);

        Assert.IsTrue(director.CalmEnough);
    }

    [Test]
    public void ScriptedBreakingPoint_PreventsCalm()
    {
        scripted.Raise();
        Tick(CalmSeconds + 1f);

        Assert.IsFalse(director.CalmEnough);
    }

    [Test]
    public void ScriptedBreakingPointEnded_AllowsCalmAgain()
    {
        scripted.Raise();
        Tick(1f);
        scriptedEnded.Raise();
        Tick(CalmSeconds);

        Assert.IsTrue(director.CalmEnough);
    }

    [Test]
    public void CalmChanged_FiresOnlyOnFlip()
    {
        Tick(CalmSeconds);
        Tick(1f); // still calm, must not fire again.

        Assert.AreEqual(new[] { true }, changes);
    }

    [Test]
    public void OnDisable_StopsListening()
    {
        TestUtils.Call(director, "OnDisable");

        digActive.Raise();
        TestUtils.Set(director, "stillSeconds", CalmSeconds);
        TestUtils.Call(director, "Update");

        // Dig did not register, so calm is reached. More importantly, no second subscribe was made.
        Assert.IsTrue(director.CalmEnough);
    }
}
