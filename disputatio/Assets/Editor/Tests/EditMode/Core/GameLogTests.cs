using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class GameLogTests
{
    [Test]
    public void Log_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.Log("edit-mode game log"));
    }

    [Test]
    public void Log_WithContext_DoesNotThrow()
    {
        var ctx = new GameObject("GameLogContext");
        Assert.DoesNotThrow(() => GameLog.Log("with context", ctx));
        Object.DestroyImmediate(ctx);
    }

    [Test]
    public void Log_ForwardsToUnityDebugLog()
    {
        const string message = "[GameLogTests] log forward";
        LogAssert.Expect(LogType.Log, message);
        GameLog.Log(message);
    }

    [Test]
    public void LogWarning_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.LogWarning("edit-mode warning"));
    }

    [Test]
    public void LogWarning_WithContext_DoesNotThrow()
    {
        var ctx = new GameObject("GameLogWarningContext");
        Assert.DoesNotThrow(() => GameLog.LogWarning("warning with context", ctx));
        Object.DestroyImmediate(ctx);
    }

    [Test]
    public void LogWarning_ForwardsToUnityDebugLogWarning()
    {
        const string message = "[GameLogTests] warning forward";
        LogAssert.Expect(LogType.Warning, message);
        GameLog.LogWarning(message);
    }

    // ---------------------------------------------------------------
    //  Edge cases: null / empty
    // ---------------------------------------------------------------

    [Test]
    public void Log_NullMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.Log(null));
    }

    [Test]
    public void Log_EmptyMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.Log(""));
    }

    [Test]
    public void LogWarning_NullMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.LogWarning(null));
    }

    [Test]
    public void LogWarning_EmptyMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.LogWarning(""));
    }

    [Test]
    public void Log_NullContext_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.Log("message", null));
    }

    [Test]
    public void LogWarning_NullContext_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => GameLog.LogWarning("warning", null));
    }
}
