using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The full catalogue of optional moments in the game, saved as one ScriptableObject asset.
/// Every zone, hold-to-interact action and ending beat looks a moment up here by id.
/// From design 3.1: the three layers are discovery (notes, graffiti, environment, bracelet,
/// guards' talks, the radio), expression (writing on the sandbag, marking the counting wall,
/// the "I want to remember" journal line, lighting the flashlight in the empty room), and
/// observation (holding listen at a distance, sitting at the prayer spot, looking at the door
/// crack, re-reading the journal on Shabbat before the page is torn).
/// </summary>
[CreateAssetMenu(menuName = "Optional/Optional Registry")]
public class OptionalRegistry : ScriptableObject
{
    [Tooltip("Every optional moment in the game. One id appears once. Used by zones, actions and the ending")]
    [SerializeField] private List<OptionalEntry> entries = new List<OptionalEntry>();

    /// <summary>The full list of optional moments. Read-only to callers; edited in the Inspector.</summary>
    public IReadOnlyList<OptionalEntry> Entries => entries;

    /// <summary>The entry with this id, or null if the id is not in the registry.</summary>
    public OptionalEntry Find(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        foreach (OptionalEntry entry in entries)
        {
            if (entry.Id == id)
            {
                return entry;
            }
        }
        return null;
    }

    /// <summary>
    /// Fills the list with the starter entries from the design doc. Called from the Inspector menu,
    /// not at runtime. The ids chosen here are the ones the rest of the game refers to.
    /// </summary>
    [ContextMenu("Fill with starter entries")]
    private void FillStarters()
    {
        entries.Clear();

        // Discovery, layer one: notes left by previous captives, in the journal.
        entries.Add(new OptionalEntry("prev-sacks-soft-corner", "Prev: sacks by the corner, sand soft", OptionalKind.Discovery, "2"));
        entries.Add(new OptionalEntry("prev-dug-hands-short", "Prev: I started here. My hands were not enough", OptionalKind.Discovery, "2"));
        entries.Add(new OptionalEntry("prev-watches-corners", "Prev: he stops at the corners. At the corners he looks back", OptionalKind.Discovery, "3"));

        // Discovery, layer one: writing on the walls.
        entries.Add(new OptionalEntry("wall-count-marks", "Counting wall: five-by-five scratches", OptionalKind.Discovery, "2"));
        entries.Add(new OptionalEntry("wall-no-sleep", "Wall in the corner: 'at home no one sleeps'", OptionalKind.Discovery, "4"));

        // Discovery, layer one: details in the guard room.
        entries.Add(new OptionalEntry("guard-room-watch", "Guard room: a working clock", OptionalKind.Discovery, "3"));
        entries.Add(new OptionalEntry("guard-room-cups", "Guard room: tea cups on the table", OptionalKind.Discovery, "3"));
        entries.Add(new OptionalEntry("guard-room-flower", "Guard room: a dried flower in one of the cups", OptionalKind.Discovery, "3"));

        // Discovery, layer one: the festival bracelet (only visible in high hope).
        entries.Add(new OptionalEntry("bracelet-festival", "The festival bracelet on the hero's wrist", OptionalKind.Discovery, "1"));

        // Discovery, layer one: overheard guard talk and the radio.
        entries.Add(new OptionalEntry("guards-talk-home", "A guard talks about his home and a sick relative", OptionalKind.Discovery, "3"));
        entries.Add(new OptionalEntry("radio-families-wait", "Radio fragment: 'the families are still waiting'", OptionalKind.Discovery, "3"));

        // Expression, layer two: the hostage choice and the counting-wall mark.
        entries.Add(new OptionalEntry("sandbag-hostage-write", "Wrote 'hostage' on the white sandbag", OptionalKind.Expression, "4"));
        entries.Add(new OptionalEntry("count-wall-mark", "Added a line of your own to the counting wall", OptionalKind.Expression, "2"));

        // Expression, layer two: the journal's "what I want to remember" page.
        entries.Add(new OptionalEntry("journal-i-want-to-remember", "Journal entry: one thing to remember", OptionalKind.Expression, "3"));

        // Expression, layer two: spending a battery to light the empty room.
        entries.Add(new OptionalEntry("flashlight-empty-room", "Flashlight on in the empty room", OptionalKind.Expression, "4"));

        // Observation, layer three: holding listen with no intent to move.
        entries.Add(new OptionalEntry("listen-at-distance", "Listened to the far world: a guard, keys, distant thunder", OptionalKind.Observation, "3"));

        // Observation, layer three: sitting at the prayer spot even when hope is already full.
        entries.Add(new OptionalEntry("prayer-spot-sit", "Sat at the prayer spot. Breath calmed", OptionalKind.Observation, "3"));

        // Observation, layer three: watching the crack of light under the door.
        entries.Add(new OptionalEntry("door-crack-look", "Watched the crack of light under the door", OptionalKind.Observation, "1"));

        // Observation, layer three: re-reading the journal before the Shabbat page is torn in stage 3.
        entries.Add(new OptionalEntry("journal-reread-shabbat", "Re-read the Shabbat journal page", OptionalKind.Observation, "3"));
    }
}
