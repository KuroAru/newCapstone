#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드 클릭이 UI에 먹히는지 점검할 때 사용합니다. 전체 화면 근처 크기의 Graphic.raycastTarget을 로그합니다.
/// </summary>
internal static class UIRaycastDiagnostics
{
    [MenuItem("Tools/Godlotto/Log Large UI Raycast Targets")]
    static void LogLargeRaycastTargets()
    {
        const float minFraction = 0.85f;
        var sb = new StringBuilder();
        int count = 0;

        foreach (var g in Object.FindObjectsOfType<Graphic>(true))
        {
            if (!g.raycastTarget || !g.isActiveAndEnabled)
                continue;

            var rt = g.rectTransform;
            if (rt == null)
                continue;

            var r = rt.rect;
            if (r.width < Screen.width * minFraction && r.height < Screen.height * minFraction)
                continue;

            count++;
            sb.AppendLine($"{GetHierarchyPath(g.transform)}  (≈{r.width:F0}x{r.height:F0})");
        }

        if (count == 0)
        {
            Debug.Log("[UIRaycastDiagnostics] raycastTarget이 켜진 대형(화면의 " + minFraction * 100f +
            "% 이상) Graphic이 없습니다.");
            return;
        }

        Debug.LogWarning($"[UIRaycastDiagnostics] 대형 raycastTarget {count}개 — 월드 클릭을 가릴 수 있습니다.\n{sb}");
    }

    static string GetHierarchyPath(Transform t)
    {
        if (t == null)
            return "";

        var stack = new System.Collections.Generic.Stack<string>();
        for (var p = t; p != null; p = p.parent)
            stack.Push(p.name);

        var sb = new StringBuilder();
        while (stack.Count > 0)
        {
            if (sb.Length > 0)
                sb.Append('/');
            sb.Append(stack.Pop());
        }

        return sb.ToString();
    }
}
#endif
