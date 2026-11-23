using NUnit.Framework;
using UnityEngine;

/// <summary>
/// The highlight target becomes bright or dark on SetHighlighted. In edit mode no coroutine
/// runs, so the tween takes the instant path: the final values are applied right away.
/// </summary>
public class HighlightTargetTests : EditModeTest
{
    private const float TestOnIntensity = 2f;
    private const float TestOffIntensity = 0.1f;

    private HighlightTarget target;
    private SpriteRenderer sprite;

    [SetUp]
    public void SetUp()
    {
        GameObject go = Make("Target");
        sprite = go.AddComponent<SpriteRenderer>();
        target = go.AddComponent<HighlightTarget>();
        TestUtils.Set(target, "onIntensity", TestOnIntensity);
        TestUtils.Set(target, "offIntensity", TestOffIntensity);
        TestUtils.Set(target, "fallbackSprite", sprite);
        TestUtils.Set(target, "onAlpha", 1f);
        TestUtils.Set(target, "offAlpha", 0f);
        TestUtils.Call(target, "Awake");
    }

    [Test]
    public void AtStart_TheTargetIsNotHighlighted()
    {
        Assert.IsFalse(target.IsHighlighted);
    }

    [Test]
    public void AtStart_TheFallbackSpriteStartsAtTheOffAlpha()
    {
        Assert.AreEqual(0f, sprite.color.a);
    }

    [Test]
    public void SetHighlighted_True_TurnsTheTargetOn()
    {
        target.SetHighlighted(true, 0f);

        Assert.IsTrue(target.IsHighlighted);
    }

    [Test]
    public void SetHighlighted_False_TurnsTheTargetOff()
    {
        target.SetHighlighted(true, 0f);

        target.SetHighlighted(false, 0f);

        Assert.IsFalse(target.IsHighlighted);
    }

    [Test]
    public void SetHighlighted_True_RaisesTheFallbackSpriteAlpha()
    {
        target.SetHighlighted(true, 0f);

        Assert.AreEqual(1f, sprite.color.a);
    }

    [Test]
    public void SetHighlighted_False_LowersTheFallbackSpriteAlpha()
    {
        target.SetHighlighted(true, 0f);
        target.SetHighlighted(false, 0f);

        Assert.AreEqual(0f, sprite.color.a);
    }

    [Test]
    public void SetHighlighted_True_WithNoLerp_ReachesTheOnValuesImmediately()
    {
        // In edit mode the component is not active, so the tween path falls back to an instant apply.
        target.SetHighlighted(true, 1f);

        Assert.AreEqual(1f, sprite.color.a);
    }

    [Test]
    public void SetHighlighted_TwiceOn_StaysOn()
    {
        target.SetHighlighted(true, 0f);
        target.SetHighlighted(true, 0f);

        Assert.IsTrue(target.IsHighlighted);
    }

    [Test]
    public void OnAndOffIntensity_AreReadable()
    {
        Assert.AreEqual(TestOnIntensity, target.OnIntensity);
        Assert.AreEqual(TestOffIntensity, target.OffIntensity);
    }
}
