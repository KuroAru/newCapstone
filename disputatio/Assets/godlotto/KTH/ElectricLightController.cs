using UnityEngine;
using UnityEngine.Rendering.Universal;
using Fungus; 

public class ElectricLightController : MonoBehaviour
{
    [Header("펑거스 연결")]
    [Tooltip("ElectricOn 변수를 관리하는 플로우차트 오브젝트를 연결하세요.")]
    public Flowchart targetFlowchart;
    [Tooltip("감지할 펑거스 변수 이름")]
    public string variableName = "ElectricOn";

    [Header("조명 설정")]
    [Tooltip("ElectricOn 상태에 따라 켜고 꺼질 2D 조명 오브젝트를 연결하세요.")]
    public Light2D targetLight;

    // 이전 상태를 기억하여 상태가 변했을 때만 작동하도록 하는 변수
    private bool previousElectricState = false;

    private void Start()
    {
        if (targetFlowchart != null && targetLight != null)
        {
            // 씬 시작 시 펑거스 변수의 초기 상태를 읽어와 조명 상태를 맞춥니다.
            previousElectricState = targetFlowchart.GetBooleanVariable(variableName);
            targetLight.enabled = previousElectricState;
        }
    }

    private void Update()
    {
        // 연결이 누락되었다면 작동하지 않음
        if (targetFlowchart == null || targetLight == null) return;

        // 매 프레임 펑거스 변수의 현재 상태를 읽어옵니다.
        bool currentElectricState = targetFlowchart.GetBooleanVariable(variableName);

        // 현재 상태가 이전 상태와 다를 때(즉, True->False 또는 False->True로 변할 때)만 실행합니다.
        if (currentElectricState != previousElectricState)
        {
            // 조명을 현재 변수 상태(True면 켜짐, False면 꺼짐)와 똑같이 맞춥니다.
            targetLight.enabled = currentElectricState;
            
            // 다음 비교를 위해 이전 상태 값을 갱신합니다.
            previousElectricState = currentElectricState;
        }
    }
}