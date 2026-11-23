using System;
using UnityEngine;

/// <summary>
/// The live event "power outage". When the live config says the event runs now, the tunnel loses its electricity:
/// the bulbs are off and the cameras are blind, but the tunnel is darker and the guard, with his flashlight, walks faster.
/// The player trades the cameras' eyes for the dark. Everything about the event comes from the config:
/// when it runs, how dark, how fast, and the line of the opening.
/// </summary>
public class PowerOutage : MonoBehaviour
{
    [SerializeField] private LiveConfigLoader liveConfig;

    [Tooltip("The parent of the tunnel's bulbs. Turned off during the event")]
    [SerializeField] private GameObject bulbs;

    [Tooltip("The parent of the cameras and the guards")]
    [SerializeField] private Transform watchers;

    [Tooltip("The hero's senses. They hold how bright the whole tunnel is")]
    [SerializeField] private HopeSenses heroSenses;

    [Header("Listens to")]
    [SerializeField] private GameEvent liveConfigLoaded;

    [Header("Raises")]
    [Tooltip("Raised with the event's line for the opening. The opening's UI listens and shows it under the day's title")]
    [SerializeField] private TextEvent openingLineChanged;

    /// <summary>True while the event runs in this level.</summary>
    public bool IsOn { get; private set; }

    private void OnEnable()
    {
        liveConfigLoaded.Subscribe(OnLiveConfigLoaded);
    }

    private void OnDisable()
    {
        liveConfigLoaded.Unsubscribe(OnLiveConfigLoaded);
    }

    private void OnLiveConfigLoaded()
    {
        LivePowerOutage outage = liveConfig.Config.powerOutage;
        if (outage.IsActiveAt(DateTime.UtcNow))
        {
            TurnThePowerOff(outage);
        }
    }

    private void TurnThePowerOff(LivePowerOutage outage)
    {
        IsOn = true;
        bulbs.SetActive(false);
        heroSenses.TunnelLightFactor = outage.tunnelLightFactor;

        foreach (CameraIndicators securityCamera in watchers.GetComponentsInChildren<CameraIndicators>())
        {
            securityCamera.TurnOff();
        }

        foreach (GuardPatrol guard in watchers.GetComponentsInChildren<GuardPatrol>())
        {
            guard.AlertFactor = outage.guardSpeedFactor;
        }

        if (!string.IsNullOrEmpty(outage.openingLine))
        {
            openingLineChanged.Raise(outage.openingLine);
        }
    }
}
