/// <summary>
/// The four tiers of hope, from the earlier version of section 2.2 of the dramatic-elements design doc (the README lists the gap).
/// Each tier maps to a different body state: speed, sight, hearing, breathing and pulse.
/// </summary>
public enum HopeTier
{
    /// <summary>75 and above — "אמונה". Fast, sharp sight, quiet breath.</summary>
    Faith,

    /// <summary>40 to 75 — "רגוע". Normal body, quiet breath.</summary>
    Calm,

    /// <summary>25 to 40 — "חשש". Slower, shorter sight, audible breath.</summary>
    Worry,

    /// <summary>Below 25 — "ייאוש". Very slow, tunnel vision, fast breath, pulse masks footsteps.</summary>
    Despair,
}
