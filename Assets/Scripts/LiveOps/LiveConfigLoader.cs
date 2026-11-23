using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Downloads the live config when the game starts, and tells the rest of the game when it is ready.
/// If the download fails (no network, a broken file) the game goes on with a config that changes nothing,
/// so the game never waits for the network longer than the timeout and never breaks because of it.
/// </summary>
public class LiveConfigLoader : MonoBehaviour
{
    /// <summary>
    /// A PlayerPrefs key. When it holds 1 the loader downloads nothing and the game runs with its built-in numbers.
    /// The automated tests set it, so a live event the team is running never changes what a test checks.
    /// </summary>
    public const string OfflinePrefsKey = "LitT.LiveConfig.Offline";

    [Tooltip("Where the config file is. The raw file of the main branch on GitHub")]
    [SerializeField] private string url = "https://raw.githubusercontent.com/Game-development-Ariel/ex5/main/live-ops/live-config.json";

    [Tooltip("Seconds to wait for the download before going on without it")]
    [Min(1)]
    [SerializeField] private int timeoutSeconds = 5;

    [Header("Raises")]
    [Tooltip("Raised once, when the config is ready: downloaded, or given up on")]
    [SerializeField] private GameEvent liveConfigLoaded;

    /// <summary>The config the game runs with. Before the download ends it is one that changes nothing.</summary>
    public LiveConfig Config { get; private set; } = new LiveConfig();

    /// <summary>True from the moment liveConfigLoaded was raised.</summary>
    public bool IsReady { get; private set; }

    /// <summary>True if the config came from the file, false if the game runs without it.</summary>
    public bool CameFromTheFile { get; private set; }

    private IEnumerator Start()
    {
        if (PlayerPrefs.GetInt(OfflinePrefsKey, 0) != 0)
        {
            Finish();
            yield break;
        }

        // A different address every time skips the browser's old copy. GitHub's own servers may still send
        // a copy up to five minutes old, so a change in the file reaches the players within five minutes.
        string freshUrl = url + "?t=" + DateTime.UtcNow.Ticks;

        using (UnityWebRequest request = UnityWebRequest.Get(freshUrl))
        {
            request.timeout = timeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Use(LiveConfig.FromJson(request.downloadHandler.text));
            }
            // A failed download (no network, no such file) changes nothing: Config stays the one that changes nothing.
        }

        Finish();
    }

    private void Finish()
    {
        IsReady = true;
        liveConfigLoaded.Raise();
    }

    private void Use(LiveConfig downloaded)
    {
        if (downloaded == null)
        {
            return; // The file is not a valid config, so the game runs with its built-in numbers.
        }

        Config = downloaded;
        CameFromTheFile = true;
    }
}
