using UnityEngine;

/// <summary>
/// 전역에서 마우스 입력을 수집해
/// - SnapTarget 레이어를 완전히 무시하고
/// - 마우스 아래 '드래그 가능한 것'만 집어서
/// - 드래그/스냅을 수행하는 매니저.
/// 씬에 1개만 두세요.
/// </summary>
public class DragManager2D : MonoBehaviour
{
    [Header("스냅 탐색 반경(드롭 시 근처 타겟 보정)")]
    public float snapSearchRadius = 100f;

    [Tooltip("2D 드래그용 월드 평면의 Z (씬에 맞게 조정). ChildRoom 등 픽셀 좌표·z=0 스프라이트 기준.")]
    [SerializeField] private float dragPlaneWorldZ = 0f;

    private Camera cam;
    private DraggableSnap2D current;
    private Vector3 grabOffset;
    private Vector3 originalPos;

    // SnapTarget 레이어 마스크 (에디터에서 레이어 이름만 맞춰두면 됨)
    private int layerMaskWithoutSnapTargets;

    void Awake()
    {
        cam = Camera.main;

        // SnapTarget 레이어를 완전히 제외한 마스크 구성
        int snapTargetMask = LayerMask.GetMask("SnapTarget"); // ← 레이어 이름만 "SnapTarget"으로 지정하세요.
        layerMaskWithoutSnapTargets = ~snapTargetMask;
    }

    void Start()
    {
        DraggableSnap2D.RefreshSpriteDepthByWorldY();
    }

    /// <summary>
    /// Input.mousePosition.z == 0인 채로 ScreenToWorldPoint를 쓰면 정사영 카메라에서 깊이가 어긋나
    /// OverlapPoint가 빈 배열을 반환할 수 있음. 카메라 레이와 월드 Z 평면 교차로 보정한다.
    /// </summary>
    private Vector3 MouseWorldOnDragPlane()
    {
        if (cam == null)
            return Vector3.zero;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 planePoint = new Vector3(0f, 0f, dragPlaneWorldZ);
        Plane plane = new Plane(-cam.transform.forward, planePoint);

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        Vector3 fallback = Input.mousePosition;
        fallback.z = Mathf.Abs(cam.transform.position.z - dragPlaneWorldZ);
        return cam.ScreenToWorldPoint(fallback);
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (Input.GetMouseButtonDown(0)) TryBeginDrag();
        if (Input.GetMouseButton(0)   ) OnDrag();
        if (Input.GetMouseButtonUp(0) ) EndDrag();
    }

    void TryBeginDrag()
    {
        if (current != null) return; // 이미 드래그 중

        Vector3 mw = MouseWorldOnDragPlane();

        // SnapTarget 레이어를 '완전히 제외'하고 포인트 히트
        Collider2D[] hits = Physics2D.OverlapPointAll(mw, layerMaskWithoutSnapTargets);

        if (hits == null || hits.Length == 0) return;

        // '드래그 가능한 것'만 후보로 필터링 + 화면상 가장 앞(정렬 우선) 순서로 선택
        Collider2D bestCol = null;
        float bestOrder = float.NegativeInfinity;

        foreach (var h in hits)
        {
            var drag = h.GetComponent<DraggableSnap2D>();
            if (drag == null || drag.isSnapped && drag.lockAfterSnap) continue;

            float order = -h.transform.position.z; // 기본값: 카메라에 가까울수록 앞

            var sr = h.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                int layerOrder = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
                order = layerOrder * 10000f + sr.sortingOrder;
            }

            if (order > bestOrder)
            {
                bestOrder = order;
                bestCol = h;
            }
        }

        if (bestCol == null) return;

        current = bestCol.GetComponent<DraggableSnap2D>();
        if (current == null) return;

        originalPos = current.transform.position;

        Vector3 mouseWorld = MouseWorldOnDragPlane();
        grabOffset = current.transform.position - mouseWorld;
        grabOffset.z = 0f;
    }

    void OnDrag()
    {
        if (current == null) return;

        Vector3 mouseWorld = MouseWorldOnDragPlane();
        Vector3 targetPos = mouseWorld + grabOffset;
        targetPos.z = current.transform.position.z;

        current.rb.MovePosition(targetPos);
        DraggableSnap2D.RefreshSpriteDepthByWorldY();
    }

    void EndDrag()
    {
        if (current == null) return;

        // 드롭 시점에서 SnapTarget 레이어 콜라이더만 검사 (다른 레이어 오브젝트와의 오탐 방지)
        int snapMask = LayerMask.GetMask("SnapTarget");
        var hits = snapMask != 0
            ? Physics2D.OverlapCircleAll(current.transform.position, snapSearchRadius, snapMask)
            : Physics2D.OverlapCircleAll(current.transform.position, snapSearchRadius);
        SnapTarget bestTarget = null;
        float bestDistSq = float.MaxValue;
        Vector2 p = current.transform.position;

        foreach (var h in hits)
        {
            var t = h.GetComponent<SnapTarget>();
            if (t == null || !t.CanAccept(current)) continue;

            float d = ((Vector2)t.transform.position - p).sqrMagnitude;
            if (d < bestDistSq)
            {
                bestDistSq = d;
                bestTarget = t;
            }
        }

        if (bestTarget != null)
        {
            // 🔴 이전처럼 여기서 위치/변수/잠금 직접 처리하지 말고…
            // ✅ 한 줄로 위임: DraggableSnap2D가 모든 후속 처리(Flowchart true 포함) 수행
            current.OnSnappedTo(bestTarget);
        }
        else
        {
            // 실패 시 원위치 복귀
            current.rb.linearVelocity = Vector2.zero;
            current.rb.MovePosition(originalPos);
        }

        DraggableSnap2D.RefreshSpriteDepthByWorldY();
        current = null;
    }

    void OnDrawGizmosSelected()
    {
        if (current != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(current.transform.position, snapSearchRadius);
        }
    }
}
