using NUnit.Framework;
using UnityEngine;

public class PotPanelBottleBackgroundSyncTests
{
    [Test]
    public void ChoosePotPanelSprite_ReturnsVisibleSprite_WhenGetBottleFalse()
    {
        Sprite whenFalse = CreateSprite("whenFalse");
        Sprite whenTrue = CreateSprite("whenTrue");

        Sprite result = PotPanelBottleBackgroundSync.ChoosePotPanelSprite(false, whenFalse, whenTrue);

        Assert.AreSame(whenFalse, result);
    }

    [Test]
    public void ChoosePotPanelSprite_ReturnsBottleOffSprite_WhenGetBottleTrue()
    {
        Sprite whenFalse = CreateSprite("whenFalse");
        Sprite whenTrue = CreateSprite("whenTrue");

        Sprite result = PotPanelBottleBackgroundSync.ChoosePotPanelSprite(true, whenFalse, whenTrue);

        Assert.AreSame(whenTrue, result);
    }

    private static Sprite CreateSprite(string objectName)
    {
        var texture = new Texture2D(2, 2);
        texture.name = objectName + "_texture";
        return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }
}
