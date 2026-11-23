using System;

/// <summary>
/// The live event "power outage", as it is written in the live config. The names are the keys of the JSON file.
/// The event runs when it is enabled and the time now is between its start and its end.
/// </summary>
[Serializable]
public class LivePowerOutage
{
    public bool enabled;

    // Dates as text, in UTC, like "2026-11-23T00:00:00Z".
    public string startUtc = "";
    public string endUtc = "";

    // The line under "Day 1" in the opening while the event runs.
    public string openingLine = "";

    // How much of the tunnel's light is left: 1 = as usual, 0.5 = half.
    public float tunnelLightFactor = 1f;

    // How fast the guard walks: 1 = as usual, 1.3 = 30 percent faster.
    public float guardSpeedFactor = 1f;

    /// <summary>True if the event runs at the given moment (in UTC). Dates that can't be read mean it doesn't run.</summary>
    public bool IsActiveAt(DateTime utcNow)
    {
        if (!enabled)
        {
            return false;
        }

        bool startRead = DateTime.TryParse(startUtc, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime start);
        bool endRead = DateTime.TryParse(endUtc, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime end);
        return startRead && endRead && utcNow >= start && utcNow <= end;
    }
}
