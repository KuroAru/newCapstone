using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tag가 Enemy인 루트 오브젝트 트리에서 SpriteRenderer·루트 Canvas의 2D 정렬을
/// 프로젝트의 "Enemy" Sorting Layer로 올려 다른 오브젝트 위에 그리도록 합니다.
/// </summary>
public static class EnemySortingUtility
{
    public const string EnemySortingLayerName = "Enemy";

    /// <summary>같은 Enemy 트리 안에서 기존 Order in Layer 상대값을 유지할 때 더할 기본값.</summary>
    public const int DefaultSpriteOrderBase = 0;

    /// <summary>루트 Canvas를 스프라이트보다 앞에 두기 위한 추가 오프셋.</summary>
    public const int CanvasOrderBoost = 10000;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSceneLoaded()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyAfterFirstSceneLoad()
    {
        ApplyToAllTaggedEnemyRoots();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToAllTaggedEnemyRoots();
    }

    /// <summary>활성 오브젝트 중 태그가 Enemy인 루트마다 정렬을 적용합니다.</summary>
    public static void ApplyToAllTaggedEnemyRoots()
    {
        GameObject[] roots;
        try
        {
            roots = GameObject.FindGameObjectsWithTag("Enemy");
        }
        catch (UnityException)
        {
            return;
        }

        if (roots == null || roots.Length == 0)
            return;

        foreach (var go in roots)
        {
            if (go != null)
                ApplyToRoot(go);
        }
    }

    /// <summary>런타임 스폰된 적 등 한 루트에 대해 호출합니다.</summary>
    public static void ApplyToRoot(GameObject root)
    {
        if (root == null)
            return;

        if (!TryGetEnemySortingLayerId(out int layerId))
        {
            Debug.LogWarning(
                "[EnemySortingUtility] Sorting Layer 'Enemy' 없음. Project Settings > Tags and Layers > Sorting Layers에 추가했는지 확인하세요.");
            return;
        }

        var sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in sprites)
        {
            if (sr == null)
                continue;
            sr.sortingLayerID = layerId;
            sr.sortingOrder = DefaultSpriteOrderBase + sr.sortingOrder;
        }

        var canvases = root.GetComponentsInChildren<Canvas>(true);
        foreach (var canvas in canvases)
        {
            if (canvas == null || !canvas.isRootCanvas)
                continue;
            canvas.overrideSorting = true;
            canvas.sortingLayerID = layerId;
            canvas.sortingOrder = DefaultSpriteOrderBase + CanvasOrderBoost + canvas.sortingOrder;
        }
    }

    private static bool TryGetEnemySortingLayerId(out int layerId)
    {
        foreach (var layer in SortingLayer.layers)
        {
            if (layer.name == EnemySortingLayerName)
            {
                layerId = layer.id;
                return true;
            }
        }

        layerId = 0;
        return false;
    }
}
