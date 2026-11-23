using UnityEngine;

/// <summary>
/// Writes the sounds of the hero's body as subtitles, for players who can't hear them (design 3.3).
/// In Worry the breath is heard, and in Despair the pulse covers the steps outside: both are told
/// only by sound, so without a caption a deaf player would miss a change that matters.
/// </summary>
public class BodyCaptions : MonoBehaviour
{
    [SerializeField] private HopeBody body;

    [Tooltip("The subtitle manager listens to it")]
    [SerializeField] private SubtitleRequestedEvent subtitleRequested;

    [SerializeField] private string worryCaption = "[breathing, heavier]";
    [SerializeField] private string despairCaption = "[heartbeat, loud. it covers the steps outside]";

    [Tooltip("Seconds a caption stays on the screen")]
    [SerializeField] private float captionSeconds = 3f;

    private void OnEnable()
    {
        body.TierChanged += OnTierChanged;
    }

    private void OnDisable()
    {
        body.TierChanged -= OnTierChanged;
    }

    private void OnTierChanged(HopeTier tier)
    {
        if (body.GetComponent<Hope>().IsStarting)
        {
            return; // The level's starting hope is not a change the hero's body makes a sound about.
        }

        if (tier == HopeTier.Worry)
        {
            Caption(worryCaption);
        }
        else if (tier == HopeTier.Despair)
        {
            Caption(despairCaption);
        }
    }

    private void Caption(string text)
    {
        subtitleRequested.Raise(new SubtitleRequest(text, null, captionSeconds, SubtitleLanguage.En));
    }
}
