using System.Collections.Generic;
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

    [Header("다중 조명 설정")]
    [Tooltip("ElectricOn 상태에 따라 동시에 켜고 꺼질 2D 조명 오브젝트들을 모두 연결하세요.")]
    public List<Light2D> targetLights = new List<Light2D>();

    [Header("다중 스프라이트 설정")]
    [Tooltip("ElectricOn 상태에 따라 동시에 표시하거나 숨길 스프라이트 렌더러들을 모두 연결하세요.")]
    public List<SpriteRenderer> targetSprites = new List<SpriteRenderer>();

    // 이전 상태를 기억하여 상태가 변했을 때만 작동하도록 하는 변수
    private bool previousElectricState = false;

    private void Start()
    {
        if (targetFlowchart != null)
        {
            // 씬 시작 시 펑거스 변수의 초기 상태를 읽어옵니다.
            previousElectricState = targetFlowchart.GetBooleanVariable(variableName);

            // 리스트에 등록된 모든 조명의 초기 상태를 동기화합니다.
            foreach (Light2D light in targetLights)
            {
                if (light != null)
                {
                    light.enabled = previousElectricState;
                }
            }

            // 리스트에 등록된 모든 스프라이트의 초기 상태를 동기화합니다.
            foreach (SpriteRenderer sprite in targetSprites)
            {
                if (sprite != null)
                {
                    sprite.enabled = previousElectricState;
                }
            }
        }
    }

    private void Update()
    {
        // 연결이 누락되었거나 관리할 오브젝트가 하나도 없다면 작동하지 않습니다.
        if (targetFlowchart == null) return;
        if (targetLights.Count == 0 && targetSprites.Count == 0) return;

        // 매 프레임 펑거스 변수의 현재 상태를 읽어옵니다.
        bool currentElectricState = targetFlowchart.GetBooleanVariable(variableName);

        // 현재 상태가 이전 상태와 다를 때만 실행합니다.
        if (currentElectricState != previousElectricState)
        {
            // 리스트에 있는 모든 조명을 켜거나 끕니다.
            foreach (Light2D light in targetLights)
            {
                if (light != null)
                {
                    light.enabled = currentElectricState;
                }
            }

            // 리스트에 있는 모든 스프라이트를 켜거나 끕니다.
            foreach (SpriteRenderer sprite in targetSprites)
            {
                if (sprite != null)
                {
                    sprite.enabled = currentElectricState;
                }
            }
            
            // 다음 비교를 위해 이전 상태 값을 갱신합니다.
            previousElectricState = currentElectricState;
        }
    }
}