using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests for IntensityScene: picks the right variant for the current mode, honors skipOpening on
/// the opening scene, and lets its instances be destroyed with the scene.
/// </summary>
public class IntensitySceneTests : EditModeTest
{
    private ContentIntensity intensity;

    [SetUp]
    public void SetUp()
    {
        GameEvent modeChanged = Track(TestData.Event("ModeChanged"));
        intensity = Track(ScriptableObject.CreateInstance<ContentIntensity>());
        TestUtils.Set(intensity, "modeChanged", modeChanged);
    }

    [Test]
    public void ImpliedMode_InstantiatesImpliedPrefab()
    {
        intensity.Mode = IntensityLevel.Implied;
        GameObject implied = Make("Implied Prefab");
        GameObject direct = Make("Direct Prefab");
        IntensityScene scene = MakeScene(implied, direct);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(1, scene.Instances.Count);
        Assert.IsTrue(scene.Instances[0].name.StartsWith("Implied Prefab"));
        Track(scene.Instances[0]);
    }

    [Test]
    public void DirectMode_InstantiatesDirectPrefab()
    {
        intensity.Mode = IntensityLevel.Direct;
        GameObject implied = Make("Implied Prefab");
        GameObject direct = Make("Direct Prefab");
        IntensityScene scene = MakeScene(implied, direct);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(1, scene.Instances.Count);
        Assert.IsTrue(scene.Instances[0].name.StartsWith("Direct Prefab"));
        Track(scene.Instances[0]);
    }

    [Test]
    public void NullSide_IsSkipped()
    {
        intensity.Mode = IntensityLevel.Direct;
        GameObject implied = Make("Implied Prefab");
        IntensityScene scene = MakeScene(implied, null);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(0, scene.Instances.Count);
    }

    [Test]
    public void SkipOpening_OnOpeningScene_RaisesSkipAndSpawnsNothing()
    {
        intensity.Mode = IntensityLevel.Implied;
        intensity.SkipOpening = true;
        GameObject implied = Make("Implied Prefab");
        GameObject direct = Make("Direct Prefab");
        GameEvent skipped = Track(TestData.Event("SkippedOpening"));
        var spy = new EventSpy(skipped);
        IntensityScene scene = MakeScene(implied, direct);
        TestUtils.Set(scene, "isOpening", true);
        TestUtils.Set(scene, "skippedOpeningEvent", skipped);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(1, spy.Count);
        Assert.IsTrue(scene.SkippedOpening);
        Assert.AreEqual(0, scene.Instances.Count);
    }

    [Test]
    public void SkipOpeningFalse_InImplied_StillSpawnsTheOpening()
    {
        // The design rule: Implied + skipOpening=false still shows the opening (in Implied form).
        intensity.Mode = IntensityLevel.Implied;
        intensity.SkipOpening = false;
        GameObject implied = Make("Implied Prefab");
        GameObject direct = Make("Direct Prefab");
        GameEvent skipped = Track(TestData.Event("SkippedOpening"));
        var spy = new EventSpy(skipped);
        IntensityScene scene = MakeScene(implied, direct);
        TestUtils.Set(scene, "isOpening", true);
        TestUtils.Set(scene, "skippedOpeningEvent", skipped);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(0, spy.Count);
        Assert.IsFalse(scene.SkippedOpening);
        Assert.AreEqual(1, scene.Instances.Count);
        Assert.IsTrue(scene.Instances[0].name.StartsWith("Implied Prefab"));
        Track(scene.Instances[0]);
    }

    [Test]
    public void NonOpeningScene_IgnoresSkipOpening()
    {
        intensity.Mode = IntensityLevel.Implied;
        intensity.SkipOpening = true;
        GameObject implied = Make("Implied Prefab");
        GameObject direct = Make("Direct Prefab");
        GameEvent skipped = Track(TestData.Event("SkippedOpening"));
        var spy = new EventSpy(skipped);
        IntensityScene scene = MakeScene(implied, direct);
        TestUtils.Set(scene, "isOpening", false);
        TestUtils.Set(scene, "skippedOpeningEvent", skipped);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(0, spy.Count);
        Assert.AreEqual(1, scene.Instances.Count);
        Track(scene.Instances[0]);
    }

    [Test]
    public void MountPoint_ParentsTheInstance()
    {
        intensity.Mode = IntensityLevel.Implied;
        GameObject implied = Make("Implied Prefab");
        Transform mount = Make("Mount").transform;
        IntensityScene scene = MakeScene(implied, null);
        TestUtils.Set(scene, "mountPoint", mount);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(1, scene.Instances.Count);
        Assert.AreSame(mount, scene.Instances[0].transform.parent);
        Track(scene.Instances[0]);
    }

    [Test]
    public void Instances_AreDestroyedWhenTheSceneIsDestroyed()
    {
        intensity.Mode = IntensityLevel.Implied;
        GameObject implied = Make("Implied Prefab");
        IntensityScene scene = MakeScene(implied, null);
        TestUtils.Call(scene, "Awake");
        Assert.AreEqual(1, scene.Instances.Count);
        GameObject instance = scene.Instances[0];
        Assert.IsTrue(instance != null);

        // The scene's host GameObject is destroyed. Children under it, including the instance that
        // was parented to it, are destroyed with it.
        Object.DestroyImmediate(scene.gameObject);

        Assert.IsTrue(instance == null, "The instance should be destroyed with the scene");
    }

    private IntensityScene MakeScene(GameObject impliedPrefab, GameObject directPrefab)
    {
        IntensityScene scene = Make("Scene").AddComponent<IntensityScene>();
        TestUtils.Set(scene, "intensity", intensity);
        TestUtils.Set(scene, "variants", new List<IntensityVariant>
        {
            new IntensityVariant { impliedPrefab = impliedPrefab, directPrefab = directPrefab }
        });
        return scene;
    }
}
