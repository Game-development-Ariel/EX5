using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The conductor of the end-of-game sequence. When endingTriggered is raised, it walks the
/// EndingSequence beat by beat, applies the variants that the optional missions earned, and
/// respects the content intensity (Implied skips the chair-week beat, Direct plays it).
/// From design 3.3: the component reads OptionalMissions and ContentIntensity and composes
/// the right ending; the player never sees a checklist.
/// From an early draft: the long arc from stars to release and the narrator finale (the design doc, topic 7, is now the source).
/// </summary>
public class EndingDirector : MonoBehaviour
{
    [Header("Listens to")]
    [Tooltip("Raised by the end of stage 4 to start the ending")]
    [SerializeField] private GameEvent endingTriggered;

    [Header("What plays")]
    [Tooltip("The ordered beats of the ending")]
    [SerializeField] private EndingSequence sequence;

    [Tooltip("The extra half-lines unlocked by optional conditions")]
    [SerializeField] private BonusNarrationLibrary bonusLibrary;

    [Header("Variants")]
    [Tooltip("Fires when AllMemories is completed. The stars prefab listens and draws memory-shaped constellations")]
    [SerializeField] private UnityEvent renderConstellations = new UnityEvent();

    [Tooltip("Fires when SandbagWrite is completed. The news film prefab listens and includes the sandbag")]
    [SerializeField] private UnityEvent renderSandbagInNews = new UnityEvent();

    [Tooltip("Fires when AllPreviousNotes is completed. The journal prefab listens and swaps to the thick version")]
    [SerializeField] private UnityEvent renderThickJournal = new UnityEvent();

    [Tooltip("Fires when BraceletFound is completed. The release prefab listens and adds the bracelet to the wrist")]
    [SerializeField] private UnityEvent renderBraceletOnWrist = new UnityEvent();

    [Tooltip("Fires each time a beat is played, with the beat's name. Prefabs and tests can listen")]
    [SerializeField] private BeatEnteredEvent beatEntered = new BeatEnteredEvent();

    [Tooltip("Fires each time a subtitle is shown, with the picked-language text. Tests and the subtitle UI listen")]
    [SerializeField] private SubtitleEvent subtitleShown = new SubtitleEvent();

    [Tooltip("Fires each time a bonus narration line is picked, with the picked-language text")]
    [SerializeField] private SubtitleEvent bonusNarrationPlayed = new SubtitleEvent();

    [Header("Context")]
    [Tooltip("The project's content intensity. Direct plays the chair-week beat; Implied skips it")]
    [SerializeField] private ContentIntensity intensity;

    [Tooltip("The language used for subtitles. Hebrew by default")]
    [SerializeField] private EndingLanguage language = EndingLanguage.Hebrew;

    [Tooltip("The current stage id, used to look up stage-conditioned bonus lines. '4' by convention for the finale")]
    [SerializeField] private string stageId = "stage1";

    private OptionalMissions missions;

    /// <summary>Raised with a beat's subtitle when the beat starts. The presenter shows it.</summary>
    public SubtitleEvent SubtitleShown => subtitleShown;

    /// <summary>Raised with each bonus line the completed missions earned, in the narrator's finale.</summary>
    public SubtitleEvent BonusNarrationPlayed => bonusNarrationPlayed;

    /// <summary>The missions this director reads. Falls back to OptionalMissions.Instance.</summary>
    public OptionalMissions Missions
    {
        get => missions != null ? missions : OptionalMissions.Instance;
        set => missions = value;
    }

    private void OnEnable()
    {
        if (endingTriggered != null)
        {
            endingTriggered.Subscribe(StartEnding);
        }
    }

    private void OnDisable()
    {
        if (endingTriggered != null)
        {
            endingTriggered.Unsubscribe(StartEnding);
        }
    }

    private void StartEnding()
    {
        if (isActiveAndEnabled)
        {
            StartCoroutine(Play());
        }
    }

    /// <summary>
    /// Plays the current sequence from top to bottom. Each beat spawns its prefab, fires its
    /// onEnter hook, plays audio, shows a subtitle in the picked language, and then waits for
    /// the beat's duration before moving on. Variants are applied before the beat starts, so
    /// a prefab's Awake can already see them.
    /// </summary>
    public IEnumerator Play()
    {
        if (sequence == null)
        {
            yield break;
        }

        ApplyVariants();

        for (int i = 0; i < sequence.Count; i++)
        {
            EndingBeat beat = sequence.At(i);
            if (beat == null)
            {
                continue;
            }
            if (ShouldSkip(beat))
            {
                continue;
            }

            EnterBeat(beat);
            PlayBonusFor(beat);

            yield return new WaitForSeconds(beat.Duration);
        }
    }

    /// <summary>
    /// Plays a single beat and all its hooks. Public so tests can step through the sequence
    /// one beat at a time without a coroutine.
    /// </summary>
    public void EnterBeat(EndingBeat beat)
    {
        if (beat == null)
        {
            return;
        }
        beat.OnEnter?.Invoke();
        beatEntered.Invoke(beat.Name);

        string subtitle = beat.Subtitle(language);
        if (!string.IsNullOrEmpty(subtitle))
        {
            subtitleShown.Invoke(subtitle);
        }
    }

    /// <summary>
    /// True when the beat should be skipped at the current intensity. The chair-week beat is
    /// only played in Direct; everything else always plays.
    /// </summary>
    public bool ShouldSkip(EndingBeat beat)
    {
        if (beat == null)
        {
            return true;
        }
        if (beat.Name == EndingSequence.BeatNames.ChairWeek)
        {
            IntensityLevel mode = intensity != null ? intensity.Mode : IntensityLevel.Implied;
            return mode != IntensityLevel.Direct;
        }
        return false;
    }

    /// <summary>
    /// Fires the per-mission variant hooks based on which optional missions are completed.
    /// Called once at the start of Play, so every beat's prefab can already read the flags.
    /// </summary>
    public void ApplyVariants()
    {
        OptionalMissions source = Missions;
        if (source == null)
        {
            return;
        }
        foreach (OptionalMission mission in source.CompletedMissions())
        {
            switch (mission.Kind)
            {
                case MissionKind.AllMemories:
                    renderConstellations.Invoke();
                    break;
                case MissionKind.SandbagWrite:
                    renderSandbagInNews.Invoke();
                    break;
                case MissionKind.AllPreviousNotes:
                    renderThickJournal.Invoke();
                    break;
                case MissionKind.BraceletFound:
                    renderBraceletOnWrist.Invoke();
                    break;
                case MissionKind.NoCapturesDay:
                case MissionKind.CountWallMark:
                    // These two only affect narrator text, picked up by PlayBonusFor.
                    break;
            }
        }
    }

    /// <summary>
    /// The beat-conditioned bonus narration. The narrator finale is the beat that collects
    /// the extra half-lines; a no-captures day in stage 1 adds "and in the evening he slept well".
    /// </summary>
    private void PlayBonusFor(EndingBeat beat)
    {
        if (bonusLibrary == null || Missions == null)
        {
            return;
        }
        if (beat.Name != EndingSequence.BeatNames.NarratorFinale)
        {
            return;
        }
        foreach (string condition in ConditionsFor(Missions))
        {
            if (!bonusLibrary.Has(stageId, condition))
            {
                continue;
            }
            string text = bonusLibrary.Lookup(stageId, condition, language);
            if (!string.IsNullOrEmpty(text))
            {
                bonusNarrationPlayed.Invoke(text);
            }
        }
    }

    private static IEnumerable<string> ConditionsFor(OptionalMissions source)
    {
        foreach (OptionalMission mission in source.CompletedMissions())
        {
            switch (mission.Kind)
            {
                case MissionKind.NoCapturesDay:
                    yield return BonusNarrationLibrary.Conditions.NoCaptures;
                    break;
                case MissionKind.AllMemories:
                    yield return BonusNarrationLibrary.Conditions.AllMemories;
                    break;
                case MissionKind.AllPreviousNotes:
                    yield return BonusNarrationLibrary.Conditions.AllPreviousNotes;
                    break;
                case MissionKind.SandbagWrite:
                    yield return BonusNarrationLibrary.Conditions.SandbagWrite;
                    break;
                case MissionKind.BraceletFound:
                    yield return BonusNarrationLibrary.Conditions.BraceletFound;
                    break;
                case MissionKind.CountWallMark:
                    yield return BonusNarrationLibrary.Conditions.CountWallMark;
                    break;
            }
        }
    }

    /// <summary>The event that carries the beat's name to listeners. UnityEvent subclass for the inspector.</summary>
    [Serializable]
    public class BeatEnteredEvent : UnityEvent<string>
    {
    }

    /// <summary>The event that carries the picked-language subtitle text to listeners.</summary>
    [Serializable]
    public class SubtitleEvent : UnityEvent<string>
    {
    }
}
