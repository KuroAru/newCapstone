using NUnit.Framework;
using UnityEngine;

public class AddItemToInventoryTests
{
    private Item CreateTestItem(int id, string name)
    {
        var item = ScriptableObject.CreateInstance<Item>();
        item.itemId = id;
        item.itemName = name;
        return item;
    }

    [Test]
    public void FindItemById_ReturnsItem_WhenIdExists()
    {
        Item bottle = CreateTestItem(1, "Bottle");

        Item found = AddItemToInventory.FindItemById(1);

        Assert.IsNotNull(found);
        Assert.AreEqual("Bottle", found.itemName);

        Object.DestroyImmediate(bottle);
    }

    [Test]
    public void FindItemById_ReturnsNull_WhenIdDoesNotExist()
    {
        Item result = AddItemToInventory.FindItemById(999);

        Assert.IsNull(result);
    }

    [Test]
    public void FindItemById_ReturnsCorrectItem_WhenMultipleExist()
    {
        Item a = CreateTestItem(5, "ItemA");
        Item b = CreateTestItem(10, "ItemB");

        Item found = AddItemToInventory.FindItemById(10);

        Assert.IsNotNull(found);
        Assert.AreEqual("ItemB", found.itemName);

        Object.DestroyImmediate(a);
        Object.DestroyImmediate(b);
    }

    [Test]
    public void GetSummary_ReturnsItemId()
    {
        var go = new GameObject("TestCmd");
        var cmd = go.AddComponent<AddItemToInventory>();

        string summary = cmd.GetSummary();

        Assert.IsTrue(summary.Contains("targetItemId"));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void GetButtonColor_ReturnsNonBlack()
    {
        var go = new GameObject("TestCmd");
        var cmd = go.AddComponent<AddItemToInventory>();

        Color c = cmd.GetButtonColor();

        Assert.AreNotEqual(Color.black, c);

        Object.DestroyImmediate(go);
    }
}
