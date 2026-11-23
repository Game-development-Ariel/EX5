using UnityEngine;

/// <summary>
/// Plays one clip in Implied mode and another in Direct mode. For example an explosion: in Implied
/// the player hears only a muffled rumble; in Direct it is the full sound. The AudioSource is on
/// the same object, so the volume, falloff and spatial settings are shared between the modes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class IntensityAudio : MonoBehaviour
{
    [Tooltip("The quieter, less direct version. Used in Implied mode")]
    [SerializeField] private AudioClip impliedClip;

    [Tooltip("The full version. Used in Direct mode")]
    [SerializeField] private AudioClip directClip;

    [Tooltip("The intensity settings. Falls back to ContentIntensityManager.Current if empty")]
    [SerializeField] private ContentIntensity intensity;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    /// <summary>Plays the clip for the current mode. Does nothing if the chosen clip is null.</summary>
    public void Play()
    {
        AudioClip clip = Pick();
        if (clip == null || source == null)
        {
            return;
        }
        source.PlayOneShot(clip);
    }

    /// <summary>Returns the clip for the current mode: Direct → directClip, Implied → impliedClip.</summary>
    public AudioClip Pick()
    {
        ContentIntensity current = intensity != null ? intensity : ContentIntensityManager.Current;
        IntensityLevel mode = current != null ? current.Mode : IntensityLevel.Implied;
        return mode == IntensityLevel.Direct ? directClip : impliedClip;
    }
}
