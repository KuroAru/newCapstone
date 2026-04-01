using UnityEngine;
using Fungus;

/// <summary>
/// 애니메이션 이벤트에서 호출해 Fungus 메시지를 보냅니다.
/// </summary>
public class AnimEndToFungus : MonoBehaviour
{
    [Tooltip("비우면 FlowchartLocator(Variablemanager)를 사용합니다.")]
    [SerializeField] private Flowchart flowchart;
    public string messageName = "AnimFinished";

    public void OnAnimEnd()
    {
        Flowchart fc = FlowchartLocator.Resolve(flowchart);
        if (fc != null)
            fc.SendFungusMessage(messageName);
    }
}
