using UnityEngine;

public class MiniGamePlayer : MonoBehaviour
{
    public float moveSpeed = 300f;
    public GameObject projectilePrefab;
    public float knockbackForce = 15f;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Simple WASD/Arrow movement for prototype
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveInput.normalized * moveSpeed * Time.fixedDeltaTime * 50f;
    }

    void Attack()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 direction = (mousePos - transform.position).normalized;
        
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().velocity = direction * 500f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            MiniGameManager.Instance.TakeDamage();
            // Knockback
            Vector2 knockDir = (transform.position - collision.transform.position).normalized;
            transform.position += (Vector3)knockDir * knockbackForce;
        }
    }
}
