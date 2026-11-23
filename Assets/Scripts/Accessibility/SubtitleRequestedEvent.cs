using UnityEngine;

/// <summary>
/// Event asset carrying a SubtitleRequest. Audio or dialog systems raise it, the SubtitleManager
/// listens. Kept separate so both can be wired in the Inspector without naming each other.
/// </summary>
[CreateAssetMenu(menuName = "Events/Subtitle Requested")]
public class SubtitleRequestedEvent : TypedGameEvent<SubtitleRequest>
{
}
