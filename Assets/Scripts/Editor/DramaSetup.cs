using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Puts the dramatic elements of assignment 5 into the Tutorial scene and wires them to the level:
/// the start menu (content warning and difficulty), the difficulty and its dynamic layer, the goal lights,
/// the hope's body, the accessibility settings, the optional moments and missions, and the ending.
/// It runs after the other setup tools, because it wires to the objects they build.
/// </summary>
public static class DramaSetup
{
    private const string DataFolder = "Assets/Data/";
    private const string EventsFolder = DataFolder + "Events/";
    private const string UiFolder = "Assets/Resources/UI/";

    // The sprites this tool needs that the game feel tool doesn't import: the shapes and the check mark.
    private static readonly string[] NewSprites = { "Shape_Circle", "Shape_Square", "Shape_Triangle", "Check" };

    // The light on the current goal: how far it reaches (world units), how bright, and its color.
    private const float GoalLightRadius = 2.4f;
    private const float GoalLightInnerRadius = 0.3f;
    private const float GoalLightIntensity = 0.9f;
    private static readonly Color BottleLightColor = new Color(0.62f, 0.8f, 1f);
    private static readonly Color MattressLightColor = new Color(1f, 0.82f, 0.58f);

    // The pulse is silent outside Despair: Tension already plays a heartbeat when a watcher is near.
    private const float NormalPulseGain = 0f;

    // The optional moments of the tutorial: where they are and how big, in world units (one unit is one tile).
    private static readonly Vector2 DoorCrackPosition = new Vector2(-1.5f, -0.6f);
    private static readonly Vector3 DoorCrackSize = new Vector3(1f, 1.2f, 1f);
    private static readonly Vector2 CountingWallPosition = new Vector2(-4.5f, 3.5f);
    private const float CountingWallRadius = 1f;

    // The tutorial's short ending: seconds for each of its two beats.
    private const float FirstNightSeconds = 4f;
    private const float FinaleSeconds = 5f;

    // EndingLanguage.English as the number the serialized field holds.
    private const int EnglishLanguageIndex = 1;

    [MenuItem("Tools/Tunnel/4 Setup Dramatic Elements")]
    public static void Setup()
    {
        Scene level = SceneManager.GetActiveScene();
        if (level.path != TunnelAssets.ScenePath)
        {
            Debug.LogError("Open the Tutorial scene first.");
            return;
        }

        ImportNewSprites();

        GameObject player = GameObject.Find("Player");
        Hope hope = player.GetComponent<Hope>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Transform watchers = GameObject.Find("Watchers").transform;
        Tension tension = Object.FindAnyObjectByType<Tension>();

        PlayerMovementLock movementLock = SetupTools.GetOrAdd<PlayerMovementLock>(player);
        ContinuousHope continuousHope = SetupTools.GetOrAdd<ContinuousHope>(player);
        SetupTools.GetOrAdd<HopeNaturalDecay>(player);
        HopeBody body = SetupBody(player);

        GameObject root = SetupTools.Recreate("Dramatic Elements");

        SkillTracker tracker = SetupDifficulty(Child(root, "Difficulty"), hope, movement, watchers, out DifficultyManager difficulty, out DifficultyModulator modulator);
        AttentionDirector attention = SetupAttention(Child(root, "Attention"));
        SubtitleManager subtitles = SetupAccessibility(Child(root, "Accessibility"), player, hope, body);
        SlowMomentsDirector calm = SetupOptional(Child(root, "Optional"), player, movement, tension);
        ContentIntensity intensity = Intensity();
        SetupEnding(Child(root, "Ending"), intensity, subtitles);
        ContentIntensityManager intensityManager = SetupStartMenu(Child(root, "Start Menu"), intensity, difficulty);

        GameObject options = Child(root, "Options");
        OptionsMenu menu = options.AddComponent<OptionsMenu>();
        SetupTools.SetField(menu, "movementLock", movementLock);
        SetupTools.SetField(menu, "intensityManager", intensityManager);
        DevOverlay overlay = options.AddComponent<DevOverlay>();
        SetupTools.SetField(overlay, "hope", hope);
        SetupTools.SetField(overlay, "continuousHope", continuousHope);
        SetupTools.SetField(overlay, "body", body);
        SetupTools.SetField(overlay, "movementLock", movementLock);
        SetupTools.SetField(overlay, "tracker", tracker);
        SetupTools.SetField(overlay, "modulator", modulator);
        SetupTools.SetField(overlay, "calm", calm);
        SetupTools.SetField(overlay, "attention", attention);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(level);
        EditorSceneManager.SaveScene(level);
        Debug.Log("Dramatic elements setup done and saved.");
    }

    // ---------- Section 2: the hope's body ----------

    private static HopeBody SetupBody(GameObject player)
    {
        Transform old = player.transform.Find("Body Sounds");
        if (old != null)
        {
            Object.DestroyImmediate(old.gameObject);
        }
        var sounds = new GameObject("Body Sounds");
        sounds.transform.SetParent(player.transform, false);

        AudioClip breath = Resources.Load<AudioClip>("Audio/Breathing");
        AudioSource breathSource = LoopingSource(sounds.transform, "Breath", null, false);
        AudioSource pulseSource = LoopingSource(sounds.transform, "Pulse", Resources.Load<AudioClip>("Audio/Heartbeat"), true);

        HopeBody body = SetupTools.GetOrAdd<HopeBody>(player);
        SetupTools.SetField(body, "breathSource", breathSource);
        SetupTools.SetField(body, "quietBreathClip", breath);
        SetupTools.SetField(body, "audibleBreathClip", breath);
        SetupTools.SetField(body, "fastBreathClip", breath);
        SetupTools.SetField(body, "pulseSource", pulseSource);
        SetFloat(body, "normalPulseGain", NormalPulseGain);
        return body;
    }

    private static AudioSource LoopingSource(Transform parent, string name, AudioClip clip, bool playOnAwake)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = playOnAwake;
        source.volume = 0f; // HopeBody raises it.
        return source;
    }

    // ---------- Section 1: the difficulty ----------

    private static SkillTracker SetupDifficulty(GameObject holder, Hope hope, PlayerMovement movement, Transform watchers,
        out DifficultyManager manager, out DifficultyModulator modulator)
    {
        DifficultyProfile medium = Profile(Difficulty.Medium);
        Profile(Difficulty.Easy);
        Profile(Difficulty.Hard);

        GameEvent profileLoaded = Event("ProfileLoaded");
        var caught = SetupTools.Asset<CaughtEvent>(EventsFolder + "Caught.asset");
        var stageStarted = SetupTools.Asset<StageStartedEvent>(EventsFolder + "StageStarted.asset");
        var stageEnded = SetupTools.Asset<StageEndedEvent>(EventsFolder + "StageEnded.asset");
        var hopeChanged = SetupTools.Asset<HopeChangedEvent>(EventsFolder + "HopeChanged.asset");
        var playerIdle = SetupTools.Asset<PlayerIdleSignal>(EventsFolder + "PlayerIdle.asset");
        var quietEntered = SetupTools.Asset<QuietPlaceEnteredEvent>(EventsFolder + "QuietPlaceEntered.asset");
        var quietExited = SetupTools.Asset<QuietPlaceExitedEvent>(EventsFolder + "QuietPlaceExited.asset");

        manager = holder.AddComponent<DifficultyManager>();
        SetupTools.SetField(manager, "defaultProfile", medium);
        SetupTools.SetField(manager, "profileLoaded", profileLoaded);

        SkillTracker tracker = holder.AddComponent<SkillTracker>();
        SetupTools.SetField(tracker, "caught", caught);
        SetupTools.SetField(tracker, "playerIdle", playerIdle);
        SetupTools.SetField(tracker, "hopeChanged", hopeChanged);
        SetupTools.SetField(tracker, "quietPlaceEntered", quietEntered);
        SetupTools.SetField(tracker, "quietPlaceExited", quietExited);
        SetupTools.SetField(tracker, "stageStarted", stageStarted);
        SetupTools.SetField(tracker, "stageEnded", stageEnded);

        modulator = holder.AddComponent<DifficultyModulator>();
        SetupTools.SetField(modulator, "skillTracker", tracker);
        SetupTools.SetField(modulator, "mediumProfile", medium);
        SetupTools.SetField(modulator, "stageStarted", stageStarted);
        // The dark after a capture is the tutorial's scripted sequence: the difficulty doesn't change inside it.
        SetupTools.SetArray(modulator, "scriptedSequenceStarts", new Object[] { Event("ScriptedBeatStarted") });
        SetupTools.SetArray(modulator, "scriptedSequenceEnds", new Object[] { Event("ScriptedBeatEnded") });

        DifficultyApplier applier = holder.AddComponent<DifficultyApplier>();
        SetupTools.SetField(applier, "hope", hope);
        SetupTools.SetField(applier, "modulator", modulator);
        SetupTools.SetField(applier, "watchers", watchers);
        SetupTools.SetField(applier, "profileLoaded", profileLoaded);

        SkillSignals signals = holder.AddComponent<SkillSignals>();
        SetupTools.SetField(signals, "hope", hope);
        SetupTools.SetField(signals, "player", movement);
        SetupTools.SetField(signals, "tracker", tracker);
        SetupTools.SetField(signals, "playerCaught", TunnelAssets.PlayerCaught);
        SetupTools.SetField(signals, "openingFinished", TunnelAssets.OpeningFinished);
        SetupTools.SetField(signals, "levelWon", TunnelAssets.LevelWon);
        SetupTools.SetField(signals, "levelLost", TunnelAssets.LevelLost);
        SetupTools.SetField(signals, "memoryFound", TunnelAssets.MemoryFound);
        SetupTools.SetField(signals, "caught", caught);
        SetupTools.SetField(signals, "stageStarted", stageStarted);
        SetupTools.SetField(signals, "stageEnded", stageEnded);
        SetupTools.SetField(signals, "hopeChanged", hopeChanged);
        SetupTools.SetField(signals, "playerIdle", playerIdle);

        foreach (QuietPlace place in Object.FindObjectsByType<QuietPlace>(FindObjectsSortMode.None))
        {
            SetupTools.SetField(place, "entered", quietEntered);
            SetupTools.SetField(place, "exited", quietExited);
            SetString(place, "optionalId", "prayer-spot-sit");
        }
        return tracker;
    }

    private static DifficultyProfile Profile(Difficulty difficulty)
    {
        string path = DataFolder + "Difficulty/" + difficulty + ".asset";
        DifficultyProfile profile = AssetDatabase.LoadAssetAtPath<DifficultyProfile>(path);
        if (profile == null)
        {
            System.IO.Directory.CreateDirectory(DataFolder + "Difficulty");
            profile = DifficultyProfileFactory.Create(difficulty);
            AssetDatabase.CreateAsset(profile, path);
        }
        return profile;
    }

    // ---------- Section 2: the goal lights ----------

    private static AttentionDirector SetupAttention(GameObject holder)
    {
        HighlightTarget bottle = GoalLight(GameObject.Find("BottleSpawn"), BottleLightColor);
        HighlightTarget mattress = GoalLight(GameObject.Find("MattressGoal"), MattressLightColor);

        // The story of the level in two states: looking for the water, and carrying it back.
        // Losing the bottle to a capture goes back to the first.
        SceneStateMachine machine = holder.AddComponent<SceneStateMachine>();
        var machineFields = new SerializedObject(machine);
        SerializedProperty states = machineFields.FindProperty("states");
        states.arraySize = 2;
        SetState(states.GetArrayElementAtIndex(0), "Find the water", TunnelAssets.BottleDropped);
        SetState(states.GetArrayElementAtIndex(1), "Bring it back", TunnelAssets.BottlePicked);
        machineFields.ApplyModifiedPropertiesWithoutUndo();

        AttentionDirector director = holder.AddComponent<AttentionDirector>();
        var directorFields = new SerializedObject(director);
        SerializedProperty beats = directorFields.FindProperty("beats");
        beats.arraySize = 2;
        SetBeat(beats.GetArrayElementAtIndex(0), "Water lit", bottle, mattress);
        SetBeat(beats.GetArrayElementAtIndex(1), "Mattress lit", mattress, bottle);
        directorFields.FindProperty("sceneStateMachine").objectReferenceValue = machine;
        directorFields.ApplyModifiedPropertiesWithoutUndo();
        return director;
    }

    private static HighlightTarget GoalLight(GameObject goal, Color color)
    {
        Transform old = goal.transform.Find("Goal Light");
        if (old != null)
        {
            Object.DestroyImmediate(old.gameObject);
        }
        var lightObject = new GameObject("Goal Light");
        lightObject.transform.SetParent(goal.transform, false);

        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.pointLightOuterRadius = GoalLightRadius;
        light.pointLightInnerRadius = GoalLightInnerRadius;
        light.intensity = 0f; // The AttentionDirector raises it when this is the goal.
        light.color = color;

        HighlightTarget target = lightObject.AddComponent<HighlightTarget>();
        SetupTools.SetField(target, "highlightLight", light);
        SetFloat(target, "onIntensity", GoalLightIntensity);
        return target;
    }

    private static void SetState(SerializedProperty state, string name, GameEvent onEnter)
    {
        state.FindPropertyRelative("name").stringValue = name;
        state.FindPropertyRelative("onEnter").objectReferenceValue = onEnter;
        state.FindPropertyRelative("onExit").objectReferenceValue = null;
    }

    private static void SetBeat(SerializedProperty beat, string name, HighlightTarget toLight, HighlightTarget toDim)
    {
        beat.FindPropertyRelative("name").stringValue = name;
        beat.FindPropertyRelative("ambientBoostMultiplier").floatValue = 1f;
        SerializedProperty lit = beat.FindPropertyRelative("toLight");
        lit.arraySize = 1;
        lit.GetArrayElementAtIndex(0).objectReferenceValue = toLight;
        SerializedProperty dimmed = beat.FindPropertyRelative("toDim");
        dimmed.arraySize = 1;
        dimmed.GetArrayElementAtIndex(0).objectReferenceValue = toDim;
    }

    // ---------- Section 3: accessibility ----------

    private static SubtitleManager SetupAccessibility(GameObject holder, GameObject player, Hope hope, HopeBody body)
    {
        AccessibilitySettings settings = SetupTools.Asset<AccessibilitySettings>(DataFolder + "Accessibility/AccessibilitySettings.asset");
        // The settings asset raises this one itself, and its listeners reach it through the asset,
        // so it lives next to the asset and not with the events the scene's scripts hold.
        SetupTools.SetField(settings, "settingsChanged", SetupTools.Asset<GameEvent>(DataFolder + "Accessibility/SettingsChanged.asset"));
        var soundCue = SetupTools.Asset<SoundCueRaisedEvent>(EventsFolder + "SoundCueRaised.asset");
        var subtitleRequested = SetupTools.Asset<SubtitleRequestedEvent>(EventsFolder + "SubtitleRequested.asset");

        AccessibilityManager manager = holder.AddComponent<AccessibilityManager>();
        SetupTools.SetField(manager, "settings", settings);
        SetBool(manager, "dontDestroyOnLoad", false); // The settings are saved, so each load of the scene has its own manager.

        SetupTools.SetField(holder.AddComponent<VisualCueOverlay>(), "soundCueRaised", soundCue);
        SubtitleManager subtitles = holder.AddComponent<SubtitleManager>();
        SetupTools.SetField(subtitles, "subtitleRequested", subtitleRequested);

        ColorblindFilter shapes = holder.AddComponent<ColorblindFilter>();
        SetupTools.SetField(shapes, "hope", hope);
        SetupTools.SetField(shapes, "hopeEffects", TunnelAssets.HopeEffects);

        BodyCaptions captions = holder.AddComponent<BodyCaptions>();
        SetupTools.SetField(captions, "body", body);
        SetupTools.SetField(captions, "subtitleRequested", subtitleRequested);

        holder.AddComponent<HudFreeze>();

        // The picture: the mood volume gets the high contrast and the brightness floor.
        GameObject mood = GameObject.Find("Mood");
        SetupTools.GetOrAdd<HighContrastRenderer>(mood);
        BrightnessFloor floor = SetupTools.GetOrAdd<BrightnessFloor>(mood);
        SetupTools.SetField(floor, "tunnelLight", GameObject.Find("Global Light 2D").GetComponent<Light2D>());

        // The guard's steps are heard, so they get an arrow for players who can't hear them.
        foreach (HeardSteps steps in Object.FindObjectsByType<HeardSteps>(FindObjectsSortMode.None))
        {
            VisualCueForSound cue = SetupTools.GetOrAdd<VisualCueForSound>(steps.gameObject);
            SetupTools.SetField(cue, "soundCueRaised", soundCue);
            SetupTools.SetField(cue, "hero", player.transform);
            SetString(cue, "label", "steps");
            SetupTools.SetField(steps, "cue", cue);
        }
        return subtitles;
    }

    // ---------- Section 3: optional moments and missions ----------

    private static SlowMomentsDirector SetupOptional(GameObject holder, GameObject player, PlayerMovement movement, Tension tension)
    {
        OptionalRegistry registry = SetupTools.Asset<OptionalRegistry>(DataFolder + "Optional/OptionalRegistry.asset");
        if (registry.Entries.Count == 0)
        {
            // The starter list is a context-menu method of the asset, so it is called by name.
            typeof(OptionalRegistry).GetMethod("FillStarters", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(registry, null);
            EditorUtility.SetDirty(registry);
        }

        SetupTools.SetField(holder.AddComponent<OptionalContent>(), "registry", registry);
        OptionalMissions missions = holder.AddComponent<OptionalMissions>();

        RunStateTracker runState = holder.AddComponent<RunStateTracker>();
        SetupTools.SetField(runState, "missions", missions);
        SetupTools.SetField(runState, "memoryFound", TunnelAssets.MemoryFound);
        SetupTools.SetField(runState, "playerCaught", TunnelAssets.PlayerCaught);

        GameEvent guardClose = Event("GuardClose");
        GameEvent guardClear = Event("GuardClear");
        GameEvent beatStarted = Event("ScriptedBeatStarted");
        GameEvent beatEnded = Event("ScriptedBeatEnded");

        SlowMomentsDirector calm = holder.AddComponent<SlowMomentsDirector>();
        // The tutorial has no digging, so the director's two dig events stay empty.
        SetupTools.SetField(calm, "guardCloseEvent", guardClose);
        SetupTools.SetField(calm, "guardClearEvent", guardClear);
        SetupTools.SetField(calm, "scriptedBreakingPointEvent", beatStarted);
        SetupTools.SetField(calm, "scriptedBreakingPointEndedEvent", beatEnded);
        SetupTools.SetField(calm, "playerMovement", movement);

        CalmSignals signals = holder.AddComponent<CalmSignals>();
        SetupTools.SetField(signals, "tension", tension);
        SetupTools.SetField(signals, "settings", TunnelAssets.LevelSettings);
        SetupTools.SetField(signals, "playerCaught", TunnelAssets.PlayerCaught);
        SetupTools.SetField(signals, "guardClose", guardClose);
        SetupTools.SetField(signals, "guardClear", guardClear);
        SetupTools.SetField(signals, "scriptedBeatStarted", beatStarted);
        SetupTools.SetField(signals, "scriptedBeatEnded", beatEnded);

        // Observation: standing quietly by the cell door, when no guard is near.
        var door = new GameObject("Door Crack");
        door.transform.SetParent(holder.transform, false);
        door.transform.position = DoorCrackPosition;
        BoxCollider doorBox = door.AddComponent<BoxCollider>();
        doorBox.isTrigger = true;
        doorBox.size = DoorCrackSize;
        OptionalZone doorZone = door.AddComponent<OptionalZone>();
        SetString(doorZone, "entryId", "door-crack-look");
        SetupTools.SetField(doorZone, "calm", calm);
        SetupTools.SetField(doorZone, "thoughtRequested", TunnelAssets.HintRequested);
        SetString(doorZone, "thought", "A line of light under the door. I watch it for a while.\nIt doesn't move. Out there, it is still day.");

        // Expression: adding a line of his own to the counting wall.
        var wall = new GameObject("Counting Wall");
        wall.transform.SetParent(holder.transform, false);
        wall.transform.position = CountingWallPosition;
        OptionalAction wallAction = wall.AddComponent<OptionalAction>();
        SetString(wallAction, "entryId", OptionalMissions.CountWallMarkEntryId);
        SetFloat(wallAction, "cancelRadius", CountingWallRadius);
        SetupTools.SetField(wallAction, "player", player.transform);
        SetupTools.SetField(wallAction, "thoughtRequested", TunnelAssets.HintRequested);
        SetString(wallAction, "hint", "I could scratch a line of my own here. One day.\n(Stand here and hold E)");
        SetString(wallAction, "completedThought", "One line. Mine.\nSomeone will count it after me.");
        return calm;
    }

    // ---------- Section 3: the ending ----------

    private static void SetupEnding(GameObject holder, ContentIntensity intensity, SubtitleManager subtitles)
    {
        string sequencePath = DataFolder + "Ending/TutorialEnding.asset";
        EndingSequence sequence = AssetDatabase.LoadAssetAtPath<EndingSequence>(sequencePath);
        if (sequence == null)
        {
            System.IO.Directory.CreateDirectory(DataFolder + "Ending");
            sequence = ScriptableObject.CreateInstance<EndingSequence>();
            // The full game's ending (the stars, the long count) belongs to the last stage.
            // The tutorial ends its one day with two short beats; the bonus lines join the last.
            sequence.UseBeats(new[]
            {
                new EndingBeat("FirstNight", FirstNightSeconds, "לילה ראשון.", "The first night."),
                new EndingBeat(EndingSequence.BeatNames.NarratorFinale, FinaleSeconds,
                    "הוא ספר יום אחד. מחר יספור עוד אחד.", "He counted one day. Tomorrow he would count another."),
            });
            AssetDatabase.CreateAsset(sequence, sequencePath);
        }

        string libraryPath = DataFolder + "Ending/BonusNarration.asset";
        BonusNarrationLibrary library = AssetDatabase.LoadAssetAtPath<BonusNarrationLibrary>(libraryPath);
        if (library == null)
        {
            library = BonusNarrationLibrary.CreateDefault();
            library.Add(new BonusNarrationLibrary.Entry("stage1." + BonusNarrationLibrary.Conditions.CountWallMark,
                "על הקיר יש עכשיו קו אחד שהוא שלו.", "On the wall there is now one line that is his."));
            library.Add(new BonusNarrationLibrary.Entry("stage1." + BonusNarrationLibrary.Conditions.AllMemories,
                "הוא זכר את כולם.", "He remembered every one of them."));
            AssetDatabase.CreateAsset(library, libraryPath);
        }

        EndingDirector director = holder.AddComponent<EndingDirector>();
        SetupTools.SetField(director, "endingTriggered", TunnelAssets.LevelWon);
        SetupTools.SetField(director, "sequence", sequence);
        SetupTools.SetField(director, "bonusLibrary", library);
        SetupTools.SetField(director, "intensity", intensity);
        var fields = new SerializedObject(director);
        fields.FindProperty("language").enumValueIndex = EnglishLanguageIndex; // The game's fonts and texts are English.
        fields.ApplyModifiedPropertiesWithoutUndo();

        EndingPresenter presenter = holder.AddComponent<EndingPresenter>();
        SetupTools.SetField(presenter, "director", director);
        SetupTools.SetField(presenter, "subtitles", subtitles);
    }

    // ---------- Section 3: content intensity, and the start of the game ----------

    private static ContentIntensity Intensity()
    {
        string path = DataFolder + "Intensity/ContentIntensity.asset";
        bool isNew = !System.IO.File.Exists(path);
        ContentIntensity intensity = SetupTools.Asset<ContentIntensity>(path);
        SetupTools.SetField(intensity, "modeChanged", SetupTools.Asset<GameEvent>(DataFolder + "Intensity/ModeChanged.asset")); // Like the settings' own event.
        if (isNew)
        {
            // The tutorial's opening is the hero waking up, with nothing hard to see, so it plays unless the player skips it.
            SetBool(intensity, "skipOpening", false);
        }
        return intensity;
    }

    private static ContentIntensityManager SetupStartMenu(GameObject holder, ContentIntensity intensity, DifficultyManager difficulty)
    {
        GameEvent warningAccepted = Event("WarningAccepted");
        GameEvent warningDeclined = Event("WarningDeclined");
        GameEvent startChosen = Event("StartChosen");

        ContentIntensityManager manager = holder.AddComponent<ContentIntensityManager>();
        SetupTools.SetField(manager, "intensity", intensity);

        // On its own object, so the start menu can turn it off after the player accepts.
        var warningObject = new GameObject("Content Warning");
        warningObject.transform.SetParent(holder.transform, false);
        ContentWarningScreen warning = warningObject.AddComponent<ContentWarningScreen>();
        SetupTools.SetField(warning, "text", SetupTools.Asset<ContentWarningText>(DataFolder + "Intensity/ContentWarningText.asset"));
        SetupTools.SetField(warning, "intensity", intensity);
        SetupTools.SetField(warning, "warningAccepted", warningAccepted);
        SetupTools.SetField(warning, "warningDeclined", warningDeclined);

        StartMenu menu = holder.AddComponent<StartMenu>();
        SetupTools.SetField(menu, "warning", warning);
        SetupTools.SetField(menu, "intensityManager", manager);
        SetupTools.SetField(menu, "difficulty", difficulty);
        SetupTools.SetField(menu, "warningAccepted", warningAccepted);
        SetupTools.SetField(menu, "warningDeclined", warningDeclined);
        SetupTools.SetField(menu, "startChosen", startChosen);

        OpeningSkipRouter router = holder.AddComponent<OpeningSkipRouter>();
        SetupTools.SetField(router, "intensity", intensity);
        GameEvent skipOpening = Event("SkipOpening");
        SetupTools.SetField(router, "skipToTutorialStartEvent", skipOpening);

        OpeningGate gate = holder.AddComponent<OpeningGate>();
        SetupTools.SetField(gate, "titleCard", Object.FindAnyObjectByType<TitleCard>());
        SetupTools.SetField(gate, "skipRouter", router);
        SetupTools.SetField(gate, "startChosen", startChosen);
        SetupTools.SetField(gate, "skipOpening", skipOpening);
        SetupTools.SetField(gate, "openingFinished", TunnelAssets.OpeningFinished);
        return manager;
    }

    // ---------- Helpers ----------

    private static GameObject Child(GameObject parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        return child;
    }

    private static GameEvent Event(string name)
    {
        return SetupTools.Asset<GameEvent>(EventsFolder + name + ".asset");
    }

    private static void SetFloat(Object target, string field, float value)
    {
        var fields = new SerializedObject(target);
        fields.FindProperty(field).floatValue = value;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Object target, string field, bool value)
    {
        var fields = new SerializedObject(target);
        fields.FindProperty(field).boolValue = value;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        var fields = new SerializedObject(target);
        fields.FindProperty(field).stringValue = value;
        fields.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ImportNewSprites()
    {
        foreach (string name in NewSprites)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(UiFolder + name + ".png");
            if (importer == null || importer.textureType == TextureImporterType.Sprite)
            {
                continue;
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
