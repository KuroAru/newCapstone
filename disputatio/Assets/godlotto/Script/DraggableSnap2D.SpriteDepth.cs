using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드 Y 기준으로 같은 Sorting Layer끼리 스프라이트 sortingOrder를 맞춥니다.
/// </summary>
public partial class DraggableSnap2D
{
    [Header("스프라이트 앞·뒤 (목마 등 겹침)")]
    [Tooltip("같은 Sorting Layer끼리 월드 Y가 작을수록(화면 아래·가까운 쪽) 더 앞에 그립니다.")]
    [SerializeField] private bool syncSpriteDepthByWorldY = true;

    [Tooltip("Y가 작을수록 앞이 아니라 뒤로 취급하려면 켭니다.")]
    [SerializeField] private bool invertWorldYDepth = false;

    private const int DepthOrderStep = 5;

    /// <summary>
    /// 같은 Sorting Layer를 쓰는 <see cref="DraggableSnap2D"/>끼리 묶어,
    /// 월드 Y에 따라 sortingOrder를 재배치합니다. (낮은 Y = 앞, 기본값)
    /// </summary>
    public static void RefreshSpriteDepthByWorldY()
    {
        var all = Object.FindObjectsOfType<DraggableSnap2D>();
        if (all == null || all.Length == 0)
            return;

        var groups = new Dictionary<int, List<DraggableSnap2D>>();
        foreach (var d in all)
        {
            if (d == null || !d.syncSpriteDepthByWorldY || !d.isActiveAndEnabled)
                continue;
            var r = d.GetComponent<SpriteRenderer>();
            if (r == null)
                continue;
            int layerId = r.sortingLayerID;
            if (!groups.TryGetValue(layerId, out var list))
            {
                list = new List<DraggableSnap2D>();
                groups[layerId] = list;
            }
            list.Add(d);
        }

        foreach (var kv in groups)
        {
            var list = kv.Value;
            if (list == null || list.Count <= 1)
                continue;

            list.Sort(CompareDepthPeers);

            int minOrder = int.MaxValue;
            foreach (var d in list)
            {
                var r = d.GetComponent<SpriteRenderer>();
                if (r != null && r.sortingOrder < minOrder)
                    minOrder = r.sortingOrder;
            }

            if (minOrder == int.MaxValue)
                minOrder = 0;

            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                var r = list[i].GetComponent<SpriteRenderer>();
                if (r == null)
                    continue;
                r.sortingOrder = minOrder + (n - 1 - i) * DepthOrderStep;
            }
        }
    }

    private static int CompareDepthPeers(DraggableSnap2D a, DraggableSnap2D b)
    {
        float fa = a.invertWorldYDepth ? -a.transform.position.y : a.transform.position.y;
        float fb = b.invertWorldYDepth ? -b.transform.position.y : b.transform.position.y;
        int c = fa.CompareTo(fb);
        if (c != 0)
            return c;
        return string.CompareOrdinal(a.gameObject.name, b.gameObject.name);
    }
}
