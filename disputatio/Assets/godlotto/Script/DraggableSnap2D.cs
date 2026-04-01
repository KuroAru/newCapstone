using System.Collections.Generic;
using UnityEngine;
using Fungus;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableSnap2D : MonoBehaviour
{
    public SnapKind kind = SnapKind.Any;
    public bool lockAfterSnap = true;

    [Header("Fungus 연동 (선택)")]
    public Flowchart targetFlowchart;
    public string fungusVarName = "";

    [Header("스프라이트 앞·뒤 (목마 등 겹침)")]
    [Tooltip("같은 Sorting Layer끼리 월드 Y가 작을수록(화면 아래·가까운 쪽) 더 앞에 그립니다.")]
    [SerializeField] bool syncSpriteDepthByWorldY = true;

    [Tooltip("Y가 작을수록 앞이 아니라 뒤로 취급하려면 켭니다.")]
    [SerializeField] bool invertWorldYDepth = false;

    [HideInInspector] public bool isSnapped = false;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Vector3 originalPos;

    private SpriteRenderer sr;
    private const string SnapSaveKeyPrefix = "SnapState_";

    private const int DepthOrderStep = 5;

#if UNITY_EDITOR
    private static bool sClearedOnceThisPlay = false;   // ← 에디터 Play 세션 동안 단 1회만 초기화
#endif

    void Awake()
    {
#if UNITY_EDITOR
        if (!sClearedOnceThisPlay)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            sClearedOnceThisPlay = true;
            Debug.Log("[Editor Play] PlayerPrefs cleared (once per play session).");
        }
#endif
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        sr = GetComponent<SpriteRenderer>();

        // 저장된 스냅 상태 복원
        string key = SnapSaveKeyPrefix + gameObject.name;
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            var target = FindSnapTargetForKind();
            if (target != null)
            {
                transform.position = target.transform.position;
                isSnapped = true;
                target.occupied = true;

                if (lockAfterSnap)
                {
                    var col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }

                if (targetFlowchart && !string.IsNullOrEmpty(fungusVarName))
                    targetFlowchart.SetBooleanVariable(fungusVarName, true);
            }
        }
    }

    public void OnSnappedTo(SnapTarget target)
    {
        isSnapped = true;
        target.occupied = true;
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(target.transform.position);

        // 저장
        string key = SnapSaveKeyPrefix + gameObject.name;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        if (targetFlowchart && !string.IsNullOrEmpty(fungusVarName))
            targetFlowchart.SetBooleanVariable(fungusVarName, true);

        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.9f);

        if (lockAfterSnap)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    public void ResetToOrigin()
    {
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(originalPos);
    }

    private SnapTarget FindSnapTargetForKind()
    {
        var all = FindObjectsOfType<SnapTarget>();
        foreach (var t in all)
            if (t.acceptKind == kind) return t;
        return null;
    }

    /// <summary>
    /// 같은 Sorting Layer를 쓰는 <see cref="DraggableSnap2D"/>끼리 묶어,
    /// 월드 Y에 따라 sortingOrder를 재배치합니다. (낮은 Y = 앞, 기본값)
    /// </summary>
    public static void RefreshSpriteDepthByWorldY()
    {
        var all = Object.FindObjectsOfType<DraggableSnap2D>();
        if (all == null || all.Length == 0) return;

        var groups = new Dictionary<int, List<DraggableSnap2D>>();
        foreach (var d in all)
        {
            if (d == null || !d.syncSpriteDepthByWorldY || !d.isActiveAndEnabled) continue;
            var r = d.GetComponent<SpriteRenderer>();
            if (r == null) continue;
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
            if (list == null || list.Count <= 1) continue;

            list.Sort((a, b) =>
            {
                float fa = a.invertWorldYDepth ? -a.transform.position.y : a.transform.position.y;
                float fb = b.invertWorldYDepth ? -b.transform.position.y : b.transform.position.y;
                int c = fa.CompareTo(fb);
                if (c != 0) return c;
                return string.CompareOrdinal(a.gameObject.name, b.gameObject.name);
            });

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
                if (r == null) continue;
                r.sortingOrder = minOrder + (n - 1 - i) * DepthOrderStep;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isSnapped ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}
