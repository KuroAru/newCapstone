using UnityEngine;
using Fungus;

/// <summary>
/// 씬에 Fungus <see cref="SaveManager"/> / 프로젝트 <see cref="SaveSlotManager"/>가 없으면
/// DontDestroyOnLoad 오브젝트를 만들어 자동 배치합니다. 씬에 수동으로 넣은 경우는 건드리지 않습니다.
/// </summary>
static class FungusSaveSystemBootstrap
{
    const string AutoSaveSlotObjectName = "SaveSlotManager (Auto)";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterSceneLoad()
    {
        EnsureSaveStack();
    }

    /// <summary>
    /// UI 등에서 필요 시 즉시 보장할 때 호출합니다(이미 있으면 아무 것도 하지 않음).
    /// </summary>
    public static void EnsureSaveStack()
    {
        EnsureFungusSaveManagerPresent();
        EnsureSaveSlotManagerPresent();
    }

    static void EnsureFungusSaveManagerPresent()
    {
        FungusManager[] fungusManagers = Object.FindObjectsByType<FungusManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (fungusManagers != null && fungusManagers.Length > 0)
            return;

        SaveManager[] saveManagers = Object.FindObjectsByType<SaveManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (saveManagers != null && saveManagers.Length > 0)
            return;

        _ = FungusManager.Instance;
    }

    static void EnsureSaveSlotManagerPresent()
    {
        SaveSlotManager[] slots = Object.FindObjectsByType<SaveSlotManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (slots != null && slots.Length > 0)
            return;

        GameObject go = new GameObject(AutoSaveSlotObjectName);
        Object.DontDestroyOnLoad(go);
        go.AddComponent<SaveSlotManager>();
    }
}
