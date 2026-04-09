using NUnit.Framework;
using UnityEngine;

public class ParrotPanelUiFixTests
{
    [Test]
    public void ChoosePanelBackground_ReturnsElectricSprite_WhenElectricOnIsTrue()
    {
        Sprite defaultSprite = CreateSprite("default");
        Sprite electricOnSprite = CreateSprite("electricOn");

        Sprite result = ParrotPanelUiFix.ChoosePanelBackground(defaultSprite, electricOnSprite, true);

        Assert.AreSame(electricOnSprite, result);
    }

    [Test]
    public void ChoosePanelBackground_ReturnsDefaultSprite_WhenElectricOnIsFalse()
    {
        Sprite defaultSprite = CreateSprite("default");
        Sprite electricOnSprite = CreateSprite("electricOn");

        Sprite result = ParrotPanelUiFix.ChoosePanelBackground(defaultSprite, electricOnSprite, false);

        Assert.AreSame(defaultSprite, result);
    }

    [Test]
    public void ChoosePanelBackground_ReturnsDefaultSprite_WhenElectricOnSpriteIsMissing()
    {
        Sprite defaultSprite = CreateSprite("default");

        Sprite result = ParrotPanelUiFix.ChoosePanelBackground(defaultSprite, null, true);

        Assert.AreSame(defaultSprite, result);
    }

    private static Sprite CreateSprite(string objectName)
    {
        var texture = new Texture2D(2, 2);
        texture.name = objectName + "_texture";
        return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }
}
