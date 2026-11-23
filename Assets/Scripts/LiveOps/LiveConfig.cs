using System;
using UnityEngine;

/// <summary>
/// Everything the game reads from the live config file (live-ops/live-config.json in the repository).
/// The team changes that file on GitHub, and the next run of the game behaves differently, with no new build.
/// A new LiveConfig, before any file was read, changes nothing: no number is replaced and no event runs.
/// </summary>
[Serializable]
public class LiveConfig
{
    public int version;

    public LiveDifficultyNumbers easy = new LiveDifficultyNumbers();
    public LiveDifficultyNumbers medium = new LiveDifficultyNumbers();
    public LiveDifficultyNumbers hard = new LiveDifficultyNumbers();

    public LivePowerOutage powerOutage = new LivePowerOutage();

    /// <summary>The replaced numbers of the given difficulty level.</summary>
    public LiveDifficultyNumbers NumbersFor(Difficulty difficulty)
    {
        if (difficulty == Difficulty.Easy)
        {
            return easy;
        }
        return difficulty == Difficulty.Hard ? hard : medium;
    }

    /// <summary>Reads a config from the text of the JSON file. Returns null if the text is not a config.</summary>
    public static LiveConfig FromJson(string json)
    {
        try
        {
            return JsonUtility.FromJson<LiveConfig>(json);
        }
        catch (ArgumentException)
        {
            return null; // The text is not JSON at all, for example an error page.
        }
    }
}
