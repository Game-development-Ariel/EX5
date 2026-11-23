using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests for the content intensity data asset, its manager (load and save from PlayerPrefs),
/// and a small check that the IntensityScene picks the right variant.
/// Every test clears the two PlayerPrefs keys in TearDown so one test cannot leak into another.
/// </summary>
public class ContentIntensityTests : EditModeTest
{
    private ContentIntensity intensity;
    private GameEvent modeChanged;

    [SetUp]
    public void SetUp()
    {
        ClearPrefs();
        modeChanged = Track(TestData.Event("ModeChanged"));
        intensity = Track(ScriptableObject.CreateInstance<ContentIntensity>());
        TestUtils.Set(intensity, "modeChanged", modeChanged);
    }

    [TearDown]
    public void CleanPrefs()
    {
        ClearPrefs();
    }

    private static void ClearPrefs()
    {
        PlayerPrefs.DeleteKey(ContentIntensityManager.ModeKey);
        PlayerPrefs.DeleteKey(ContentIntensityManager.ContentWarningShownKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void DefaultMode_IsImplied()
    {
        Assert.AreEqual(IntensityLevel.Implied, intensity.Mode);
    }

    [Test]
    public void DefaultContentWarningShown_IsFalse()
    {
        Assert.IsFalse(intensity.ContentWarningShown);
    }

    [Test]
    public void SetMode_ToDifferentValue_RaisesModeChanged()
    {
        var spy = new EventSpy(modeChanged);

        intensity.Mode = IntensityLevel.Direct;

        Assert.AreEqual(1, spy.Count);
        Assert.AreEqual(IntensityLevel.Direct, intensity.Mode);
    }

    [Test]
    public void SetMode_ToSameValue_DoesNotRaise()
    {
        var spy = new EventSpy(modeChanged);

        intensity.Mode = IntensityLevel.Implied;

        Assert.AreEqual(0, spy.Count);
    }

    [Test]
    public void SetMode_WithoutEventAsset_IsSafe()
    {
        TestUtils.Set(intensity, "modeChanged", null);

        Assert.DoesNotThrow(() => intensity.Mode = IntensityLevel.Direct);
        Assert.AreEqual(IntensityLevel.Direct, intensity.Mode);
    }

    [Test]
    public void Manager_LoadsModeFromPlayerPrefs()
    {
        PlayerPrefs.SetInt(ContentIntensityManager.ModeKey, (int)IntensityLevel.Direct);
        var manager = MakeManager();

        TestUtils.Call(manager, "Awake");

        Assert.AreEqual(IntensityLevel.Direct, intensity.Mode);
        Assert.AreSame(intensity, ContentIntensityManager.Current);
    }

    [Test]
    public void Manager_WithNoPrefs_KeepsAssetDefault()
    {
        intensity.Mode = IntensityLevel.Direct;
        var manager = MakeManager();

        TestUtils.Call(manager, "Awake");

        Assert.AreEqual(IntensityLevel.Direct, intensity.Mode);
    }

    [Test]
    public void Manager_LoadsWarningShown()
    {
        PlayerPrefs.SetInt(ContentIntensityManager.ContentWarningShownKey, 1);
        var manager = MakeManager();

        TestUtils.Call(manager, "Awake");

        Assert.IsTrue(intensity.ContentWarningShown);
    }

    [Test]
    public void Manager_SavesOnModeChange()
    {
        var manager = MakeManager();
        TestUtils.Call(manager, "Awake");
        TestUtils.Call(manager, "OnEnable");

        intensity.Mode = IntensityLevel.Direct;

        Assert.AreEqual((int)IntensityLevel.Direct, PlayerPrefs.GetInt(ContentIntensityManager.ModeKey));
    }

    [Test]
    public void Manager_SaveAlsoWritesFlags()
    {
        intensity.ContentWarningShown = true;
        var manager = MakeManager();
        TestUtils.Call(manager, "Awake");

        TestUtils.Call(manager, "Save");

        Assert.AreEqual(1, PlayerPrefs.GetInt(ContentIntensityManager.ContentWarningShownKey, -1));
    }

    [Test]
    public void Manager_OnDisable_StopsSaving()
    {
        var manager = MakeManager();
        TestUtils.Call(manager, "Awake");
        TestUtils.Call(manager, "OnEnable");
        TestUtils.Call(manager, "OnDisable");
        PlayerPrefs.DeleteKey(ContentIntensityManager.ModeKey);

        intensity.Mode = IntensityLevel.Direct;

        Assert.IsFalse(PlayerPrefs.HasKey(ContentIntensityManager.ModeKey));
    }

    [Test]
    public void IntensityScene_InImpliedMode_InstantiatesImpliedPrefab()
    {
        GameObject impliedPrefab = Make("Implied Prefab");
        GameObject directPrefab = Make("Direct Prefab");
        IntensityScene scene = MakeSceneWithOneVariant(impliedPrefab, directPrefab);

        TestUtils.Call(scene, "Awake");

        Assert.AreEqual(1, scene.Instances.Count);
        Assert.IsTrue(scene.Instances[0].name.StartsWith("Implied Prefab"));
        Track(scene.Instances[0]);
    }

    private ContentIntensityManager MakeManager()
    {
        ContentIntensityManager manager = Make("Manager").AddComponent<ContentIntensityManager>();
        TestUtils.Set(manager, "intensity", intensity);
        return manager;
    }

    private IntensityScene MakeSceneWithOneVariant(GameObject impliedPrefab, GameObject directPrefab)
    {
        IntensityScene scene = Make("Scene").AddComponent<IntensityScene>();
        TestUtils.Set(scene, "intensity", intensity);
        TestUtils.Set(scene, "variants", new System.Collections.Generic.List<IntensityVariant>
        {
            new IntensityVariant { impliedPrefab = impliedPrefab, directPrefab = directPrefab }
        });
        return scene;
    }
}
