using NUnit.Framework;
using UnityEngine;

public class HopeBodyTests
{
    [Test]
    public void TierOf_HighHope_IsFaith()
    {
        Assert.AreEqual(HopeTier.Faith, HopeBody.TierOf(90f, HopeTier.Calm, hysteresis: 0f));
        Assert.AreEqual(HopeTier.Faith, HopeBody.TierOf(100f, HopeTier.Calm, hysteresis: 0f));
    }

    [Test]
    public void TierOf_MidHope_IsCalm()
    {
        Assert.AreEqual(HopeTier.Calm, HopeBody.TierOf(60f, HopeTier.Faith, hysteresis: 0f));
        Assert.AreEqual(HopeTier.Calm, HopeBody.TierOf(50f, HopeTier.Faith, hysteresis: 0f));
    }

    [Test]
    public void TierOf_LowHope_IsWorry()
    {
        Assert.AreEqual(HopeTier.Worry, HopeBody.TierOf(30f, HopeTier.Calm, hysteresis: 0f));
        Assert.AreEqual(HopeTier.Worry, HopeBody.TierOf(25f, HopeTier.Calm, hysteresis: 0f));
    }

    [Test]
    public void TierOf_VeryLowHope_IsDespair()
    {
        Assert.AreEqual(HopeTier.Despair, HopeBody.TierOf(20f, HopeTier.Worry, hysteresis: 0f));
        Assert.AreEqual(HopeTier.Despair, HopeBody.TierOf(0f, HopeTier.Worry, hysteresis: 0f));
    }

    [Test]
    public void TierOf_AtFaithBoundary_IsFaith()
    {
        Assert.AreEqual(HopeTier.Faith, HopeBody.TierOf(75f, HopeTier.Calm, hysteresis: 0f));
    }

    [Test]
    public void TierOf_HysteresisPreventsFlickerOnDescent()
    {
        HopeTier current = HopeTier.Faith;
        float hysteresis = 1.5f;

        Assert.AreEqual(HopeTier.Faith, HopeBody.TierOf(74.5f, current, hysteresis),
            "Just below 75 should stay Faith while in Faith due to hysteresis");
        Assert.AreEqual(HopeTier.Calm, HopeBody.TierOf(73f, current, hysteresis),
            "Drop below 73.5 should fall out of Faith");
    }

    [Test]
    public void TierOf_HysteresisPreventsFlickerOnAscent()
    {
        HopeTier current = HopeTier.Calm;
        Assert.AreEqual(HopeTier.Calm, HopeBody.TierOf(74.9f, current, hysteresis: 1.5f),
            "Below 75 from Calm stays Calm");
        Assert.AreEqual(HopeTier.Faith, HopeBody.TierOf(75.1f, current, hysteresis: 1.5f),
            "Above 75 from Calm goes to Faith");
    }

    [Test]
    public void ContinuousHope_AddFractional_DoesNotFlushUntilBoundary()
    {
        GameObject go = new GameObject("HopeHost");
        TunnelLevelSettings settings = ScriptableObject.CreateInstance<TunnelLevelSettings>();
        settings.maxHope = 100;
        settings.startHope = 50;

        Hope hope = go.AddComponent<Hope>();
        typeof(Hope).GetField("settings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(hope, settings);
        TestUtils.Call(hope, "Awake");

        ContinuousHope continuous = go.AddComponent<ContinuousHope>();
        TestUtils.Call(continuous, "Awake");
        TestUtils.Call(continuous, "Start");

        int before = hope.Current;
        continuous.Add(0.4f);
        continuous.Add(0.3f);
        Assert.AreEqual(before, hope.Current, "Fractional additions below 1 do not change the integer");

        continuous.Add(0.4f);
        Assert.AreEqual(before + 1, hope.Current, "Fractional additions that cross the boundary advance the integer");

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(settings);
    }

    [Test]
    public void ContinuousHope_LoseFractional_FlushesOnBoundary()
    {
        GameObject go = new GameObject("HopeHost");
        TunnelLevelSettings settings = ScriptableObject.CreateInstance<TunnelLevelSettings>();
        settings.maxHope = 100;
        settings.startHope = 50;

        Hope hope = go.AddComponent<Hope>();
        typeof(Hope).GetField("settings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(hope, settings);
        TestUtils.Call(hope, "Awake");

        ContinuousHope continuous = go.AddComponent<ContinuousHope>();
        TestUtils.Call(continuous, "Awake");
        TestUtils.Call(continuous, "Start");

        int before = hope.Current;
        continuous.Lose(0.9f);
        Assert.AreEqual(before, hope.Current, "Fractional loss below 1 does not change the integer");

        continuous.Lose(0.2f);
        Assert.AreEqual(before - 1, hope.Current, "Crossing the boundary lowers the integer");

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(settings);
    }

    [Test]
    public void ContinuousHope_ClampsToZero()
    {
        GameObject go = new GameObject("HopeHost");
        TunnelLevelSettings settings = ScriptableObject.CreateInstance<TunnelLevelSettings>();
        settings.maxHope = 100;
        settings.startHope = 5;

        Hope hope = go.AddComponent<Hope>();
        typeof(Hope).GetField("settings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(hope, settings);
        TestUtils.Call(hope, "Awake");

        ContinuousHope continuous = go.AddComponent<ContinuousHope>();
        TestUtils.Call(continuous, "Awake");
        TestUtils.Call(continuous, "Start");

        continuous.Lose(200f);
        Assert.AreEqual(0f, continuous.Value, 0.001f);
        Assert.AreEqual(0, hope.Current);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(settings);
    }

    [Test]
    public void ContinuousHope_ClampsToFullScale()
    {
        GameObject go = new GameObject("HopeHost");
        TunnelLevelSettings settings = ScriptableObject.CreateInstance<TunnelLevelSettings>();
        settings.maxHope = 100;
        settings.startHope = 95;

        Hope hope = go.AddComponent<Hope>();
        typeof(Hope).GetField("settings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(hope, settings);
        TestUtils.Call(hope, "Awake");

        ContinuousHope continuous = go.AddComponent<ContinuousHope>();
        TestUtils.Call(continuous, "Awake");
        TestUtils.Call(continuous, "Start");

        continuous.Add(50f);
        Assert.AreEqual(ContinuousHope.FullScale, continuous.Value, 0.001f);

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(settings);
    }
}
