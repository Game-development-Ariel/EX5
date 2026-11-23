using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Enforces section 2.3 of the dramatic-elements doc: no non-diegetic UI text.
/// Scans every .cs file in the project for string literals that correspond to banned
/// "LEVEL COMPLETE" / "GAME OVER" / "SUCCESS" / "YOU WON" patterns. Also scans every
/// scene and prefab text asset for the same.
///
/// This is the authoritative enforcement. <see cref="HudFreeze"/> is the runtime fallback
/// for text set dynamically from external data.
/// </summary>
public class UIBanTest
{
    private static readonly string[] BannedTextPatterns =
    {
        "LEVEL COMPLETE",
        "LEVEL FAILED",
        "YOU WIN",
        "YOU WON",
        "YOU DIED",
        "YOU WERE CAUGHT",
        "GAME OVER",
        "VICTORY",
        "DEFEAT",
    };

    private static readonly string[] AllowedFiles =
    {
        "HudFreeze.cs",
        "UIBanTest.cs",
    };

    [Test]
    public void NoScript_ContainsBannedNonDiegeticText()
    {
        string scriptsRoot = Path.Combine(Application.dataPath, "Scripts");
        if (!Directory.Exists(scriptsRoot))
        {
            Assert.Fail($"Scripts folder not found at {scriptsRoot}");
        }

        string[] offenders = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !AllowedFiles.Contains(Path.GetFileName(path)))
            .Where(ContainsBannedText)
            .ToArray();

        Assert.IsEmpty(offenders,
            "Section 2.3 forbids non-diegetic text in UI. Found banned strings in: " +
            string.Join(", ", offenders));
    }

    [Test]
    public void NoSceneAsset_ContainsBannedNonDiegeticText()
    {
        string scenesRoot = Path.Combine(Application.dataPath, "Scenes");
        if (!Directory.Exists(scenesRoot))
        {
            Assert.Pass("No Scenes folder — scenes are built from code");
            return;
        }

        string[] offenders = Directory.GetFiles(scenesRoot, "*.unity", SearchOption.AllDirectories)
            .Where(ContainsBannedText)
            .ToArray();

        Assert.IsEmpty(offenders,
            "Section 2.3 forbids non-diegetic text in UI. Found banned text in scene files: " +
            string.Join(", ", offenders));
    }

    [Test]
    public void BannedPatterns_AreNotEmpty()
    {
        Assert.Greater(BannedTextPatterns.Length, 0, "The banned list must not be empty");
        foreach (string pattern in BannedTextPatterns)
        {
            Assert.IsNotEmpty(pattern);
            Assert.AreEqual(pattern.ToUpperInvariant(), pattern, "Patterns are expected upper-case");
        }
    }

    private static bool ContainsBannedText(string path)
    {
        string content = File.ReadAllText(path);
        string upper = content.ToUpperInvariant();

        foreach (string pattern in BannedTextPatterns)
        {
            if (upper.Contains(pattern))
            {
                return true;
            }
        }

        return false;
    }
}
