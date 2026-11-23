using NUnit.Framework;

/// <summary>The legacy Text draws left to right, so Hebrew is written backwards for it, line by line.</summary>
public class RightToLeftTests
{
    private const int LongLine = 100;
    private const int ShortLine = 5;

    [Test]
    public void AHebrewLine_IsReversed()
    {
        Assert.AreEqual("םולש", RightToLeft.ForLegacyText("שלום", LongLine));
    }

    [Test]
    public void ANumberInsideTheLine_KeepsItsOwnOrder()
    {
        Assert.AreEqual("738 םוי", RightToLeft.ForLegacyText("יום 738", LongLine));
    }

    [Test]
    public void ALongText_IsBrokenIntoLines_BeforeItIsReversed()
    {
        // The first word must stay on the first line, or the text would read from the bottom up.
        Assert.AreEqual("אבג\nדהו", RightToLeft.ForLegacyText("גבא והד", ShortLine));
    }

    [Test]
    public void AnEmptyText_StaysEmpty()
    {
        Assert.AreEqual("", RightToLeft.ForLegacyText("", LongLine));
        Assert.IsNull(RightToLeft.ForLegacyText(null, LongLine));
    }
}
