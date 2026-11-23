using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// An object in the scene that the AttentionDirector can light up or dim. A bright target is
/// the player's immediate goal: the bottle, the mattress, the digging sack, the candle wire.
/// When off, the Light2D fades to offIntensity (dark by default) and the sprite (if any) fades too,
/// so the object is in the scene but hard to see. When on, the Light2D rises to onIntensity and
/// a short sting can play once to draw the eye and the ear together.
/// </summary>
public class HighlightTarget : MonoBehaviour
{
    // The default time a highlight takes to come up or fade. Small so the switch feels like an eye
    // adjusting, not a cut; scenes pass their own duration when they want it slower or faster.
    private const float DefaultLerpDuration = 0.5f;

    [Tooltip("The Light2D that paints this object bright. Optional: targets without a light fall back to the sprite alpha")]
    [SerializeField] private Light2D highlightLight;

    [Tooltip("The intensity the light reaches when the target is the current goal")]
    [SerializeField] private float onIntensity = 1f;

    [Tooltip("The intensity the light rests at when the target is not the current goal. 0 = dark")]
    [SerializeField] private float offIntensity = 0f;

    [Tooltip("The sprite that fades when there is no Light2D. Optional")]
    [SerializeField] private SpriteRenderer fallbackSprite;

    [Tooltip("The alpha of the sprite when the target is the goal")]
    [SerializeField] private float onAlpha = 1f;

    [Tooltip("The alpha of the sprite when the target is not the goal. Low = almost invisible in the dark")]
    [SerializeField] private float offAlpha = 0.15f;

    [Tooltip("A short sound that plays once when this target becomes the goal. Optional")]
    [SerializeField] private AudioSource highlightSting;

    // The state the target was last set to. Used so the sting plays only on the on transition.
    private bool highlighted;

    // The coroutine that is tweening now, so a new call can replace it.
    private Coroutine currentTween;

    /// <summary>True while this target is the active goal.</summary>
    public bool IsHighlighted => highlighted;

    /// <summary>The on intensity the Light2D reaches.</summary>
    public float OnIntensity => onIntensity;

    /// <summary>The off intensity the Light2D rests at.</summary>
    public float OffIntensity => offIntensity;

    /// <summary>The Light2D driven by this target, or null when the target uses only the sprite fallback.</summary>
    public Light2D HighlightLight => highlightLight;

    private void Awake()
    {
        // Start the scene already in the resting state. The director will raise the chosen target.
        ApplyInstant(false);
    }

    /// <summary>Sets the target on or off, tweening the light and the sprite over lerpDuration seconds.</summary>
    public void SetHighlighted(bool on, float lerpDuration = DefaultLerpDuration)
    {
        bool rising = on && !highlighted;
        highlighted = on;

        if (currentTween != null)
        {
            StopCoroutine(currentTween);
            currentTween = null;
        }

        if (!isActiveAndEnabled || !Application.isPlaying || lerpDuration <= 0f)
        {
            ApplyInstant(on);
        }
        else
        {
            currentTween = StartCoroutine(Tween(on, lerpDuration));
        }

        if (rising && highlightSting != null)
        {
            highlightSting.Play();
        }
    }

    private IEnumerator Tween(bool on, float duration)
    {
        float fromLight = highlightLight != null ? highlightLight.intensity : 0f;
        float toLight = on ? onIntensity : offIntensity;
        float fromAlpha = fallbackSprite != null ? fallbackSprite.color.a : 0f;
        float toAlpha = on ? onAlpha : offAlpha;

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float t = elapsed / duration;
            Apply(Mathf.Lerp(fromLight, toLight, t), Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }
        Apply(toLight, toAlpha);
        currentTween = null;
    }

    private void ApplyInstant(bool on)
    {
        Apply(on ? onIntensity : offIntensity, on ? onAlpha : offAlpha);
    }

    private void Apply(float lightIntensity, float spriteAlpha)
    {
        if (highlightLight != null)
        {
            highlightLight.intensity = lightIntensity;
        }
        if (fallbackSprite != null)
        {
            Color c = fallbackSprite.color;
            c.a = spriteAlpha;
            fallbackSprite.color = c;
        }
    }
}
