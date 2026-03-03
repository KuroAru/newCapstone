using UnityEngine;
using System.Collections;

public class MiniGamePlayer : MonoBehaviour
{
    public float moveSpeed = 300f;
    // [수정] PPU 1 환경(400x300)에서는 힘이 최소 1000 이상은 되어야 밀려납니다.
    public float knockbackForce = 1500f; 
    
    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f;
    private bool isInvincible = false;
    private bool isKnockedBack = false; // [추가] 넉백 중인지 체크

    [Header("Melee Attack Settings")]
    public GameObject meleeHitboxPrefab;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.5f;

    [Header("Animation Settings")]
    public SpriteRenderer spriteRenderer; 
    public Animator animator; 

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("측면으로 걷는 모션2_0");
        if (controller != null) animator.runtimeAnimatorController = controller;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        UpdateVisuals();

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(PerformMeleeAttack());
        }
    }

    void UpdateVisuals()
    {
        bool isMoving = Mathf.Abs(moveInput.x) > 0 || Mathf.Abs(moveInput.y) > 0;

        // [수정] 공격 중이 아닐 때만 이동 방향에 따라 캐릭터를 반전시킵니다.
        if (!isAttacking)
        {
            if (moveInput.x > 0) spriteRenderer.flipX = false;
            else if (moveInput.x < 0) spriteRenderer.flipX = true;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
            if (isMoving && !animator.GetCurrentAnimatorStateInfo(0).IsName("WalkRight"))
            {
                animator.Play("WalkRight");
            }
        }
    }

    void FixedUpdate()
    {
        // [수정] 넉백 중이 아닐 때만 이동 속도를 제어합니다.
        // 넉백 중일 때는 물리 엔진이 준 힘(AddForce)에 의해 움직이게 내버려 둡니다.
        if (!isKnockedBack)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed * Time.fixedDeltaTime * 50f;
        }
    }

    IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;

        // 마우스 위치 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 attackDir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        // [추가] 공격 방향(마우스 위치)에 따라 캐릭터 반전 설정
        if (attackDir.x > 0) spriteRenderer.flipX = false; // 오른쪽 조준
        else if (attackDir.x < 0) spriteRenderer.flipX = true;  // 왼쪽 조준

        if (animator != null) animator.SetTrigger("Attack");

        if (meleeHitboxPrefab != null)
        {
            float spawnDistance = (Mathf.Abs(attackDir.y) > Mathf.Abs(attackDir.x)) ? 340f : 260f;
            GameObject hitbox = Instantiate(meleeHitboxPrefab, 
                transform.position + (Vector3)attackDir * spawnDistance, 
                Quaternion.Euler(0, 0, angle));
            hitbox.transform.SetParent(transform);
            Destroy(hitbox, attackDuration);
        }

        // 공격 후 쿨타임 동안은 계속 공격 방향을 바라보게 하려면 
        // yield return 이후에 isAttacking을 false로 바꿉니다.
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            if (MiniGameManager.Instance != null) MiniGameManager.Instance.TakeDamage();
            
            // 넉백 방향 계산
            Vector2 knockDir = (transform.position - collision.transform.position).normalized;
            
            // 무적 및 넉백 루틴 시작
            StartCoroutine(KnockbackRoutine());
            StartCoroutine(InvincibilityRoutine());

            // 힘 적용
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // [추가] 넉백 시간 동안 조작을 잠시 멈추는 코루틴
    IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(0.2f); // 넉백으로 밀려나는 시간 (짧게 설정)
        isKnockedBack = false;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0;
        while (timer < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
}