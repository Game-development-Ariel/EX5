using System;

/// <summary>
/// The numbers of one difficulty level that the live config may replace. The names are the keys of the JSON file.
/// A value of NotSet (-1) means "keep the number the game was built with".
/// </summary>
[Serializable]
public class LiveDifficultyNumbers
{
    /// <summary>What a field holds when the live config doesn't replace it.</summary>
    public const int NotSet = -1;

    public int startingHope = NotSet;
    public int hopeLostWhenCaught = NotSet;
    public float hopeNaturalDecayPerMinute = NotSet;
}
