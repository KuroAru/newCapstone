using UnityEngine;

/// <summary>
/// 월드(Transform) 백 버튼을 UI <see cref="FirstFloorForkReturnButton"/>과 동일한
/// 기준(참조 해상도 1920×1080, 좌상단에서 (32,32) 안쪽)에 맞춥니다.
/// </summary>
[DefaultExecutionOrder(-50)]
public class ScreenReferenceCornerAlign : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] Vector2 insetFromTopLeft = new Vector2(32f, 32f);
    [SerializeField] float zOffset;

    void Awake()
    {
        Apply();
    }

    void Apply()
    {
        var c = cam != null ? cam : Camera.main;
        if (c == null || !c.orthographic)
            return;

        float halfH = c.orthographicSize;
        float halfW = halfH * c.aspect;
        Vector3 cp = c.transform.position;

        float wuPerPixX = (2f * halfW) / referenceResolution.x;
        float wuPerPixY = (2f * halfH) / referenceResolution.y;

        float left = cp.x - halfW;
        float top = cp.y + halfH;

        float x = left + insetFromTopLeft.x * wuPerPixX;
        float y = top - insetFromTopLeft.y * wuPerPixY;
        float z = cp.z + zOffset;

        transform.position = new Vector3(x, y, z);
    }
}
