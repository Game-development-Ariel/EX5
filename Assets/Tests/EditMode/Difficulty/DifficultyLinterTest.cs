using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

/// <summary>
/// Proves the systems that react to difficulty hold none of the profile's numbers themselves.
/// If a value like 45 (guard check seconds), 60 (guard check or camera cone), 14 (patrol cycle),
/// 50 (starting hope), 10 (hope lost when caught), 20 (continuous dig threshold / listen interval),
/// 0.15 (dig meters per second) or 65 (battery seconds) appears in GuardPatrol, VisionSensor,
/// HopeSenses or Hope, it belongs in the DifficultyProfile.
///
/// A line ending with "// difficulty-exempt" may keep its literal when the reviewer decided the number
/// is not a difficulty parameter (for example a tolerance or a tween length).
/// </summary>
public class DifficultyLinterTest
{
    private static readonly string[] ScannedFiles =
    {
        "Assets/Scripts/Security/GuardPatrol.cs",
        "Assets/Scripts/Security/VisionSensor.cs",
        "Assets/Scripts/Player/HopeSenses.cs",
        "Assets/Scripts/Player/Hope.cs"
    };

    // Every one of these numbers lives in DifficultyProfile. Writing them in a scanned file is a design regression.
    private static readonly HashSet<string> ForbiddenLiterals = new HashSet<string>
    {
        "45", "60", "14", "50", "10", "20", "0.15", "65"
    };

    private const string ExemptMarker = "// difficulty-exempt";

    private static readonly Regex BlockComment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
    private static readonly Regex LineComment = new Regex(@"//[^\n]*");
    private static readonly Regex TextOrChar = new Regex(@"@?\$?""(?:[^""\\\n]|\\.)*""|'(?:[^'\\]|\\.)'");
    private static readonly Regex Attribute = new Regex(@"^\s*\[[^\]\n]*\]", RegexOptions.Multiline);
    private static readonly Regex NamedDeclaration = new Regex(@"\b(?:const|static\s+readonly)\b[^;]*;", RegexOptions.Singleline);
    private static readonly Regex FieldDefault = new Regex(
        @"(?:\[[^\]\n]*\bSerializeField\b[^\]\n]*\](?:\s*\[[^\]]*\])*\s*(?:private|protected)?\s*|(?<![\w.])public\s+)[\w<>\[\],. ]+?\s+\w+\s*=(?!>)[^;]*;");
    private static readonly Regex Number = new Regex(@"(?<![\w.])\d+(?:\.\d+)?[fFdDmM]?(?![\w.])");

    private static string KeepLines(Match match)
    {
        return new string('\n', match.Value.Count(c => c == '\n'));
    }

    private static string WithoutNamedPlaces(string code)
    {
        code = BlockComment.Replace(code, KeepLines);
        code = TextOrChar.Replace(code, "\"\"");
        code = LineComment.Replace(code, "");
        code = NamedDeclaration.Replace(code, KeepLines);
        code = FieldDefault.Replace(code, KeepLines);
        return Attribute.Replace(code, "");
    }

    private static string Value(string literal)
    {
        string value = literal.TrimEnd('f', 'F', 'd', 'D', 'm', 'M');
        return value.EndsWith(".0") ? value.Substring(0, value.Length - 2) : value;
    }

    [Test]
    public void SystemsThatReactToDifficulty_HoldNoDifficultyNumbers()
    {
        var problems = new List<string>();
        foreach (string path in ScannedFiles)
        {
            if (!File.Exists(path))
            {
                continue;
            }
            string code = File.ReadAllText(path).Replace("\r\n", "\n");
            string[] originalLines = code.Split('\n');
            string[] strippedLines = WithoutNamedPlaces(code).Split('\n');
            for (int i = 0; i < strippedLines.Length; i++)
            {
                if (i < originalLines.Length && originalLines[i].Contains(ExemptMarker))
                {
                    continue;
                }
                foreach (Match number in Number.Matches(strippedLines[i]))
                {
                    if (ForbiddenLiterals.Contains(Value(number.Value)))
                    {
                        problems.Add($"{path.Replace('\\', '/')}:{i + 1}: {number.Value} belongs in DifficultyProfile, not in \"{originalLines[i].Trim()}\"");
                    }
                }
            }
        }

        Assert.IsEmpty(problems, "Move these numbers to DifficultyProfile, or mark the line with \"// difficulty-exempt\":\n" + string.Join("\n", problems));
    }
}
