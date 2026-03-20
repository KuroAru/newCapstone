using UnityEngine;
using UnityEngine.AI;

public class PlayerPathLine : MonoBehaviour
{
    private NavMeshAgent agent;
    private LineRenderer pathLine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        pathLine = GetComponent<LineRenderer>();

        // 선 설정 (스크립트에서도 가능하지만 인스펙터에서 하면 더 편합니다)
        pathLine.startWidth = 0.1f;
        pathLine.endWidth = 0.1f;
        pathLine.positionCount = 0;
        
        // 2D에서 선이 캐릭터나 배경 뒤로 숨지 않도록 설정
        pathLine.useWorldSpace = true;
        pathLine.sortingOrder = 5; 
    }

    void Update()
    {
        // 1. 마우스 우클릭으로 이동 지점 설정
        if (Input.GetMouseButtonDown(1))
        {
            SetDestinationToMouse();
        }

        // 2. 실시간으로 경로 선 업데이트
        DrawPath();
    }

    void SetDestinationToMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        agent.SetDestination(mousePos);
    }

    void DrawPath()
    {
        // 경로가 없거나 도착했다면 선을 지움
        if (agent.path == null || agent.path.corners.Length < 2)
        {
            pathLine.positionCount = 0;
            return;
        }

        // NavMeshAgent가 계산한 경로의 좌표들을 LineRenderer에 전달
        pathLine.positionCount = agent.path.corners.Length;
        
        for (int i = 0; i < agent.path.corners.Length; i++)
        {
            Vector3 point = agent.path.corners[i];
            point.z = -0.1f; // 2D 평면보다 살짝 앞에 그려지도록 설정
            pathLine.SetPosition(i, point);
        }
    }
}