using System.Collections.Generic;

/// <summary>
/// The screen rules from design 2.1: at most two to four small lights at a time. Everything else is dark.
/// The director asks the policy to cut down a chosen set to the allowed count, so a level that marked
/// too many targets to light never shines more than the budget.
/// </summary>
public static class AttentionPolicy
{
    /// <summary>The maximum bright targets the screen may show at once, from design 2.1.</summary>
    public const int MaxBrightTargets = 4;

    /// <summary>
    /// Returns at most maxActive targets from the given set, in the order they came in. The rest are left out.
    /// Null targets are skipped. The director turns on the returned targets and turns off the rest.
    /// </summary>
    public static List<HighlightTarget> ThrottleHighlights(int maxActive, IEnumerable<HighlightTarget> all)
    {
        var kept = new List<HighlightTarget>();
        if (all == null || maxActive <= 0)
        {
            return kept;
        }
        foreach (HighlightTarget target in all)
        {
            if (target == null)
            {
                continue;
            }
            if (kept.Count >= maxActive)
            {
                break;
            }
            kept.Add(target);
        }
        return kept;
    }
}
