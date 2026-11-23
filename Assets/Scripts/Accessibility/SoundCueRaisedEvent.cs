using UnityEngine;

/// <summary>
/// Event asset carrying a SoundCue. The audio layer raises it, the VisualCueOverlay listens.
/// Two assets stay decoupled the same way GameEvent couples the security camera and the HUD.
/// </summary>
[CreateAssetMenu(menuName = "Events/Sound Cue Raised")]
public class SoundCueRaisedEvent : TypedGameEvent<SoundCue>
{
}
