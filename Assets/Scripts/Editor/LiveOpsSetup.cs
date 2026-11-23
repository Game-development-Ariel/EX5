using UnityEngine;

/// <summary>
/// Puts the live ops of assignment 5 (section 10) into the Tutorial scene: the loader of the live config
/// and the power outage event, and tells the start menu, the difficulty and the test page about them.
/// DramaSetup calls it last, because it wires to the objects DramaSetup builds.
/// </summary>
public static class LiveOpsSetup
{
    public static void Setup(GameObject dramaRoot)
    {
        GameEvent liveConfigLoaded = SetupTools.Asset<GameEvent>("Assets/Data/Events/LiveConfigLoaded.asset");

        var holder = new GameObject("Live Ops");
        holder.transform.SetParent(dramaRoot.transform, false);

        LiveConfigLoader loader = holder.AddComponent<LiveConfigLoader>();
        SetupTools.SetField(loader, "liveConfigLoaded", liveConfigLoaded);

        PowerOutage outage = holder.AddComponent<PowerOutage>();
        SetupTools.SetField(outage, "liveConfig", loader);
        SetupTools.SetField(outage, "bulbs", GameObject.Find("Bulbs"));
        SetupTools.SetField(outage, "watchers", GameObject.Find("Watchers").transform);
        SetupTools.SetField(outage, "heroSenses", Object.FindAnyObjectByType<HopeSenses>());
        SetupTools.SetField(outage, "liveConfigLoaded", liveConfigLoaded);

        // The event's line for the opening goes to the opening's gate, which gives it to the title card.
        TextEvent openingLineChanged = SetupTools.Asset<TextEvent>("Assets/Data/Events/OpeningLineChanged.asset");
        SetupTools.SetField(outage, "openingLineChanged", openingLineChanged);
        SetupTools.SetField(dramaRoot.GetComponentInChildren<OpeningGate>(), "openingLineChanged", openingLineChanged);

        // The start menu waits for the config, and the difficulty takes its numbers from it.
        StartMenu menu = dramaRoot.GetComponentInChildren<StartMenu>();
        SetupTools.SetField(menu, "liveConfig", loader);
        SetupTools.SetField(menu, "liveConfigLoaded", liveConfigLoaded);
        SetupTools.SetField(dramaRoot.GetComponentInChildren<DifficultyManager>(), "liveConfig", loader);

        DevOverlay overlay = dramaRoot.GetComponentInChildren<DevOverlay>();
        SetupTools.SetField(overlay, "liveConfig", loader);
        SetupTools.SetField(overlay, "powerOutage", outage);
    }
}
