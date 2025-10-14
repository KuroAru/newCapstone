using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIClickDebugger : MonoBehaviour
{
    void Update()
    {
        // 마우스 왼쪽 버튼을 클릭했을 때만 실행
        if (Input.GetMouseButtonDown(0))
        {
            // --- 여기가 핵심 ---
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            // --- 여기까지 ---

            if (results.Count > 0)
            {
                Debug.Log("===== UI 클릭 진단 시작 =====");
                // 감지된 모든 UI 요소의 이름을 순서대로 출력
                for (int i = 0; i < results.Count; i++)
                {
                    Debug.Log((i + 1) + "번째: " + results[i].gameObject.name, results[i].gameObject);
                }
                Debug.Log("===== UI 클릭 진단 종료 =====");
            }
        }
    }
}