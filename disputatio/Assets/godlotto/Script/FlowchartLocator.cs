using Fungus;
using UnityEngine;

/// <summary>
/// "Variablemanager" 글로벌 Flowchart를 찾아 반환하는 공유 유틸.
/// 여러 스크립트에서 <c>GameObject.Find("Variablemanager")</c>를 반복하지 않도록 합니다.
/// </summary>
public static class FlowchartLocator
{
    private const string GlobalFlowchartName = "Variablemanager";

    /// <summary>
    /// 씬에서 "Variablemanager" 이름의 GameObject를 찾아 Flowchart를 반환합니다.
    /// 없으면 null을 반환하고 경고 로그를 남깁니다.
    /// </summary>
    public static Flowchart Find()
    {
        GameObject go = GameObject.Find(GlobalFlowchartName);
        if (go == null)
        {
            Debug.LogWarning($"[FlowchartLocator] '{GlobalFlowchartName}' GameObject를 찾을 수 없습니다.");
            return null;
        }

        Flowchart fc = go.GetComponent<Flowchart>();
        if (fc == null)
            Debug.LogWarning($"[FlowchartLocator] '{GlobalFlowchartName}'에 Flowchart 컴포넌트가 없습니다.");

        return fc;
    }

    /// <summary>
    /// 인스펙터에 연결된 Flowchart가 있으면 그것을, 없으면 글로벌 Variablemanager Flowchart를 반환합니다.
    /// </summary>
    public static Flowchart Resolve(Flowchart preferred)
    {
        return preferred != null ? preferred : Find();
    }
}
