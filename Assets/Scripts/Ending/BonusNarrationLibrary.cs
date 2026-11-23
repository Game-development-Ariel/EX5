using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The extra narrator half-lines the EndingDirector may add when an optional condition is met.
/// Keys are "stageId.condition" strings, so a designer can add a line for "stage1.no-captures"
/// without changing code. Saved as an asset so the writers hold the authoritative text.
/// From design 3.3: a no-captures day earns a half-line like "and in the evening he slept well",
/// and the player never sees a notification that the line was unlocked.
/// </summary>
[CreateAssetMenu(menuName = "Tunnel/Bonus Narration Library")]
public class BonusNarrationLibrary : ScriptableObject
{
    /// <summary>
    /// One entry in the library. Serializable so the inspector can show it as a flat list;
    /// a Dictionary would hide the keys in the editor.
    /// </summary>
    [Serializable]
    public class Entry
    {
        [Tooltip("The lookup key. By convention 'stageId.condition', e.g. 'stage1.no-captures'")]
        [SerializeField] private string key;

        [Tooltip("Hebrew line. From design 3.3: 'ובערב הוא ישן טוב'")]
        [TextArea]
        [SerializeField] private string textHe;

        [Tooltip("English line, used when the project language is English")]
        [TextArea]
        [SerializeField] private string textEn;

        /// <summary>The lookup key. By convention 'stageId.condition'.</summary>
        public string Key => key;

        /// <summary>The Hebrew line.</summary>
        public string TextHe => textHe;

        /// <summary>The English line.</summary>
        public string TextEn => textEn;

        public Entry()
        {
        }

        /// <summary>Lets factory code and tests build an entry without the inspector.</summary>
        public Entry(string key, string textHe, string textEn)
        {
            this.key = key;
            this.textHe = textHe;
            this.textEn = textEn;
        }
    }

    [Tooltip("The bonus lines the ending can pick from")]
    [SerializeField] private List<Entry> entries = new List<Entry>();

    /// <summary>The entries the library holds.</summary>
    public IReadOnlyList<Entry> Entries => entries;

    /// <summary>
    /// The line for a given stage and condition, in the picked language. Returns an empty string
    /// when the key is missing, so the director's code can concatenate without a null check.
    /// </summary>
    public string Lookup(string stageId, string condition, EndingLanguage language)
    {
        string key = BuildKey(stageId, condition);
        foreach (Entry entry in entries)
        {
            if (entry != null && entry.Key == key)
            {
                string he = entry.TextHe;
                string en = entry.TextEn;
                if (language == EndingLanguage.Hebrew)
                {
                    return string.IsNullOrEmpty(he) ? en ?? string.Empty : he;
                }
                return string.IsNullOrEmpty(en) ? he ?? string.Empty : en;
            }
        }
        return string.Empty;
    }

    /// <summary>True when the library has a line for this stage and condition.</summary>
    public bool Has(string stageId, string condition)
    {
        string key = BuildKey(stageId, condition);
        foreach (Entry entry in entries)
        {
            if (entry != null && entry.Key == key)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Adds an entry at runtime. Used by the default factory and by tests.</summary>
    public void Add(Entry entry)
    {
        if (entry != null)
        {
            entries.Add(entry);
        }
    }

    /// <summary>
    /// The default library with the design-3.3 "stage 1 no-captures" line. Used by scenes
    /// that want the shipping defaults without a disk asset.
    /// </summary>
    public static BonusNarrationLibrary CreateDefault()
    {
        BonusNarrationLibrary library = ScriptableObject.CreateInstance<BonusNarrationLibrary>();
        library.Add(new Entry(
            BuildKey("stage1", Conditions.NoCaptures),
            "ובערב הוא ישן טוב.",
            "And in the evening he slept well."));
        library.Add(new Entry(
            BuildKey("stage2", Conditions.NoCaptures),
            "הוא התחיל לסמוך על עצמו.",
            "He began to trust himself."));
        return library;
    }

    /// <summary>The condition keys known to the library. Kept here so a typo is caught at compile time.</summary>
    public static class Conditions
    {
        public const string NoCaptures = "no-captures";
        public const string AllMemories = "all-memories";
        public const string AllPreviousNotes = "all-previous-notes";
        public const string SandbagWrite = "sandbag-write";
        public const string BraceletFound = "bracelet-found";
        public const string CountWallMark = "count-wall-mark";
    }

    private static string BuildKey(string stageId, string condition)
    {
        return (stageId ?? string.Empty) + "." + (condition ?? string.Empty);
    }
}
