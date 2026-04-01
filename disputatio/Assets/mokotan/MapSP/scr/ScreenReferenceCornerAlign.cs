using UnityEngine;

/// <summary>
/// 월드(Transform) 백 버튼을 UI <see cref="FirstFloorForkReturnButton"/>과 동일한
/// 기준(참조 해상도 1920×1080, 좌상단에서 (32,32) 안쪽)에 맞춥니다.
/// Screen Space - Camera 캔버스는 오쏘 프러스텀과 월드 범위가 다르므로, 가능하면
/// 해당 캔버스의 월드 코너를 기준으로 배치합니다.
/// </summary>
[DefaultExecutionOrder(-50)]
public class ScreenReferenceCornerAlign : MonoBehaviour
{
    [SerializeField] Camera cam;
    [Tooltip("비우면 씬의 Screen Space Camera 캔버스를 자동 탐색합니다. 여러 개일 때만 지정하세요.")]
    [SerializeField] Canvas alignToCanvas;
    [SerializeField] Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] Vector2 insetFromTopLeft = new Vector2(32f, 32f);
    [SerializeField] float zOffset;

    readonly Vector3[] _canvasCorners = new Vector3[4];

    bool _lateAligned;

    void OnEnable()
    {
        _lateAligned = false;
    }

    void Awake()
    {
        Apply();
    }

    void LateUpdate()
    {
        if (_lateAligned)
            return;
        _lateAligned = true;
        Canvas.ForceUpdateCanvases();
        Apply();
    }

    void Apply()
    {
        var c = cam != null ? cam : Camera.main;
        if (c == null || !c.orthographic)
            return;

        float targetZ = Mathf.Approximately(zOffset, 0f) ? transform.position.z : c.transform.position.z + zOffset;

        if (TryApplyUsingCanvas(targetZ))
            return;

        ApplyUsingOrthographicFrustum(c, targetZ);
    }

    bool TryApplyUsingCanvas(float targetZ)
    {
        Canvas canvas = alignToCanvas;
        if (canvas == null)
        {
            var found = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (var i = 0; i < found.Length; i++)
            {
                var cv = found[i];
                if (cv != null && cv.isActiveAndEnabled && cv.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    canvas = cv;
                    break;
                }
            }
        }

        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceCamera)
            return false;

        var rt = canvas.rootCanvas.transform as RectTransform;
        if (rt == null)
            return false;

        rt.GetWorldCorners(_canvasCorners);
        Vector3 topLeft = _canvasCorners[1];
        Vector3 topRight = _canvasCorners[2];
        Vector3 bottomLeft = _canvasCorners[0];
        Vector3 alongTop = topRight - topLeft;
        Vector3 alongLeftDown = bottomLeft - topLeft;
        if (alongTop.sqrMagnitude < 1e-8f || alongLeftDown.sqrMagnitude < 1e-8f)
            return false;

        Vector3 p = topLeft
            + alongTop * (insetFromTopLeft.x / referenceResolution.x)
            + alongLeftDown * (insetFromTopLeft.y / referenceResolution.y);
        p.z = targetZ;
        transform.position = p;
        return true;
    }

    void ApplyUsingOrthographicFrustum(Camera c, float targetZ)
    {
        float halfH = c.orthographicSize;
        float halfW = halfH * c.aspect;
        Vector3 cp = c.transform.position;

        float wuPerPixX = (2f * halfW) / referenceResolution.x;
        float wuPerPixY = (2f * halfH) / referenceResolution.y;

        float left = cp.x - halfW;
        float top = cp.y + halfH;

        float x = left + insetFromTopLeft.x * wuPerPixX;
        float y = top - insetFromTopLeft.y * wuPerPixY;
        transform.position = new Vector3(x, y, targetZ);
    }
}
