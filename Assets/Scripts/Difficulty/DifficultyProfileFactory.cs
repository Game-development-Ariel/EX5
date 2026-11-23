using UnityEngine;

/// <summary>
/// Builds a DifficultyProfile instance for each preset, with the numbers from the design document's section 1.2.
/// The game ships three assets built from this factory. Tests call the same factory, so a drift between
/// code and spec surfaces in the DifficultyProfileFactoryTests.
/// </summary>
public static class DifficultyProfileFactory
{
    // Hope does not drop with time on any level (design, topic 4): it changes only from what happens.
    // The decay stays in the profile as a number Live Ops can turn on for a test, without a new build.
    private const float NoHopeDecay = 0f;

    // Easy ("Day One"): wider windows, more hope.
    // Seconds between random cell checks: 70..90 (double the default on the low end).
    private const float EasyGuardCheckMin = 70f;
    private const float EasyGuardCheckMax = 90f;
    // One patrol loop runs 20 seconds on Easy.
    private const float EasyPatrolCycle = 20f;
    // One guard in stage 2, so the second cycle field is left equal to the first and unused.
    private const int EasyStage2GuardCount = 1;
    // Narrower camera cone (45 degrees) leaves more hiding space.
    private const float EasyCameraCone = 45f;
    // Three cameras per stage instead of five.
    private const int EasyCameraCount = 3;
    // Start with 70 hope and lose only 5 on a capture.
    private const int EasyStartHope = 70;
    private const int EasyHopeLostCaught = 5;
    // Memory gives the default 15 hope on Easy.
    private const int EasyMemoryValue = 15;
    // Faster dig: 0.20 m/s. Noise reaches only the adjacent corridor.
    private const float EasyDigSpeed = 0.20f;
    private const int EasyDigNoiseCorridors = 1;
    // Flashlight battery lasts 90 seconds on Easy.
    private const float EasyBattery = 90f;
    // Journal shows a concrete hint after two captures.
    private const int EasyJournalHintCaptures = 2;

    // Medium (Default): the tuned baseline. Section 1.2 names Medium as the balance target.
    private const float MediumGuardCheckMin = 45f;
    private const float MediumGuardCheckMax = 60f;
    private const float MediumPatrolCycle = 14f;
    private const int MediumStage2GuardCount = 2;
    private const float MediumCameraCone = 60f;
    private const int MediumCameraCount = 5;
    private const int MediumStartHope = 50;
    private const int MediumHopeLostCaught = 10;
    private const int MediumMemoryValue = 15;
    // 0.15 m/s dig, noise heard in two adjacent corridors.
    private const float MediumDigSpeed = 0.15f;
    private const int MediumDigNoiseCorridors = 2;
    // Flashlight battery lasts 65 seconds on Medium.
    private const float MediumBattery = 65f;

    // Hard ("I Lost The Festival"): every axis tightens.
    private const float HardGuardCheckMin = 30f;
    private const float HardGuardCheckMax = 45f;
    // Two guards in stage 2 with un-synced loops: 14 and 17 seconds.
    private const float HardPatrolCycle = 14f;
    private const float HardSecondPatrolCycle = 17f;
    private const int HardStage2GuardCount = 2;
    // Random listen pause: 0.5 sec stop, once every 20..30 seconds, always fires.
    private const float HardListenPauseProbability = 1f;
    private const float HardListenPauseDuration = 0.5f;
    private const float HardListenPauseIntervalMin = 20f;
    private const float HardListenPauseIntervalMax = 30f;
    // Wider cone (75 degrees), seven cameras, one of which rotates every 8 seconds.
    private const float HardCameraCone = 75f;
    private const int HardCameraCount = 7;
    private const float HardRotatingPeriod = 8f;
    // Start with 30 hope, lose 20 on capture, memory gives only 10.
    private const int HardStartHope = 30;
    private const int HardHopeLostCaught = 20;
    private const int HardMemoryValue = 10;
    // Slower dig: 0.11 m/s. 20-second continuous-dig threshold brings the guard early.
    private const float HardDigSpeed = 0.11f;
    private const int HardDigNoiseCorridors = 2;
    private const float HardContinuousDigThreshold = 20f;
    // Flashlight battery lasts 45 seconds on Hard.
    private const float HardBattery = 45f;

    // Shared "off" defaults for flags not on for the given preset.
    // Used for Easy and Medium journal-hint capture count and the second patrol cycle on single-guard stages.
    private const int DefaultHintCapturesWhenOff = 2;

    /// <summary>Easy: forgiving numbers, journal hint after two captures.</summary>
    public static DifficultyProfile CreateEasy()
    {
        DifficultyProfile profile = ScriptableObject.CreateInstance<DifficultyProfile>();
        profile.name = "Profile_Easy";
        profile.SetFields(
            preset: Difficulty.Easy,
            guardCheckMin: EasyGuardCheckMin, guardCheckMax: EasyGuardCheckMax,
            patrolCycle: EasyPatrolCycle, secondPatrolCycle: EasyPatrolCycle, guardsInStage2: EasyStage2GuardCount,
            listenPauseOn: false, listenPauseChance: HardListenPauseProbability,
            listenPauseDuration: HardListenPauseDuration,
            listenPauseIntervalMin: HardListenPauseIntervalMin, listenPauseIntervalMax: HardListenPauseIntervalMax,
            cameraCone: EasyCameraCone, cameras: EasyCameraCount, rotatingOn: false, rotatingPeriod: HardRotatingPeriod,
            startHope: EasyStartHope, hopeLostCaught: EasyHopeLostCaught,
            hopeDecayPerMinute: NoHopeDecay, memoryValue: EasyMemoryValue,
            digSpeed: EasyDigSpeed, digNoiseRooms: EasyDigNoiseCorridors,
            continuousDigOn: false, continuousDigSeconds: HardContinuousDigThreshold,
            battery: EasyBattery,
            journalHintsOn: true, hintCaptures: EasyJournalHintCaptures);
        return profile;
    }

    /// <summary>Medium: the balance target. No modulators change these numbers directly, only layered multipliers.</summary>
    public static DifficultyProfile CreateMedium()
    {
        DifficultyProfile profile = ScriptableObject.CreateInstance<DifficultyProfile>();
        profile.name = "Profile_Medium";
        profile.SetFields(
            preset: Difficulty.Medium,
            guardCheckMin: MediumGuardCheckMin, guardCheckMax: MediumGuardCheckMax,
            patrolCycle: MediumPatrolCycle, secondPatrolCycle: MediumPatrolCycle, guardsInStage2: MediumStage2GuardCount,
            listenPauseOn: false, listenPauseChance: HardListenPauseProbability,
            listenPauseDuration: HardListenPauseDuration,
            listenPauseIntervalMin: HardListenPauseIntervalMin, listenPauseIntervalMax: HardListenPauseIntervalMax,
            cameraCone: MediumCameraCone, cameras: MediumCameraCount, rotatingOn: false, rotatingPeriod: HardRotatingPeriod,
            startHope: MediumStartHope, hopeLostCaught: MediumHopeLostCaught,
            hopeDecayPerMinute: NoHopeDecay, memoryValue: MediumMemoryValue,
            digSpeed: MediumDigSpeed, digNoiseRooms: MediumDigNoiseCorridors,
            continuousDigOn: false, continuousDigSeconds: HardContinuousDigThreshold,
            battery: MediumBattery,
            journalHintsOn: false, hintCaptures: DefaultHintCapturesWhenOff);
        return profile;
    }

    /// <summary>Hard: every axis works against the player, no journal hints.</summary>
    public static DifficultyProfile CreateHard()
    {
        DifficultyProfile profile = ScriptableObject.CreateInstance<DifficultyProfile>();
        profile.name = "Profile_Hard";
        profile.SetFields(
            preset: Difficulty.Hard,
            guardCheckMin: HardGuardCheckMin, guardCheckMax: HardGuardCheckMax,
            patrolCycle: HardPatrolCycle, secondPatrolCycle: HardSecondPatrolCycle,
            guardsInStage2: HardStage2GuardCount,
            listenPauseOn: true, listenPauseChance: HardListenPauseProbability,
            listenPauseDuration: HardListenPauseDuration,
            listenPauseIntervalMin: HardListenPauseIntervalMin,
            listenPauseIntervalMax: HardListenPauseIntervalMax,
            cameraCone: HardCameraCone, cameras: HardCameraCount,
            rotatingOn: true, rotatingPeriod: HardRotatingPeriod,
            startHope: HardStartHope, hopeLostCaught: HardHopeLostCaught,
            hopeDecayPerMinute: NoHopeDecay, memoryValue: HardMemoryValue,
            digSpeed: HardDigSpeed, digNoiseRooms: HardDigNoiseCorridors,
            continuousDigOn: true, continuousDigSeconds: HardContinuousDigThreshold,
            battery: HardBattery,
            journalHintsOn: false, hintCaptures: DefaultHintCapturesWhenOff);
        return profile;
    }

    /// <summary>Builds the preset by enum, for the menu selection.</summary>
    public static DifficultyProfile Create(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return CreateEasy();
            case Difficulty.Hard: return CreateHard();
            default: return CreateMedium();
        }
    }
}
