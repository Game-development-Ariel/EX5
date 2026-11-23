/// <summary>
/// Which of the three layers an optional moment belongs to.
/// From design 3.1: discovery and exploration, self-expression and creation, and quiet observation.
/// </summary>
public enum OptionalKind
{
    /// <summary>Reading a note, seeing graffiti, noticing a detail, overhearing a guard or the radio.</summary>
    Discovery,

    /// <summary>Writing on the sandbag, adding a mark on the counting wall, the "I want to remember" journal line, lighting the flashlight.</summary>
    Expression,

    /// <summary>Holding listen at a distance, sitting at the prayer spot, looking at the door crack, re-reading the journal.</summary>
    Observation,
}
