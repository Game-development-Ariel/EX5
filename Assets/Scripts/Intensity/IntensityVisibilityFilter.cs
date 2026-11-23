using UnityEngine;

/// <summary>
/// Hides a renderer in Implied mode and shows it in Direct mode. Put this on sprites that only
/// belong to the Direct version of a scene (blood on the floor, bruises, a distant fire), so the
/// same scene can be built once and the renderer is hidden when the player chose Implied.
/// </summary>
public class IntensityVisibilityFilter : MonoBehaviour
{
    [Tooltip("The renderer to hide in Implied mode. If empty, the renderer on this object is used")]
    [SerializeField] private Renderer target;

    [Tooltip("The intensity settings. Falls back to ContentIntensityManager.Current if empty")]
    [SerializeField] private ContentIntensity intensity;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<Renderer>();
        }
    }

    private void OnEnable()
    {
        Apply();
        ContentIntensity current = intensity != null ? intensity : ContentIntensityManager.Current;
        if (current != null && current.ModeChanged != null)
        {
            current.ModeChanged.Subscribe(Apply);
        }
    }

    private void OnDisable()
    {
        ContentIntensity current = intensity != null ? intensity : ContentIntensityManager.Current;
        if (current != null && current.ModeChanged != null)
        {
            current.ModeChanged.Unsubscribe(Apply);
        }
    }

    /// <summary>Hides the renderer in Implied mode, shows it in Direct mode.</summary>
    public void Apply()
    {
        if (target == null)
        {
            return;
        }
        ContentIntensity current = intensity != null ? intensity : ContentIntensityManager.Current;
        IntensityLevel mode = current != null ? current.Mode : IntensityLevel.Implied;
        target.enabled = mode == IntensityLevel.Direct;
    }
}
