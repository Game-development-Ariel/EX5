using UnityEngine;

/// <summary>
/// The shim that lets every "shake the camera" caller stay the same while the noScreenShake toggle
/// turns shakes into silence. ScreenShake itself is authored and reads its event, so this guard
/// does not replace it — new callers that want to shake imperatively route through here. A missing
/// ScreenShake is tolerated: the method logs nothing and returns, so a scene without a camera-shake
/// component does not crash.
///
/// The design (section 2.3) treats shakes as diegetic feedback (digger's hand, explosion). Removing
/// them loses nothing structural because the sound and the vignette still carry the moment.
/// </summary>
public static class ScreenShakeGuard
{
    /// <summary>Raises the given ScreenShake event if the player has not opted out.</summary>
    public static void Shake(GameEvent shakeEvent)
    {
        if (shakeEvent == null)
        {
            return;
        }
        if (AccessibilitySettings.Current.NoScreenShake)
        {
            return;
        }
        shakeEvent.Raise();
    }

    /// <summary>
    /// Imperative form for systems that hold a ScreenShake reference. Strength and duration go
    /// through reflection into its private fields, so the existing authored ScreenShake script
    /// stays untouched. No-op when noScreenShake is on, or when the component is missing.
    /// </summary>
    public static void Shake(ScreenShake shake, float strength, float duration)
    {
        if (shake == null)
        {
            return;
        }
        if (AccessibilitySettings.Current.NoScreenShake)
        {
            return;
        }
        var strengthField = typeof(ScreenShake).GetField("strength",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var secondsField = typeof(ScreenShake).GetField("seconds",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var timeLeftField = typeof(ScreenShake).GetField("timeLeft",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        strengthField?.SetValue(shake, strength);
        secondsField?.SetValue(shake, duration);
        timeLeftField?.SetValue(shake, duration);
    }

    /// <summary>Returns the shake strength scaled by the accessibility toggle. Multiplies by zero when no screen shake is on.</summary>
    public static float Scale(float strength)
    {
        return AccessibilitySettings.Current.NoScreenShake ? 0f : strength;
    }
}
