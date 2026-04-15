using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class TestSingleton : SingletonMonoBehaviour<TestSingleton>
{
    protected override bool PersistAcrossScenes => false;

    public bool OnSingletonAwakeCalled { get; private set; }

    protected override void OnSingletonAwake() => OnSingletonAwakeCalled = true;
}

public sealed class PersistentTestSingleton : SingletonMonoBehaviour<PersistentTestSingleton>
{
    protected override bool PersistAcrossScenes => true;

    public bool OnSingletonAwakeCalled { get; private set; }

    protected override void OnSingletonAwake() => OnSingletonAwakeCalled = true;
}

[TestFixture]
public class SingletonMonoBehaviourTests
{
    [SetUp]
    public void SetUp()
    {
        DestroyAll<TestSingleton>();
        DestroyAll<PersistentTestSingleton>();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyAll<TestSingleton>();
        DestroyAll<PersistentTestSingleton>();
    }

    private static void DestroyAll<T>() where T : MonoBehaviour
    {
        T[] found = Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            T s = found[i];
            if (s != null && s.gameObject != null)
                Object.DestroyImmediate(s.gameObject);
        }
    }

    // ---------------------------------------------------------------
    //  Basic lifecycle
    // ---------------------------------------------------------------

    [Test]
    public void Instance_IsNull_BeforeAnyInstantiation()
    {
        Assert.IsNull(TestSingleton.Instance);
    }

    [Test]
    public void AfterAwake_Instance_IsSetToObject()
    {
        var go = new GameObject(nameof(AfterAwake_Instance_IsSetToObject));
        TestSingleton s = go.AddComponent<TestSingleton>();

        Assert.IsNotNull(TestSingleton.Instance);
        Assert.AreSame(s, TestSingleton.Instance);
        Assert.IsTrue(s.OnSingletonAwakeCalled);

        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator DuplicateInstance_SecondAwake_IsDestroyedOrUnityNull()
    {
        var go1 = new GameObject("SingletonFirst");
        TestSingleton first = go1.AddComponent<TestSingleton>();
        Assert.AreSame(first, TestSingleton.Instance);

        var go2 = new GameObject("SingletonSecond");
        TestSingleton second = go2.AddComponent<TestSingleton>();

        Assert.AreSame(first, TestSingleton.Instance);
        Assert.IsFalse(second.OnSingletonAwakeCalled);

        yield return null;
        yield return null;

        Assert.IsTrue(second == null, "Duplicate instance should be destroyed.");
        Assert.AreSame(first, TestSingleton.Instance);

        Object.DestroyImmediate(go1);
    }

    [Test]
    public void OnDestroy_ClearsInstance_WhenSingletonDestroyed()
    {
        var go = new GameObject(nameof(OnDestroy_ClearsInstance_WhenSingletonDestroyed));
        TestSingleton s = go.AddComponent<TestSingleton>();
        Assert.AreSame(s, TestSingleton.Instance);

        Object.DestroyImmediate(go);

        Assert.IsNull(TestSingleton.Instance);
    }

    // ---------------------------------------------------------------
    //  PersistAcrossScenes variant
    // ---------------------------------------------------------------

    [Test]
    public void PersistentSingleton_Instance_IsSet()
    {
        var go = new GameObject(nameof(PersistentSingleton_Instance_IsSet));
        PersistentTestSingleton s = go.AddComponent<PersistentTestSingleton>();

        Assert.IsNotNull(PersistentTestSingleton.Instance);
        Assert.AreSame(s, PersistentTestSingleton.Instance);
        Assert.IsTrue(s.OnSingletonAwakeCalled);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void PersistentSingleton_OnDestroy_ClearsInstance()
    {
        var go = new GameObject(nameof(PersistentSingleton_OnDestroy_ClearsInstance));
        go.AddComponent<PersistentTestSingleton>();
        Assert.IsNotNull(PersistentTestSingleton.Instance);

        Object.DestroyImmediate(go);

        Assert.IsNull(PersistentTestSingleton.Instance);
    }

    [UnityTest]
    public IEnumerator PersistentSingleton_Duplicate_IsDestroyed()
    {
        var go1 = new GameObject("PersistFirst");
        PersistentTestSingleton first = go1.AddComponent<PersistentTestSingleton>();

        var go2 = new GameObject("PersistSecond");
        PersistentTestSingleton second = go2.AddComponent<PersistentTestSingleton>();

        Assert.AreSame(first, PersistentTestSingleton.Instance);
        Assert.IsFalse(second.OnSingletonAwakeCalled);

        yield return null;
        yield return null;

        Assert.IsTrue(second == null, "Duplicate persistent singleton should be destroyed.");
        Assert.AreSame(first, PersistentTestSingleton.Instance);

        Object.DestroyImmediate(go1);
    }

    // ---------------------------------------------------------------
    //  Independent singleton types do not interfere
    // ---------------------------------------------------------------

    [Test]
    public void TwoSingletonTypes_HaveIndependentInstances()
    {
        var go1 = new GameObject("TypeA");
        TestSingleton a = go1.AddComponent<TestSingleton>();

        var go2 = new GameObject("TypeB");
        PersistentTestSingleton b = go2.AddComponent<PersistentTestSingleton>();

        Assert.AreSame(a, TestSingleton.Instance);
        Assert.AreSame(b, PersistentTestSingleton.Instance);

        Object.DestroyImmediate(go1);
        Assert.IsNull(TestSingleton.Instance);
        Assert.IsNotNull(PersistentTestSingleton.Instance);

        Object.DestroyImmediate(go2);
    }

    // ---------------------------------------------------------------
    //  Re-instantiation after destroy
    // ---------------------------------------------------------------

    [Test]
    public void Singleton_CanBeReinstantiated_AfterDestroy()
    {
        var go1 = new GameObject("FirstLife");
        TestSingleton first = go1.AddComponent<TestSingleton>();
        Assert.AreSame(first, TestSingleton.Instance);

        Object.DestroyImmediate(go1);
        Assert.IsNull(TestSingleton.Instance);

        var go2 = new GameObject("SecondLife");
        TestSingleton second = go2.AddComponent<TestSingleton>();
        Assert.AreSame(second, TestSingleton.Instance);
        Assert.IsTrue(second.OnSingletonAwakeCalled);

        Object.DestroyImmediate(go2);
    }
}
