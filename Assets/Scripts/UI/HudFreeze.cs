using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Runtime guard that freezes the non-diegetic HUD. Section 2.3 of the dramatic-elements doc
/// requires that no text like "SUCCESS", "FAILURE", "LEVEL", "SCORE" or "GAME OVER" appears on
/// screen. If a TextMeshProUGUI component in the active scene ever shows one of those strings,
/// this component disables it and logs a warning. The design of the game must stay diegetic and
/// this is the last line of defence against a regression.
///
/// The authoritative enforcement is <c>UIBanTest</c> at CI time; this component is for the
/// case where something slips past the lint (e.g. text set from code at runtime).
/// </summary>
public class HudFreeze : MonoBehaviour
{
    private static readonly string[] DefaultBannedTexts = new[]
    {
        "SUCCESS",
        "FAILURE",
        "LEVEL COMPLETE",
        "LEVEL FAILED",
        "SCORE",
        "GAME OVER",
        "YOU WIN",
        "YOU DIED",
        "YOU WERE CAUGHT",
        "VICTORY",
        "DEFEAT",
    };

    [Tooltip("How often the scene is re-scanned, in seconds. Set 0 to scan only on Awake")]
    [Min(0f)]
    [SerializeField] private float scanIntervalSeconds = 2f;

    [Tooltip("Extra banned strings specific to this project, in addition to the default list")]
    [SerializeField] private List<string> additionalBannedTexts = new List<string>();

    [Tooltip("When true, disables the offending TextMeshProUGUI instead of only logging")]
    [SerializeField] private bool disableOffenders = true;

    private float lastScanTime;

    private void Awake()
    {
        Scan();
    }

    private void Update()
    {
        if (scanIntervalSeconds <= 0f)
        {
            return;
        }

        if (Time.unscaledTime - lastScanTime >= scanIntervalSeconds)
        {
            Scan();
        }
    }

    private void Scan()
    {
        lastScanTime = Time.unscaledTime;

        TextMeshProUGUI[] labels = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMeshProUGUI label in labels)
        {
            if (label == null || string.IsNullOrEmpty(label.text))
            {
                continue;
            }

            if (!IsBanned(label.text))
            {
                continue;
            }

            Debug.LogWarning($"HudFreeze: removed non-diegetic text '{label.text}' on {label.gameObject.name}. " +
                             $"Section 2.3 of the dramatic-elements doc forbids this.", label);

            if (disableOffenders)
            {
                label.enabled = false;
            }
        }
    }

    private bool IsBanned(string text)
    {
        string upper = text.ToUpperInvariant();
        foreach (string banned in DefaultBannedTexts)
        {
            if (upper.Contains(banned))
            {
                return true;
            }
        }

        foreach (string banned in additionalBannedTexts)
        {
            if (!string.IsNullOrEmpty(banned) && upper.Contains(banned.ToUpperInvariant()))
            {
                return true;
            }
        }

        return false;
    }
}
