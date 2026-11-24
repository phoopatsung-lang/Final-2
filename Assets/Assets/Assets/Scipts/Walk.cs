
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator _animator;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("ไม่พบ Rigidbody2D บน Player!");
            enabled = false;
            return;
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (_animator == null)
            _animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        movement = Vector2.zero;

        // รับเฉพาะ WASD
        if (Input.GetKey(KeyCode.D))
            movement.x = 1f;

        if (Input.GetKey(KeyCode.A))
            movement.x = -1f;

        if (Input.GetKey(KeyCode.W))
            movement.y = 1f;

        if (Input.GetKey(KeyCode.S))
            movement.y = -1f;

        movement = movement.normalized;

        // Animator
        if (_animator != null)
        {
            _animator.SetFloat("MoveX", movement.x);
            _animator.SetFloat("MoveY", movement.y);
            _animator.SetFloat("Speed", movement.sqrMagnitude);
        }

        // พลิกตัวละคร
        if (spriteRenderer != null)
        {
            if (movement.x > 0)
                spriteRenderer.flipX = true;
            else if (movement.x < 0)
                spriteRenderer.flipX = false;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player ชนกับ: " + collision.gameObject.name);
    }
}





