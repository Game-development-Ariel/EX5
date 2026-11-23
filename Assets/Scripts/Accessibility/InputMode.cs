using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The place every "hold a key" action asks whether it should accept a tap-tap-tap instead. The
/// design (section 2.2) says important actions are holds so the time in the world equals the time
/// the player spends. For players with motor limits, the same meter fills from repeated taps at
/// the same rate. Each tap counts for 0.3s of hold, and the taps must come within TapGapSeconds
/// of each other, so the player cannot store taps for a different action later.
///
/// Systems call IsHoldFulfilled each frame they hold the key; they also call RegisterTap once per
/// keydown. In hold mode RegisterTap is simply ignored and the usual hold time applies. The class
/// is static because it is a shim, not a system — no scene state, nothing to destroy.
/// </summary>
public static class InputMode
{
    /// <summary>How much of a "hold" one tap earns in tap mode.</summary>
    public const float TapWorthSeconds = 0.3f;

    /// <summary>If no new tap comes within this many seconds, the tap run resets.</summary>
    public const float TapGapSeconds = 0.6f;

    private struct TapRun
    {
        public float LastTapTime;
        public float Accumulated;
    }

    private static readonly Dictionary<string, TapRun> taps = new Dictionary<string, TapRun>();

    /// <summary>
    /// Call every frame while the hold key is down. In hold mode returns true when heldSeconds
    /// reaches requiredSeconds. In tap mode returns true when enough taps have come in; the
    /// heldSeconds is ignored.
    /// </summary>
    public static bool IsHoldFulfilled(string actionId, float requiredSeconds)
    {
        return IsHoldFulfilled(actionId, requiredSeconds, 0f);
    }

    /// <summary>Same but the caller passes the current hold time for hold mode.</summary>
    public static bool IsHoldFulfilled(string actionId, float requiredSeconds, float heldSeconds)
    {
        if (!AccessibilitySettings.Current.TapInsteadOfHold)
        {
            return heldSeconds >= requiredSeconds;
        }
        if (taps.TryGetValue(actionId, out TapRun run))
        {
            // Stale runs count as zero — the run reset.
            if (Time.unscaledTime - run.LastTapTime > TapGapSeconds)
            {
                return false;
            }
            return run.Accumulated >= requiredSeconds;
        }
        return false;
    }

    /// <summary>Call once per key-down for this action. In hold mode it is a no-op.</summary>
    public static void RegisterTap(string actionId)
    {
        if (!AccessibilitySettings.Current.TapInsteadOfHold)
        {
            return;
        }
        TapRun run;
        if (taps.TryGetValue(actionId, out run) && Time.unscaledTime - run.LastTapTime <= TapGapSeconds)
        {
            run.Accumulated += TapWorthSeconds;
        }
        else
        {
            run = new TapRun { Accumulated = TapWorthSeconds };
        }
        run.LastTapTime = Time.unscaledTime;
        taps[actionId] = run;
    }

    /// <summary>Clears the tap run after the action has fired, so the next invocation starts clean.</summary>
    public static void Reset(string actionId)
    {
        taps.Remove(actionId);
    }

    /// <summary>Clears everything. Tests call this between cases; the game does not need to.</summary>
    public static void ResetAll()
    {
        taps.Clear();
    }

    /// <summary>How many taps have been counted for this action right now. Useful for a progress ring UI.</summary>
    public static int TapCountFor(string actionId)
    {
        if (!taps.TryGetValue(actionId, out TapRun run))
        {
            return 0;
        }
        if (Time.unscaledTime - run.LastTapTime > TapGapSeconds)
        {
            return 0;
        }
        return Mathf.RoundToInt(run.Accumulated / TapWorthSeconds);
    }

    /// <summary>The accumulated hold time for tap mode, in seconds. Zero in hold mode.</summary>
    public static float AccumulatedSecondsFor(string actionId)
    {
        if (!AccessibilitySettings.Current.TapInsteadOfHold)
        {
            return 0f;
        }
        if (!taps.TryGetValue(actionId, out TapRun run))
        {
            return 0f;
        }
        if (Time.unscaledTime - run.LastTapTime > TapGapSeconds)
        {
            return 0f;
        }
        return run.Accumulated;
    }
}
