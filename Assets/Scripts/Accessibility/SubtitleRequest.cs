/// <summary>
/// One line of subtitle the player should see. Speaker and language let the subtitle manager style
/// voices differently (guard, narrator, hero's thought) and pick the right text in future
/// localisations. The duration is how long the line stays on screen after it finishes typing.
/// </summary>
public struct SubtitleRequest
{
    /// <summary>The spoken text, in the chosen language.</summary>
    public string Text;

    /// <summary>Who is speaking: "narrator", "guard", "hero", "radio". Free-form, used only for styling.</summary>
    public string Speaker;

    /// <summary>Seconds the subtitle stays after it finished typing in.</summary>
    public float Duration;

    /// <summary>Which language the Text is in. The manager uses it to pick the font and the direction.</summary>
    public SubtitleLanguage Language;

    public SubtitleRequest(string text, string speaker, float duration, SubtitleLanguage language)
    {
        Text = text;
        Speaker = speaker;
        Duration = duration;
        Language = language;
    }
}

/// <summary>The two languages the game ships with. He = Hebrew (right-to-left), En = English (left-to-right).</summary>
public enum SubtitleLanguage
{
    He,
    En,
}
