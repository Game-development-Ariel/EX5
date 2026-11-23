using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The place every flasher — alarms, bulb flickers, explosion flashes — asks whether it is allowed
/// to flash. When reducedFlashing is on, ShouldFlash is false; callers must choose a steady colour
/// instead, and the FlashFree helper does a 0.3s fade on a Light2D for callers that used to flash a
/// colour. The design keeps the diegetic feel (section 2.3) — the tunnel still responds, just softly.
/// </summary>
public static class ReducedFlashing
{
    // The shortest fade, in seconds. A fade of zero seconds would be a flash again.
    private const float ShortestFadeSeconds = 0.01f;

    /// <summary>Length of the soft fade that replaces a flash.</summary>
    public const float FadeSeconds = 0.3f;

    /// <summary>True when callers may flash. False when they should use a steady colour and FlashFree.</summary>
    public static bool ShouldFlash
    {
        get { return !AccessibilitySettings.Current.ReducedFlashing; }
    }

    /// <summary>
    /// Fades a Light2D to the given colour over FadeSeconds without any flicker. The caller passes
    /// its MonoBehaviour so the coroutine stays bound to the right object. If the behaviour is
    /// inactive the colour is applied instantly to stay safe for pooled objects.
    /// </summary>
    public static void FlashFree(MonoBehaviour host, Light2D light, Color targetColor, float intensity)
    {
        if (light == null)
        {
            return;
        }
        if (host == null || !host.isActiveAndEnabled)
        {
            light.color = targetColor;
            light.intensity = intensity;
            return;
        }
        host.StartCoroutine(FadeTo(light, targetColor, intensity, FadeSeconds));
    }

    /// <summary>Overload for callers that want a custom duration.</summary>
    public static void FlashFree(MonoBehaviour host, Light2D light, Color targetColor, float intensity, float seconds)
    {
        if (light == null)
        {
            return;
        }
        if (host == null || !host.isActiveAndEnabled)
        {
            light.color = targetColor;
            light.intensity = intensity;
            return;
        }
        host.StartCoroutine(FadeTo(light, targetColor, intensity, Mathf.Max(ShortestFadeSeconds, seconds)));
    }

    private static IEnumerator FadeTo(Light2D light, Color targetColor, float intensity, float seconds)
    {
        Color fromColor = light.color;
        float fromIntensity = light.intensity;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            float lerp = Mathf.Clamp01(t / seconds);
            light.color = Color.Lerp(fromColor, targetColor, lerp);
            light.intensity = Mathf.Lerp(fromIntensity, intensity, lerp);
            yield return null;
        }
        light.color = targetColor;
        light.intensity = intensity;
    }
}
