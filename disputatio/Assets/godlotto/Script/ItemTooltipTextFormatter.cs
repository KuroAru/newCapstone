using System.Text;

public static class ItemTooltipTextFormatter
{
    public static string Build(string itemName, string itemDescription)
    {
        string safeName = string.IsNullOrWhiteSpace(itemName) ? "Unknown Item" : itemName.Trim();
        string safeDescription = string.IsNullOrWhiteSpace(itemDescription) ? "설명이 없습니다." : itemDescription.Trim();

        var builder = new StringBuilder();
        builder.Append(safeName);
        builder.Append('\n');
        builder.Append(safeDescription);
        return builder.ToString();
    }
}
