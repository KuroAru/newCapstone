using UnityEngine;
using UnityEngine.EventSystems;
using Fungus;

namespace Godlotto.FungusIntegration
{
    /// <summary>
    /// 연타·중복 포인터 이벤트로 ObjectClicked가 여러 번 큐에 쌓이는 것을 줄입니다.
    /// Fungus <see cref="Clickable2D"/>는 레거시 OnMouseDown에서
    /// <see cref="EventSystem.IsPointerOverGameObject"/>가 true면 무조건 막는데,
    /// 전체 화면 UI(투명 Image 등)만 있어도 true가 되어 문 클릭이 전부 씹힐 수 있습니다.
    /// 마우스 아래 2D 물리에 이 오브젝트의 콜라이더가 있으면 그때는 클릭을 허용합니다.
    /// </summary>
    public class GuardedClickable2D : Clickable2D
    {
        [SerializeField] float cooldownSeconds = 0.35f;

        [Tooltip("true면 포인터가 UI 위라고 나와도, 월드 2D 콜라이더가 마우스 아래에 있으면 클릭을 받습니다.")]
        [SerializeField] bool allowClickWhenWorldColliderUnderPointer = true;

        float lastClickUnscaledTime = float.NegativeInfinity;

        /// <summary>
        /// 베이스 <see cref="Clickable2D.OnMouseDown"/>를 대체합니다(가상 메서드이므로 Unity는 이 구현만 호출).
        /// </summary>
        protected override void OnMouseDown()
        {
            if (useEventSystem)
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                if (!allowClickWhenWorldColliderUnderPointer || !IsOurCollider2DUnderMouse())
                    return;
            }

            DoPointerClick();
        }

        bool IsOurCollider2DUnderMouse()
        {
            if (GetComponent<Collider2D>() == null && GetComponentInChildren<Collider2D>(true) == null)
                return false;

            Vector2 world = ScreenToWorldPoint2D(Input.mousePosition);
            foreach (Collider2D c in Physics2D.OverlapPointAll(world))
            {
                if (c.GetComponentInParent<Clickable2D>() == this)
                    return true;
            }

            return false;
        }

        static Vector2 ScreenToWorldPoint2D(Vector3 screenPosition)
        {
            Camera cam = Camera.main;
            if (cam == null)
                return Vector2.zero;

            Vector3 p = screenPosition;
            p.z = Mathf.Abs(cam.transform.position.z);
            return cam.ScreenToWorldPoint(p);
        }

        protected override void DoPointerClick()
        {
            float t = Time.unscaledTime;
            if (t - lastClickUnscaledTime < cooldownSeconds)
                return;

            lastClickUnscaledTime = t;
            base.DoPointerClick();
        }
    }
}
