using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for the Accessibility system. One fixture holds each setting and the systems that
/// read it. Nothing writes to disk — tests rebuild the shared settings in memory, leaving the
/// player's PlayerPrefs alone.
/// </summary>
public class AccessibilityTests : EditModeTest
{
    private AccessibilitySettings settings;
    private GameEvent settingsChanged;

    [SetUp]
    public void SetUp()
    {
        settings = Track(ScriptableObject.CreateInstance<AccessibilitySettings>());
        settingsChanged = Track(TestData.Event());
        TestUtils.Set(settings, "settingsChanged", settingsChanged);
        AccessibilitySettings.OverrideCurrent(settings);
        InputMode.ResetAll();
    }

    [TearDown]
    public void TearDownAccessibility()
    {
        AccessibilitySettings.OverrideCurrent(null);
        InputMode.ResetAll();
    }

    [Test]
    public void Settings_Defaults_MatchTheDesign()
    {
        // Subtitles default on (ethical baseline). Every other toggle defaults off so the game feel stays the designed feel.
        var fresh = Track(ScriptableObject.CreateInstance<AccessibilitySettings>());

        Assert.IsFalse(fresh.VisualCuesForSound);
        Assert.IsFalse(fresh.HighContrast);
        Assert.IsFalse(fresh.ColorblindFriendly);
        Assert.IsFalse(fresh.TapInsteadOfHold);
        Assert.IsFalse(fresh.ReducedFlashing);
        Assert.IsFalse(fresh.NoScreenShake);
        Assert.IsTrue(fresh.SubtitlesEnabled);
        Assert.AreEqual(0f, fresh.MinimumBrightness);
    }

    [Test]
    public void Set_UpdatesEveryField_AndRaisesSettingsChanged()
    {
        var spy = new EventSpy(settingsChanged);

        settings.Set(true, true, true, true, true, true, false, 0.2f);

        Assert.IsTrue(settings.VisualCuesForSound);
        Assert.IsTrue(settings.HighContrast);
        Assert.IsTrue(settings.ColorblindFriendly);
        Assert.IsTrue(settings.TapInsteadOfHold);
        Assert.IsTrue(settings.ReducedFlashing);
        Assert.IsTrue(settings.NoScreenShake);
        Assert.IsFalse(settings.SubtitlesEnabled);
        Assert.AreEqual(0.2f, settings.MinimumBrightness, 0.0001f);
        Assert.AreEqual(1, spy.Count, "settingsChanged must fire after a bulk Set");
        spy.Stop();
    }

    [Test]
    public void Set_ClampsMinimumBrightnessToTheRange()
    {
        settings.Set(false, false, false, false, false, false, true, 5f);
        Assert.AreEqual(0.3f, settings.MinimumBrightness, 0.0001f);

        settings.Set(false, false, false, false, false, false, true, -1f);
        Assert.AreEqual(0f, settings.MinimumBrightness, 0.0001f);
    }

    [Test]
    public void SaveThenLoad_RoundTripsEveryField()
    {
        settings.Set(true, false, true, false, true, false, true, 0.15f);
        settings.Save();

        var reloaded = Track(ScriptableObject.CreateInstance<AccessibilitySettings>());
        reloaded.Load();

        Assert.IsTrue(reloaded.VisualCuesForSound);
        Assert.IsFalse(reloaded.HighContrast);
        Assert.IsTrue(reloaded.ColorblindFriendly);
        Assert.IsFalse(reloaded.TapInsteadOfHold);
        Assert.IsTrue(reloaded.ReducedFlashing);
        Assert.IsFalse(reloaded.NoScreenShake);
        Assert.IsTrue(reloaded.SubtitlesEnabled);
        Assert.AreEqual(0.15f, reloaded.MinimumBrightness, 0.0001f);

        // Leave PlayerPrefs clean so the next test or the real game starts fresh.
        foreach (string field in new[] { "visualCuesForSound", "highContrast", "colorblindFriendly", "tapInsteadOfHold",
            "reducedFlashing", "noScreenShake", "subtitlesEnabled", "minimumBrightness" })
        {
            PlayerPrefs.DeleteKey("LITT.Accessibility." + field);
        }
        PlayerPrefs.Save();
    }

    [Test]
    public void VisualCueForSound_RaisesEvent_WhenModeIsOn()
    {
        var cueEvent = Track(ScriptableObject.CreateInstance<SoundCueRaisedEvent>());
        settings.Set(true, false, false, false, false, false, true, 0f);

        var host = Make("Emitter");
        host.transform.position = new Vector3(3f, 2f, 0f);
        var emitter = host.AddComponent<VisualCueForSound>();
        TestUtils.Set(emitter, "soundCueRaised", cueEvent);

        SoundCue received = default;
        int count = 0;
        cueEvent.Subscribe(cue => { received = cue; count++; });

        emitter.Emit(0.7f, "steps");

        Assert.AreEqual(1, count);
        Assert.AreEqual("steps", received.Label);
        Assert.AreEqual(0.7f, received.Loudness, 0.0001f);
    }

    [Test]
    public void VisualCueForSound_StaysSilent_WhenModeIsOff()
    {
        var cueEvent = Track(ScriptableObject.CreateInstance<SoundCueRaisedEvent>());
        settings.Set(false, false, false, false, false, false, true, 0f);

        var host = Make("Emitter Off");
        var emitter = host.AddComponent<VisualCueForSound>();
        TestUtils.Set(emitter, "soundCueRaised", cueEvent);

        int count = 0;
        cueEvent.Subscribe(_ => count++);

        emitter.Emit(1f, "radio");

        Assert.AreEqual(0, count, "With the mode off, the event must not fire");
    }

    [Test]
    public void VisualCueOverlay_SpawnsAChevron_OnEachCue()
    {
        var cueEvent = Track(ScriptableObject.CreateInstance<SoundCueRaisedEvent>());
        settings.Set(true, false, false, false, false, false, true, 0f);

        var host = Make("Overlay");
        var overlay = host.AddComponent<VisualCueOverlay>();
        TestUtils.Set(overlay, "soundCueRaised", cueEvent);
        TestUtils.Call(overlay, "Awake");
        TestUtils.Call(overlay, "OnEnable");

        cueEvent.Raise(new SoundCue(Vector2.right, 0.4f, "a"));
        cueEvent.Raise(new SoundCue(Vector2.up, 0.6f, "b"));

        Assert.AreEqual(2, overlay.LiveChevronCount);

        TestUtils.Call(overlay, "OnDisable");
    }

    [Test]
    public void VisualCueOverlay_IgnoresCues_WhenModeIsOff()
    {
        var cueEvent = Track(ScriptableObject.CreateInstance<SoundCueRaisedEvent>());
        settings.Set(false, false, false, false, false, false, true, 0f);

        var host = Make("Overlay Off");
        var overlay = host.AddComponent<VisualCueOverlay>();
        TestUtils.Set(overlay, "soundCueRaised", cueEvent);
        TestUtils.Call(overlay, "Awake");
        TestUtils.Call(overlay, "OnEnable");

        cueEvent.Raise(new SoundCue(Vector2.right, 1f, "x"));

        Assert.AreEqual(0, overlay.LiveChevronCount);

        TestUtils.Call(overlay, "OnDisable");
    }

    [Test]
    public void ColorblindPatternFor_MapsMoodsToStableShapes()
    {
        Assert.AreEqual(ColorblindFilter.Pattern.Circle, ColorblindFilter.ColorblindPatternFor("calm"));
        Assert.AreEqual(ColorblindFilter.Pattern.Circle, ColorblindFilter.ColorblindPatternFor("faith"));
        Assert.AreEqual(ColorblindFilter.Pattern.Square, ColorblindFilter.ColorblindPatternFor("worry"));
        Assert.AreEqual(ColorblindFilter.Pattern.Triangle, ColorblindFilter.ColorblindPatternFor("despair"));
        Assert.AreEqual(ColorblindFilter.Pattern.Circle, ColorblindFilter.ColorblindPatternFor(null));
        Assert.AreEqual(ColorblindFilter.Pattern.Circle, ColorblindFilter.ColorblindPatternFor("something unknown"));
    }

    [Test]
    public void InputMode_HoldMode_FulfillsAfterRequiredSeconds()
    {
        settings.Set(false, false, false, false, false, false, true, 0f);

        Assert.IsFalse(InputMode.IsHoldFulfilled("dig", 1.0f, 0.4f));
        Assert.IsTrue(InputMode.IsHoldFulfilled("dig", 1.0f, 1.1f));
    }

    [Test]
    public void InputMode_HoldMode_IgnoresTaps()
    {
        settings.Set(false, false, false, false, false, false, true, 0f);

        InputMode.RegisterTap("dig");
        InputMode.RegisterTap("dig");
        InputMode.RegisterTap("dig");

        Assert.AreEqual(0, InputMode.TapCountFor("dig"), "Taps must not count while hold mode is on");
        Assert.IsFalse(InputMode.IsHoldFulfilled("dig", 0.5f, 0f));
    }

    [Test]
    public void InputMode_TapMode_FulfillsAfterEnoughTaps()
    {
        settings.Set(false, false, false, true, false, false, true, 0f);

        InputMode.RegisterTap("prayer");
        Assert.AreEqual(1, InputMode.TapCountFor("prayer"));
        Assert.IsFalse(InputMode.IsHoldFulfilled("prayer", 0.9f));

        InputMode.RegisterTap("prayer");
        InputMode.RegisterTap("prayer");
        Assert.AreEqual(3, InputMode.TapCountFor("prayer"));
        Assert.IsTrue(InputMode.IsHoldFulfilled("prayer", 0.9f), "Three taps worth 0.3s each must fulfil a 0.9s hold");
    }

    [Test]
    public void InputMode_TapMode_ResetsAfterAction()
    {
        settings.Set(false, false, false, true, false, false, true, 0f);

        InputMode.RegisterTap("prayer");
        InputMode.RegisterTap("prayer");
        InputMode.Reset("prayer");

        Assert.AreEqual(0, InputMode.TapCountFor("prayer"));
    }

    [Test]
    public void ReducedFlashing_ShouldFlash_FollowsTheToggle()
    {
        settings.Set(false, false, false, false, false, false, true, 0f);
        Assert.IsTrue(ReducedFlashing.ShouldFlash);

        settings.Set(false, false, false, false, true, false, true, 0f);
        Assert.IsFalse(ReducedFlashing.ShouldFlash);
    }

    [Test]
    public void ScreenShakeGuard_NoOps_WhenNoScreenShakeIsOn()
    {
        var shakeEvent = Track(TestData.Event());
        var spy = new EventSpy(shakeEvent);

        settings.Set(false, false, false, false, false, true, true, 0f);
        ScreenShakeGuard.Shake(shakeEvent);
        Assert.AreEqual(0, spy.Count, "With noScreenShake on, the guard must not raise the event");

        settings.Set(false, false, false, false, false, false, true, 0f);
        ScreenShakeGuard.Shake(shakeEvent);
        Assert.AreEqual(1, spy.Count, "With noScreenShake off, the guard forwards the event");

        spy.Stop();
    }

    [Test]
    public void ScreenShakeGuard_Scale_ReturnsZeroWhenMuted()
    {
        settings.Set(false, false, false, false, false, true, true, 0f);
        Assert.AreEqual(0f, ScreenShakeGuard.Scale(0.5f), 0.0001f);

        settings.Set(false, false, false, false, false, false, true, 0f);
        Assert.AreEqual(0.5f, ScreenShakeGuard.Scale(0.5f), 0.0001f);
    }

    [Test]
    public void SubtitleManager_Queue_DropsOldestBeyondTwo()
    {
        var subtitleEvent = Track(ScriptableObject.CreateInstance<SubtitleRequestedEvent>());
        settings.Set(false, false, false, false, false, false, true, 0f);

        var host = Make("Subtitles");
        host.SetActive(false); // Inactive so Enqueue skips StartCoroutine and the queue tracks exactly what was pushed.
        var manager = host.AddComponent<SubtitleManager>();
        TestUtils.Set(manager, "subtitleRequested", subtitleEvent);
        TestUtils.Call(manager, "Awake");

        manager.Enqueue(new SubtitleRequest("one", "narrator", 1f, SubtitleLanguage.En));
        manager.Enqueue(new SubtitleRequest("two", "narrator", 1f, SubtitleLanguage.En));
        manager.Enqueue(new SubtitleRequest("three", "narrator", 1f, SubtitleLanguage.En));

        Assert.AreEqual(2, manager.QueueLength, "The queue must never grow past two; the oldest drops first");
    }

    [Test]
    public void SubtitleManager_Event_RoutesIntoTheManager()
    {
        var subtitleEvent = Track(ScriptableObject.CreateInstance<SubtitleRequestedEvent>());
        settings.Set(false, false, false, false, false, false, true, 0f);

        var host = Make("Subtitles Event");
        host.SetActive(false); // Inactive so Enqueue takes the no-coroutine branch and keeps the request queued.
        var manager = host.AddComponent<SubtitleManager>();
        TestUtils.Set(manager, "subtitleRequested", subtitleEvent);
        TestUtils.Call(manager, "Awake");
        TestUtils.Call(manager, "OnEnable");

        subtitleEvent.Raise(new SubtitleRequest("heard", "guard", 1f, SubtitleLanguage.He));

        Assert.GreaterOrEqual(manager.QueueLength, 1, "An event raise must leave the request in the queue");

        TestUtils.Call(manager, "OnDisable");
    }

    [Test]
    public void AccessibilityManager_Instance_PointsToTheFirstManager()
    {
        var host = Make("Manager");
        var manager = host.AddComponent<AccessibilityManager>();
        TestUtils.Set(manager, "settings", settings);
        TestUtils.Set(manager, "dontDestroyOnLoad", false);
        TestUtils.Call(manager, "Awake");

        Assert.AreEqual(manager, AccessibilityManager.Instance);
    }
}
