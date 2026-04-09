using NUnit.Framework;
using UnityEngine;

public class QuickRestartServiceTests
{
    [Test]
    public void Component_CanBeCreated()
    {
        var go = new GameObject("QuickRestartServiceTest");
        var service = go.AddComponent<QuickRestartService>();
        Assert.IsNotNull(service);
        Object.DestroyImmediate(go);
    }
}
