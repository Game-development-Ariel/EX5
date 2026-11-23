using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pushes the picture so low-vision players can read the dark tunnel. When highContrast is on,
/// this script overrides the camera's ColorAdjustments: contrast +40, post exposure +0.5. It also
/// multiplies the HighlightTarget's onIntensity by a boost, so "look at this" objects pop harder.
/// With the toggle off, the originals come back and the game looks exactly as designed. Does not
/// change the saturation, so colourblind-friendly stays a separate axis.
/// </summary>
[RequireComponent(typeof(Volume))]
public class HighContrastRenderer : MonoBehaviour
{
    private const float ContrastBoost = 40f;
    private const float ExposureBoost = 0.5f;
    private const float HighlightBoost = 1.35f;

    [Tooltip("Highlight targets whose onIntensity should be scaled up in high contrast. Leave empty to find them at Start")]
    [SerializeField] private HighlightTarget[] highlightTargets;

    private VolumeProfile profile;
    private ColorAdjustments colors;

    private float baseContrast;
    private float baseExposure;
    private readonly System.Collections.Generic.Dictionary<HighlightTarget, float> baseOnIntensities = new System.Collections.Generic.Dictionary<HighlightTarget, float>();
    private bool applied;

    private void Awake()
    {
        profile = GetComponent<Volume>().profile;
        if (profile != null)
        {
            profile.TryGet(out colors);
        }
        if (colors != null)
        {
            baseContrast = colors.contrast.value;
            baseExposure = colors.postExposure.value;
        }
    }

    private void Start()
    {
        if (highlightTargets == null || highlightTargets.Length == 0)
        {
            // Scan the scene so authored and runtime-added targets are both covered.
            highlightTargets = FindObjectsByType<HighlightTarget>(FindObjectsSortMode.None);
        }
        foreach (HighlightTarget target in highlightTargets)
        {
            if (target != null)
            {
                baseOnIntensities[target] = target.OnIntensity;
            }
        }
        Refresh();
    }

    private void OnEnable()
    {
        GameEvent changed = AccessibilitySettings.Current.SettingsChanged;
        if (changed != null)
        {
            changed.Subscribe(Refresh);
        }
    }

    private void OnDisable()
    {
        GameEvent changed = AccessibilitySettings.Current.SettingsChanged;
        if (changed != null)
        {
            changed.Unsubscribe(Refresh);
        }
        // Restore even if the object is pulled, so the picture never stays shifted.
        RestoreOriginals();
    }

    private void OnDestroy()
    {
        if (profile != null)
        {
            Destroy(profile);
        }
    }

    /// <summary>Public so a settings menu can push the state after a direct change without raising the event.</summary>
    public void Refresh()
    {
        if (AccessibilitySettings.Current.HighContrast)
        {
            ApplyHighContrast();
        }
        else
        {
            RestoreOriginals();
        }
    }

    private void ApplyHighContrast()
    {
        if (colors != null)
        {
            colors.contrast.overrideState = true;
            colors.postExposure.overrideState = true;
            colors.contrast.value = baseContrast + ContrastBoost;
            colors.postExposure.value = baseExposure + ExposureBoost;
        }
        if (!applied)
        {
            foreach (var pair in baseOnIntensities)
            {
                if (pair.Key == null)
                {
                    continue;
                }
                var intensityField = typeof(HighlightTarget).GetField("onIntensity",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                intensityField?.SetValue(pair.Key, pair.Value * HighlightBoost);
            }
            applied = true;
        }
    }

    private void RestoreOriginals()
    {
        if (colors != null)
        {
            colors.contrast.value = baseContrast;
            colors.postExposure.value = baseExposure;
        }
        if (applied)
        {
            foreach (var pair in baseOnIntensities)
            {
                if (pair.Key == null)
                {
                    continue;
                }
                var intensityField = typeof(HighlightTarget).GetField("onIntensity",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                intensityField?.SetValue(pair.Key, pair.Value);
            }
            applied = false;
        }
    }
}
