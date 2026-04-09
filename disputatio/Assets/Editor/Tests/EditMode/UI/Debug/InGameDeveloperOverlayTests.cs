using NUnit.Framework;
using UnityEngine;

public class InGameDeveloperOverlayTests
{
    [Test]
    public void Component_TogglesVisibilityWithoutException()
    {
        var go = new GameObject("InGameDeveloperOverlayTest");
        var overlay = go.AddComponent<InGameDeveloperOverlay>();
        overlay.SetVisible(true);
        overlay.ToggleVisible();
        overlay.ToggleVisible();
        Assert.IsNotNull(overlay);
        Object.DestroyImmediate(go);
    }
}
