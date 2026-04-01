using System;
using System.Collections.Generic;

/// <summary>
/// Shared parsing for LLM tool arguments deserialized via Newtonsoft (bool, long, string, etc.).
/// </summary>
public static class ChatbotToolArgs
{
    public static string GetString(Dictionary<string, object> args, string key, string defaultValue = "")
    {
        if (args == null || !args.TryGetValue(key, out object raw) || raw == null)
            return defaultValue;
        if (raw is string s)
            return s;
        return raw.ToString();
    }

    public static bool TryGetBool(Dictionary<string, object> args, string key, out bool value)
    {
        value = false;
        if (args == null || !args.TryGetValue(key, out object raw) || raw == null)
            return false;

        switch (raw)
        {
            case bool b:
                value = b;
                return true;
            case long l:
                value = l != 0;
                return true;
            case int i:
                value = i != 0;
                return true;
            case double d:
                value = Math.Abs(d) > double.Epsilon;
                return true;
            case string s:
                if (bool.TryParse(s, out value))
                    return true;
                if (string.Equals(s, "1", StringComparison.OrdinalIgnoreCase))
                {
                    value = true;
                    return true;
                }
                if (string.Equals(s, "0", StringComparison.OrdinalIgnoreCase))
                {
                    value = false;
                    return true;
                }
                if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase))
                {
                    value = true;
                    return true;
                }
                if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase))
                {
                    value = false;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }
}
