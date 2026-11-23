using System;

/// <summary>
/// Something that remembers which optional moments the player has seen or done.
/// A wrapper (like a future variant of MemoryZone) uses this instead of touching
/// OptionalContent directly, so the wrapper can be tested without a scene singleton
/// and so the real recorder can be swapped in tests.
/// From design 3.1: the game registers which optional item the player saw, found or performed.
/// </summary>
public interface IOptionalRecorder
{
    /// <summary>Marks this id as recorded. Safe to call more than once; only the first time counts.</summary>
    void Record(string id);

    /// <summary>True once Record was called for this id, false otherwise.</summary>
    bool WasRecorded(string id);

    /// <summary>Raised the first time an id is recorded. The id is passed to the listener.</summary>
    event Action<string> Recorded;
}
