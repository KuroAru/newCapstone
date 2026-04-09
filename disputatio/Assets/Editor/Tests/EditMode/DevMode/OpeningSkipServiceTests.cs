using NUnit.Framework;
using UnityEngine;

public class OpeningSkipServiceTests
{
    [Test]
    public void Component_CanBeCreated()
    {
        var go = new GameObject("OpeningSkipServiceTest");
        var service = go.AddComponent<OpeningSkipService>();
        Assert.IsNotNull(service);
        Object.DestroyImmediate(go);
    }
}
