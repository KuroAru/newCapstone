using UnityEngine;

public class MiniGameEnemy : MonoBehaviour
{
    public float moveSpeed = 150f;
    private Transform player;
    
    // 가루 효과 컴포넌트 및 상태 변수
    private SpriteDissolver dissolver;
    private SpriteRenderer sr; // 방향 전환을 위한 컴포넌트 변수
    private bool isDead = false;

    void Start()
    {
        // [필수 추가] 방향 전환을 위해 본인의 SpriteRenderer를 가져옵니다.
        sr = GetComponent<SpriteRenderer>();

        // 플레이어 소환 방식에 따라 태그를 찾습니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // 동일한 오브젝트에 붙어있는 소멸 스크립트를 가져옵니다.
        dissolver = GetComponent<SpriteDissolver>();
    }

    void Update()
    {
        // 죽지 않았고 추격할 플레이어가 있을 때만 실행합니다.
        if (player != null && !isDead)
        {
            // 1. 추격 로직
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            // 2. 방향 전환(Flip) 로직
            // 플레이어가 적보다 왼쪽에 있으면(x값이 작으면) 이미지를 뒤집습니다(true).
            // 이미지의 기본 방향이 오른쪽을 보고 있을 때 기준입니다.
            if (sr != null)
            {
                sr.flipX = player.position.x > transform.position.x;
            }
        }
    }

    // Manager에서 적을 생성할 때 타겟을 직접 넣어줄 수 있는 함수입니다.
    public void SetTarget(Transform target)
    {
        player = target;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // 설정된 공격 태그에 닿으면 사망 시퀀스를 시작합니다.
        if (other.CompareTag("Projectile") || other.CompareTag("Melee") || other.CompareTag("PlayerAttack"))
        {
            if (other.CompareTag("Projectile")) Destroy(other.gameObject);
            
            StartDeathSequence();
        }
    }

    void StartDeathSequence()
    {
        isDead = true;

        // 1. 더 이상 플레이어와 부딪히지 않도록 콜라이더를 끕니다.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. 가루가 되어 사라지는 효과 실행
        if (dissolver != null)
        {
            dissolver.StartDissolve();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}