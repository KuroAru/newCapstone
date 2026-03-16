using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimationController2D : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    [Header("Threshold")]
    public float movementThreshold = 0.1f; // 이 속도보다 느리면 정지로 판단

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        Vector3 velocity = agent.velocity;
        
        // 1. 현재 속도의 절대값을 비교 (x축이 큰지, y/z축이 큰지)
        // 만약 NavMesh가 90도 돌아가 있다면 velocity.y 대신 velocity.z를 써야 할 수도 있습니다.
        float absX = Mathf.Abs(velocity.x);
        float absY = Mathf.Abs(velocity.y); // 평면 설정에 따라 z로 변경 가능

        // 이동 중인지 확인
        if (velocity.sqrMagnitude > movementThreshold * movementThreshold)
        {
            anim.SetBool("isMoving", true);

            if (absX > absY)
            {
                // X축 이동이 더 빠름 -> 좌우 애니메이션
                if (velocity.x > 0) SetDir(1); // Right
                else SetDir(3);                // Left
            }
            else
            {
                // Y(또는 Z)축 이동이 더 빠름 -> 상하 애니메이션
                if (velocity.y > 0) SetDir(0); // Up
                else SetDir(2);                // Down
            }
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    // Animator의 'Dir' 파라미터(Integer)를 변경하는 함수
    void SetDir(int directionIndex)
    {
        // 0: Up, 1: Right, 2: Down, 3: Left (임의 설정)
        anim.SetInteger("Dir", directionIndex);
    }
}