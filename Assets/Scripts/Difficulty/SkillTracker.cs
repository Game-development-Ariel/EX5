using System;
using UnityEngine;

/// <summary>
/// Reads the player's actions as a running skill score, clamped to the range in section 1.3 of the design document.
/// The score begins at zero on every stage and feeds the DifficultyModulator. The tracker holds no feedback of its own:
/// the player never sees the score, so every change here is silent by design.
/// </summary>
public class SkillTracker : MonoBehaviour
{
    // Rule constants from section 1.3 of the design document.
    // The design treats the score as a nine-signal sum in [-10..+10].
    private const float ScoreMin = -10f;
    private const float ScoreMax = 10f;

    // Captures cost two points. The clearest single signal that the moment is too hard.
    private const float CaughtDelta = -2f;

    // 30 seconds of continuous idle (not digging, praying or listening) costs one point.
    private const float IdleThresholdSeconds = 30f;
    private const float IdleDelta = -1f;

    // A dig session of 2.5+ meters without being caught rewards one point.
    private const float DigLongMeters = 2.5f;
    private const float DigLongDelta = 1f;

    // A dig session shorter than 0.5 meters (and not cut off by a capture) is fear and loses half a point.
    private const float DigShortMeters = 0.5f;
    private const float DigShortDelta = -0.5f;

    // Each listen activation is worth one point, capped at +3 points per stage.
    private const float ListenDelta = 1f;
    private const float ListenCapPerStage = 3f;

    // Zero listens through an entire stage: the player missed one of the three core systems.
    private const float NoListenAllStageDelta = -2f;

    // Keeping hope above 60 and never using a memory is worth one point at the end of the stage.
    private const int HopeKeptAboveMin = 60;
    private const float HopeKeptDelta = 1f;

    // Hope below 25 at the end of the stage costs one point.
    private const int HopeEndBelow = 25;
    private const float HopeEndLowDelta = -1f;

    // Standing in a quiet place for more than 10 seconds without starting a prayer is -0.5.
    // The signal fires at most once per quiet-place visit, so it doesn't drain the score on long idles.
    private const float QuietPlaceGraceSeconds = 10f;
    private const float QuietPlaceNoPrayerDelta = -0.5f;

    [Header("Signals listened to")]
    [SerializeField] private CaughtEvent caught;
    [SerializeField] private PlayerIdleSignal playerIdle;
    [SerializeField] private DigSessionEndedEvent digSessionEnded;
    [SerializeField] private ListenActivatedEvent listenActivated;
    [SerializeField] private HopeChangedEvent hopeChanged;
    [SerializeField] private QuietPlaceEnteredEvent quietPlaceEntered;
    [SerializeField] private QuietPlaceExitedEvent quietPlaceExited;
    [SerializeField] private StageStartedEvent stageStarted;
    [SerializeField] private StageEndedEvent stageEnded;

    private float score;
    private int listensThisStage;
    private bool everListenedThisStage;
    private bool hopeDroppedBelowKeptMin;
    private bool everUsedMemoryThisStage;
    private int lastHope;
    private bool hopeTracked;
    private float quietPlaceEnteredAt;
    private bool insideQuietPlace;
    private bool prayedDuringCurrentVisit;
    private bool quietPenaltyGivenForVisit;
    private bool idlePenaltyGivenForCurrentIdle;

    /// <summary>The current skill score, clamped to the design's range.</summary>
    public float CurrentScore => score;

    /// <summary>How many times the player listened during this stage. Exposed for the modulator and tests.</summary>
    public int ListensThisStage => listensThisStage;

    /// <summary>Fires every time the score changes, with the new value.</summary>
    public event Action<float> ScoreChanged;

    private void OnEnable()
    {
        if (caught != null) caught.Subscribe(OnCaught);
        if (playerIdle != null) playerIdle.Subscribe(OnPlayerIdle);
        if (digSessionEnded != null) digSessionEnded.Subscribe(OnDigSessionEnded);
        if (listenActivated != null) listenActivated.Subscribe(OnListenActivated);
        if (hopeChanged != null) hopeChanged.Subscribe(OnHopeChanged);
        if (quietPlaceEntered != null) quietPlaceEntered.Subscribe(OnQuietPlaceEntered);
        if (quietPlaceExited != null) quietPlaceExited.Subscribe(OnQuietPlaceExited);
        if (stageStarted != null) stageStarted.Subscribe(OnStageStarted);
        if (stageEnded != null) stageEnded.Subscribe(OnStageEnded);
    }

    private void OnDisable()
    {
        if (caught != null) caught.Unsubscribe(OnCaught);
        if (playerIdle != null) playerIdle.Unsubscribe(OnPlayerIdle);
        if (digSessionEnded != null) digSessionEnded.Unsubscribe(OnDigSessionEnded);
        if (listenActivated != null) listenActivated.Unsubscribe(OnListenActivated);
        if (hopeChanged != null) hopeChanged.Unsubscribe(OnHopeChanged);
        if (quietPlaceEntered != null) quietPlaceEntered.Unsubscribe(OnQuietPlaceEntered);
        if (quietPlaceExited != null) quietPlaceExited.Unsubscribe(OnQuietPlaceExited);
        if (stageStarted != null) stageStarted.Unsubscribe(OnStageStarted);
        if (stageEnded != null) stageEnded.Unsubscribe(OnStageEnded);
    }

    private void Update()
    {
        // When the player is standing in a quiet place beyond the grace window and never started a prayer,
        // the penalty fires once per visit.
        if (insideQuietPlace
            && !prayedDuringCurrentVisit
            && !quietPenaltyGivenForVisit
            && Time.time - quietPlaceEnteredAt > QuietPlaceGraceSeconds)
        {
            ApplyDelta(QuietPlaceNoPrayerDelta);
            quietPenaltyGivenForVisit = true;
        }
    }

    /// <summary>Call when a prayer begins in the quiet place, so the "no prayer" penalty doesn't fire.</summary>
    public void NotePrayerStarted()
    {
        prayedDuringCurrentVisit = true;
    }

    /// <summary>Call when the player picks up a memory, so the "hope kept without memory" bonus doesn't fire.</summary>
    public void NoteMemoryUsed()
    {
        everUsedMemoryThisStage = true;
    }

    private void OnStageStarted()
    {
        score = 0f;
        listensThisStage = 0;
        everListenedThisStage = false;
        hopeDroppedBelowKeptMin = false;
        everUsedMemoryThisStage = false;
        hopeTracked = false;
        insideQuietPlace = false;
        prayedDuringCurrentVisit = false;
        quietPenaltyGivenForVisit = false;
        idlePenaltyGivenForCurrentIdle = false;
        ScoreChanged?.Invoke(score);
    }

    private void OnStageEnded()
    {
        if (!everListenedThisStage)
        {
            ApplyDelta(NoListenAllStageDelta);
        }
        if (!hopeDroppedBelowKeptMin && !everUsedMemoryThisStage && hopeTracked)
        {
            ApplyDelta(HopeKeptDelta);
        }
        if (hopeTracked && lastHope < HopeEndBelow)
        {
            ApplyDelta(HopeEndLowDelta);
        }
    }

    private void OnCaught()
    {
        ApplyDelta(CaughtDelta);
    }

    private void OnPlayerIdle(float idleSeconds)
    {
        if (idleSeconds >= IdleThresholdSeconds && !idlePenaltyGivenForCurrentIdle)
        {
            ApplyDelta(IdleDelta);
            idlePenaltyGivenForCurrentIdle = true;
        }
        else if (idleSeconds < IdleThresholdSeconds)
        {
            // Movement reset: the next long idle should penalize again.
            idlePenaltyGivenForCurrentIdle = false;
        }
    }

    private void OnDigSessionEnded(float meters, bool endedByCatch)
    {
        if (endedByCatch)
        {
            return;
        }
        if (meters >= DigLongMeters)
        {
            ApplyDelta(DigLongDelta);
        }
        else if (meters < DigShortMeters)
        {
            ApplyDelta(DigShortDelta);
        }
    }

    private void OnListenActivated()
    {
        everListenedThisStage = true;
        if (listensThisStage * ListenDelta < ListenCapPerStage)
        {
            ApplyDelta(ListenDelta);
        }
        listensThisStage++;
    }

    private void OnHopeChanged(int oldValue, int newValue)
    {
        hopeTracked = true;
        lastHope = newValue;
        if (newValue <= HopeKeptAboveMin)
        {
            hopeDroppedBelowKeptMin = true;
        }
    }

    private void OnQuietPlaceEntered()
    {
        insideQuietPlace = true;
        prayedDuringCurrentVisit = false;
        quietPenaltyGivenForVisit = false;
        quietPlaceEnteredAt = Time.time;
    }

    private void OnQuietPlaceExited()
    {
        insideQuietPlace = false;
    }

    private void ApplyDelta(float delta)
    {
        float previous = score;
        score = Mathf.Clamp(score + delta, ScoreMin, ScoreMax);
        if (!Mathf.Approximately(previous, score))
        {
            ScoreChanged?.Invoke(score);
        }
    }
}
