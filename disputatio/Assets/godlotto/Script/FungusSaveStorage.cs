using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Fungus <see cref="Fungus.SaveManager"/>와 동일한 경로 규칙으로 저장본을 읽기 위한 공용 유틸.
/// Standalone/Editor는 파일, WebGL/WebPlayer는 PlayerPrefs.
/// </summary>
public static class FungusSaveStorage
{
    public const string DefaultSlotKeyPrefix = "FungusSaveData_Slot";

    public static string GetHistoryJsonPath(string saveDataKey)
    {
        return Application.persistentDataPath + "/FungusSaves/" + saveDataKey + ".json";
    }

    public static bool HistoryExists(string saveDataKey)
    {
#if UNITY_WEBPLAYER || UNITY_WEBGL
        return PlayerPrefs.HasKey(saveDataKey);
#else
        return File.Exists(GetHistoryJsonPath(saveDataKey));
#endif
    }

    public static string ReadHistoryRaw(string saveDataKey)
    {
#if UNITY_WEBPLAYER || UNITY_WEBGL
        return PlayerPrefs.GetString(saveDataKey, "");
#else
        string path = GetHistoryJsonPath(saveDataKey);
        if (!File.Exists(path))
            return "";
        try
        {
            return File.ReadAllText(path);
        }
        catch (IOException)
        {
            return "";
        }
#endif
    }

    /// <summary>
    /// Fungus SaveHistory JSON 본문에서 첫 번째 save point 블록의 sceneName을 추출합니다.
    /// </summary>
    public static string TryParseSceneNameFromHistoryJson(string historyJson)
    {
        if (string.IsNullOrEmpty(historyJson))
            return null;

        int idx = historyJson.IndexOf("\"sceneName\":\"");
        if (idx == -1)
            return null;

        int start = idx + "\"sceneName\":\"".Length;
        int end = historyJson.IndexOf("\"", start);
        if (end == -1)
            return null;

        return historyJson.Substring(start, end - start);
    }

    public static string TryGetSceneNameForSlot(int slotIndex1Based, string keyPrefix = DefaultSlotKeyPrefix)
    {
        string key = keyPrefix + slotIndex1Based;
        return TryParseSceneNameFromHistoryJson(ReadHistoryRaw(key));
    }
}

/// <summary>
/// Fungus SaveHistory JSON에서 마지막 세이브 포인트 요약(씬명, 설명)을 뽑습니다.
/// (<see cref="FungusSaveStorage"/>와 같은 파일에 두어 Unity가 스크립트를 안정적으로 컴파일하도록 합니다.)
/// </summary>
public static class FungusSaveSlotSummary
{
    static readonly Regex SceneRx = new Regex("\"sceneName\"\\s*:\\s*\"([^\"]*)\"", RegexOptions.Compiled);
    static readonly Regex DescRx = new Regex("\"savePointDescription\"\\s*:\\s*\"([^\"]*)\"", RegexOptions.Compiled);

    public static bool TryReadSlotSummary(int slotIndex1Based, string keyPrefix, out string sceneName, out string description)
    {
        sceneName = null;
        description = null;
        string key = SaveSlotManager.SlotDataKey(slotIndex1Based, keyPrefix);
        string raw = FungusSaveStorage.ReadHistoryRaw(key);
        if (string.IsNullOrEmpty(raw))
            return false;

        TryParseFromHistoryJson(raw, out sceneName, out description);
        return !string.IsNullOrEmpty(sceneName) || !string.IsNullOrEmpty(description);
    }

    public static void TryParseFromHistoryJson(string historyJson, out string sceneName, out string description)
    {
        sceneName = null;
        description = null;
        if (string.IsNullOrEmpty(historyJson))
            return;

        MatchCollection sm = SceneRx.Matches(historyJson);
        if (sm.Count > 0)
            sceneName = sm[sm.Count - 1].Groups[1].Value;

        MatchCollection dm = DescRx.Matches(historyJson);
        if (dm.Count > 0)
            description = dm[dm.Count - 1].Groups[1].Value;
    }

    public static string GetThumbnailPath(int slot, string keyPrefix)
    {
        return Application.persistentDataPath + "/FungusSaves/" + keyPrefix + slot + "_thumb.png";
    }

    public static bool ThumbnailExists(int slot, string keyPrefix)
    {
        return File.Exists(GetThumbnailPath(slot, keyPrefix));
    }
}
