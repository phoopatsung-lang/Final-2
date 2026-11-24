using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhonePlayer : MonoBehaviour
{
    [Range(0, 3)] public int slot;
    public float speed = 5f;
    public bool keyboardForTesting = true;
    public bool hideWhenNoPhone = true;

    Rigidbody2D rb; SpriteRenderer sr; Animator anim; Collider2D col;
    Vector2 move;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        var p = PhoneServer.Get(slot);
        bool phone = p != null && p.connected;
        bool kb = keyboardForTesting && slot == 0 && !phone;

        if (phone) move = p.tilt;
        else if (kb) move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        else move = Vector2.zero;

        if (hideWhenNoPhone)
        {
            bool show = (p != null && p.id != null) || kb;
            if (sr) sr.enabled = show;
            if (col) col.enabled = show;
        }

        if (anim)
        {
            anim.SetFloat("MoveX", move.x);
            anim.SetFloat("MoveY", move.y);
            anim.SetFloat("Speed", move.sqrMagnitude);
        }
        if (sr) { if (move.x > 0.05f) sr.flipX = true; else if (move.x < -0.05f) sr.flipX = false; }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = move * speed;
    }
}
