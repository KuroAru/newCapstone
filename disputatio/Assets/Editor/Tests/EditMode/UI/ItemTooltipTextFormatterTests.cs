using NUnit.Framework;

public class ItemTooltipTextFormatterTests
{
    [Test]
    public void Build_ReturnsTrimmedNameAndDescription_WhenBothValuesExist()
    {
        string result = ItemTooltipTextFormatter.Build("  Key  ", "  Opens hidden room. ");

        Assert.AreEqual("Key\nOpens hidden room.", result);
    }

    [Test]
    public void Build_UsesFallbackName_WhenNameIsMissing()
    {
        string result = ItemTooltipTextFormatter.Build("", "Readable description");

        Assert.AreEqual("Unknown Item\nReadable description", result);
    }

    [Test]
    public void Build_UsesFallbackDescription_WhenDescriptionIsMissing()
    {
        string result = ItemTooltipTextFormatter.Build("Lantern", " ");

        Assert.AreEqual("Lantern\n설명이 없습니다.", result);
    }
}
