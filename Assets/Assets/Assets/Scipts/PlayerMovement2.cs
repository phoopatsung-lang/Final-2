
using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        movement = Vector2.zero;

        // รับเฉพาะปุ่มลูกศร
        if (Input.GetKey(KeyCode.RightArrow))
            movement.x = 1f;

        if (Input.GetKey(KeyCode.LeftArrow))
            movement.x = -1f;

        if (Input.GetKey(KeyCode.UpArrow))
            movement.y = 1f;

        if (Input.GetKey(KeyCode.DownArrow))
            movement.y = -1f;

        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }
}



