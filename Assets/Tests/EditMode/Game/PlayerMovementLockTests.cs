using NUnit.Framework;
using UnityEngine;

public class PlayerMovementLockTests
{
    private GameObject host;
    private PlayerMovement movement;
    private PlayerMovementLock lockComponent;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("PlayerForLockTest");
        host.AddComponent<Rigidbody>();
        movement = host.AddComponent<PlayerMovement>();
        lockComponent = host.AddComponent<PlayerMovementLock>();
        lockComponent.SendMessage("Awake");
        lockComponent.enabled = true;
        lockComponent.SendMessage("OnEnable");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
    }

    [Test]
    public void NoReasons_PlayerIsFree()
    {
        Assert.IsTrue(lockComponent.IsFree);
    }

    [Test]
    public void Hold_FreezesThePlayer()
    {
        lockComponent.Hold("dig");
        Assert.IsFalse(lockComponent.IsFree);
        Assert.IsFalse(movement.enabled);
    }

    [Test]
    public void Release_UnfreezesThePlayer()
    {
        lockComponent.Hold("dig");
        lockComponent.Release("dig");

        Assert.IsTrue(lockComponent.IsFree);
        Assert.IsTrue(movement.enabled);
    }

    [Test]
    public void TwoReasonsHeld_PlayerStaysFrozenUntilBothRelease()
    {
        lockComponent.Hold("dig");
        lockComponent.Hold("prayer");
        lockComponent.Release("dig");

        Assert.IsFalse(lockComponent.IsFree,
            "Player should still be frozen because prayer reason is held");
        Assert.IsFalse(movement.enabled);

        lockComponent.Release("prayer");
        Assert.IsTrue(lockComponent.IsFree);
        Assert.IsTrue(movement.enabled);
    }

    [Test]
    public void ReleaseUnknownReason_IsNoOp()
    {
        lockComponent.Release("never-added");
        Assert.IsTrue(lockComponent.IsFree);
        Assert.IsTrue(movement.enabled);
    }

    [Test]
    public void FreeChanged_FiresOnFirstHoldAndFullRelease()
    {
        int freeTransitions = 0;
        bool lastSeen = true;
        lockComponent.FreeChanged += free =>
        {
            freeTransitions++;
            lastSeen = free;
        };

        lockComponent.Hold("dig");
        Assert.AreEqual(1, freeTransitions);
        Assert.IsFalse(lastSeen);

        lockComponent.Hold("prayer");
        Assert.AreEqual(1, freeTransitions,
            "Second hold should not fire — we were already frozen");

        lockComponent.Release("dig");
        Assert.AreEqual(1, freeTransitions,
            "Partial release should not fire — still frozen");

        lockComponent.Release("prayer");
        Assert.AreEqual(2, freeTransitions);
        Assert.IsTrue(lastSeen);
    }

    [Test]
    public void SetDesiredEnabledFalse_KeepsPlayerFrozenEvenWithoutReasons()
    {
        lockComponent.SetDesiredEnabled(false);
        Assert.IsFalse(movement.enabled);

        lockComponent.SetDesiredEnabled(true);
        Assert.IsTrue(movement.enabled);
    }

    [Test]
    public void ReleaseAll_ClearsEveryReasonAtOnce()
    {
        lockComponent.Hold("dig");
        lockComponent.Hold("prayer");
        lockComponent.Hold("journal");
        Assert.IsFalse(lockComponent.IsFree);

        lockComponent.ReleaseAll();
        Assert.IsTrue(lockComponent.IsFree);
        Assert.IsTrue(movement.enabled);
    }

    [Test]
    public void ActiveReasons_RevealsWhoHolds()
    {
        lockComponent.Hold("dig");
        lockComponent.Hold("prayer");

        CollectionAssert.AreEquivalent(new[] { "dig", "prayer" }, lockComponent.ActiveReasons);
    }

    [Test]
    public void EmptyReason_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => lockComponent.Hold(""));
        Assert.Throws<System.ArgumentException>(() => lockComponent.Hold(null));
    }
}
