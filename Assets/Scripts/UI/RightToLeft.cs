using System.Collections.Generic;
using System.Text;

/// <summary>
/// The legacy UI Text draws every line left to right, so Hebrew comes out mirrored.
/// This breaks a Hebrew text into lines and writes each line backwards, so the Text draws it the right way.
/// Runs of Latin letters and digits inside the line keep their own order.
/// </summary>
public static class RightToLeft
{
    /// <summary>Returns the text ready for a legacy Text: lines of at most lineLength characters, each one reversed.</summary>
    public static string ForLegacyText(string text, int lineLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = new StringBuilder();
        foreach (string line in BreakIntoLines(text, lineLength))
        {
            if (result.Length > 0)
            {
                result.Append('\n');
            }
            result.Append(ReverseLine(line));
        }
        return result.ToString();
    }

    // The Text would break the reversed line at the wrong end, so the lines are broken here, before reversing.
    private static List<string> BreakIntoLines(string text, int lineLength)
    {
        var lines = new List<string>();
        var current = new StringBuilder();
        foreach (string word in text.Split(' '))
        {
            if (current.Length > 0 && current.Length + 1 + word.Length > lineLength)
            {
                lines.Add(current.ToString());
                current.Clear();
            }
            if (current.Length > 0)
            {
                current.Append(' ');
            }
            current.Append(word);
        }
        if (current.Length > 0)
        {
            lines.Add(current.ToString());
        }
        return lines;
    }

    private static string ReverseLine(string line)
    {
        char[] letters = line.ToCharArray();
        System.Array.Reverse(letters);

        // A number or an English word was reversed with the rest. Each such run is turned back.
        int runStart = -1;
        for (int i = 0; i <= letters.Length; i++)
        {
            bool leftToRight = i < letters.Length && IsLeftToRight(letters[i]);
            if (leftToRight && runStart < 0)
            {
                runStart = i;
            }
            else if (!leftToRight && runStart >= 0)
            {
                System.Array.Reverse(letters, runStart, i - runStart);
                runStart = -1;
            }
        }
        return new string(letters);
    }

    private static bool IsLeftToRight(char letter)
    {
        return (letter >= '0' && letter <= '9') || (letter >= 'a' && letter <= 'z') || (letter >= 'A' && letter <= 'Z');
    }
}
