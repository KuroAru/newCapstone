using UnityEngine;
using UnityEngine.AI;

public class PlayerController2D : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform targetObject; // 현재 목표로 하는 상호작용 오브젝트
    private bool isInteracted = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // 2D 환경 설정: 회전 제한 및 평면 이동 고정
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        // 1. 마우스 우클릭으로 이동 지점 설정
        if (Input.GetMouseButtonDown(1))
        {
            SetDestinationToMouse();
        }

        // 2. 상호작용 체크 (목표 오브젝트가 있고, 아직 상호작용 전일 때)
        if (targetObject != null && !isInteracted)
        {
            CheckArrival();
        }
    }

    void SetDestinationToMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2D이므로 z축 고정

        // 클릭 위치에 상호작용 가능한 오브젝트가 있는지 확인
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        
        if (hit.collider != null && hit.collider.CompareTag("Interactable"))
        {
            targetObject = hit.transform;
            isInteracted = false; // 새로운 목표 설정 시 초기화
            agent.SetDestination(targetObject.position);
        }
        else
        {
            targetObject = null;
            agent.SetDestination(mousePos);
        }
    }

    void CheckArrival()
    {
        // 남은 거리가 에이전트의 정지 거리보다 작으면 도착으로 간주
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Interact();
        }
    }

    void Interact()
    {
        isInteracted = true;
        GameLog.Log($"<color=yellow>[상호작용]</color> {targetObject.name}에 도달했습니다!");
        
        // 여기서 실제로 띄울 UI 창이나 이벤트를 호출하면 됩니다.
        // 예: UIManager.Instance.ShowDialogue(targetObject.name);
    }
}