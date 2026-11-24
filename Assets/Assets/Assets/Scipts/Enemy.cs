
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float moveTime = 2f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("ไม่พบ Rigidbody2D บน Enemy!");
            enabled = false;
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // ป้องกันการทะลุวัตถุขณะเคลื่อนที่เร็ว
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        ChangeDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= moveTime)
        {
            ChangeDirection();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    void ChangeDirection()
    {
        direction = Random.insideUnitCircle.normalized;
        timer = 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // หยุดความเร็วเดิม
            rb.linearVelocity = Vector2.zero;

            // สุ่มทิศทางใหม่
            ChangeDirection();
        }
    }
}



