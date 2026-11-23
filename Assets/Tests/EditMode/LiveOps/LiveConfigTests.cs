using System;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// The live config (assignment 5, section 10): reading the JSON file, replacing the numbers of a difficulty level,
/// and deciding if the power outage event runs at a given moment.
/// </summary>
public class LiveConfigTests : EditModeTest
{
    private const int LiveStartingHope = 80;
    private const int LiveHopeLostWhenCaught = 3;
    private const int FileVersion = 7;

    private static readonly DateTime BeforeTheEvent = new DateTime(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DuringTheEvent = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime AfterTheEvent = new DateTime(2026, 12, 2, 12, 0, 0, DateTimeKind.Utc);

    private const string Json = @"{
        ""version"": 7,
        ""easy"": { ""startingHope"": 80, ""hopeLostWhenCaught"": -1, ""hopeNaturalDecayPerMinute"": -1 },
        ""powerOutage"": {
            ""enabled"": true,
            ""startUtc"": ""2026-11-23T00:00:00Z"",
            ""endUtc"": ""2026-11-30T00:00:00Z"",
            ""openingLine"": ""The lights are out."",
            ""tunnelLightFactor"": 0.45,
            ""guardSpeedFactor"": 1.3
        }
    }";

    private static LivePowerOutage Outage(bool enabled)
    {
        return new LivePowerOutage { enabled = enabled, startUtc = "2026-11-23T00:00:00Z", endUtc = "2026-11-30T00:00:00Z" };
    }

    [Test]
    public void ANewConfig_ChangesNothing()
    {
        var config = new LiveConfig();

        Assert.AreEqual(LiveDifficultyNumbers.NotSet, config.NumbersFor(Difficulty.Medium).startingHope);
        Assert.IsFalse(config.powerOutage.IsActiveAt(DuringTheEvent));
    }

    [Test]
    public void FromJson_ReadsTheNumbersAndTheEvent()
    {
        LiveConfig config = LiveConfig.FromJson(Json);

        Assert.AreEqual(FileVersion, config.version);
        Assert.AreEqual(LiveStartingHope, config.NumbersFor(Difficulty.Easy).startingHope);
        Assert.IsTrue(config.powerOutage.enabled);
        Assert.AreEqual("The lights are out.", config.powerOutage.openingLine);
    }

    [Test]
    public void FromJson_ALevelTheFileDoesNotMention_KeepsItsNumbers()
    {
        LiveConfig config = LiveConfig.FromJson(Json);

        Assert.AreEqual(LiveDifficultyNumbers.NotSet, config.NumbersFor(Difficulty.Hard).startingHope);
    }

    [Test]
    public void FromJson_TextThatIsNotJson_GivesNoConfig()
    {
        Assert.IsNull(LiveConfig.FromJson("<html>404: Not Found</html>"));
    }

    [Test]
    public void UseLiveNumbers_ReplacesOnlyTheNumbersThatAreSet()
    {
        DifficultyProfile profile = Track(DifficultyProfileFactory.CreateMedium());
        int builtInStartingHope = profile.StartingHope;
        var live = new LiveDifficultyNumbers { hopeLostWhenCaught = LiveHopeLostWhenCaught };

        profile.UseLiveNumbers(live);

        Assert.AreEqual(LiveHopeLostWhenCaught, profile.HopeLostWhenCaught);
        Assert.AreEqual(builtInStartingHope, profile.StartingHope, "A number the config doesn't set stays as it was built");
    }

    [Test]
    public void TheOutage_RunsOnlyBetweenItsDates()
    {
        LivePowerOutage outage = Outage(true);

        Assert.IsFalse(outage.IsActiveAt(BeforeTheEvent));
        Assert.IsTrue(outage.IsActiveAt(DuringTheEvent));
        Assert.IsFalse(outage.IsActiveAt(AfterTheEvent));
    }

    [Test]
    public void TheOutage_DoesNotRun_WhenItIsNotEnabled()
    {
        Assert.IsFalse(Outage(false).IsActiveAt(DuringTheEvent));
    }

    [Test]
    public void TheOutage_DoesNotRun_WhenADateCanNotBeRead()
    {
        LivePowerOutage outage = Outage(true);
        outage.endUtc = "next week";

        Assert.IsFalse(outage.IsActiveAt(DuringTheEvent));
    }

    [Test]
    public void TheFileInTheRepository_IsAValidConfig()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "live-ops", "live-config.json");

        LiveConfig config = LiveConfig.FromJson(System.IO.File.ReadAllText(path));

        Assert.IsNotNull(config);
        Assert.Greater(config.version, 0);
    }
}
