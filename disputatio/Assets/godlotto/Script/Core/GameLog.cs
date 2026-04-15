using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// Thin wrapper over <see cref="UnityEngine.Debug"/> whose methods are compiled out of
/// release builds via <see cref="ConditionalAttribute"/>. Use <c>GameLog.Log</c> instead
/// of <c>Debug.Log</c> for gameplay diagnostics that should never ship.
/// <para>
/// <c>Debug.LogError</c> / <c>Debug.LogException</c> calls that indicate genuine bugs
/// should remain as-is (they must survive into release builds).
/// </para>
/// </summary>
public static class GameLog
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, UnityEngine.Object context)
    {
        Debug.Log(message, context);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message, UnityEngine.Object context)
    {
        Debug.LogWarning(message, context);
    }
}
