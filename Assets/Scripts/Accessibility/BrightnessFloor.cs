using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Keeps the darkest the camera can get above the player's minimumBrightness. The design (section
/// 2.2) wants the tunnel dark so low hope feels oppressive. Some players need a floor: a tunnel that
/// hits pitch black can leave them with no spatial information at all. This component watches the
/// camera's post-processing exposure every frame and raises it when it dips below the chosen floor.
/// With minimumBrightness at 0 the behaviour is unchanged — the game looks exactly as designed.
/// </summary>
[RequireComponent(typeof(Volume))]
public class BrightnessFloor : MonoBehaviour
{
    // The minimumBrightness slider is 0..0.3. Translated to post-exposure stops it means "this
    // picture never gets more than this many stops darker than neutral". 0.3 -> -0.9 stops, giving
    // a dim but readable image even at zero hope.
    private const float MaxDarknessStops = 3f;

    [Tooltip("Optional clamp on the global tunnel Light2D's intensity. Keeps the ambient light visible when minimumBrightness > 0")]
    [SerializeField] private Light2D tunnelLight;

    [Tooltip("Base intensity of the tunnel light at design's full darkness. Floor lifts above this")]
    [SerializeField] private float tunnelDesignIntensity = 0.1f;

    private VolumeProfile profile;
    private ColorAdjustments colors;

    private void Awake()
    {
        profile = GetComponent<Volume>().profile;
        if (profile != null)
        {
            profile.TryGet(out colors);
        }
    }

    private void OnDestroy()
    {
        if (profile != null)
        {
            Destroy(profile);
        }
    }

    private void LateUpdate()
    {
        float floor = AccessibilitySettings.Current.MinimumBrightness;
        if (floor <= 0f)
        {
            return;
        }
        if (colors != null)
        {
            float minExposure = -MaxDarknessStops * (1f - floor / 0.3f);
            if (colors.postExposure.value < minExposure)
            {
                colors.postExposure.overrideState = true;
                colors.postExposure.value = minExposure;
            }
        }
        if (tunnelLight != null)
        {
            float minIntensity = Mathf.Lerp(tunnelDesignIntensity, tunnelDesignIntensity + 0.25f, floor / 0.3f);
            if (tunnelLight.intensity < minIntensity)
            {
                tunnelLight.intensity = minIntensity;
            }
        }
    }
}
