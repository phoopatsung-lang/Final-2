using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 45f;
    public float destroyY = -10f; // ระดับความสูงแกน Y ที่จะให้ลบตัวเองทิ้ง

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(0, -moveSpeed);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // เช็คว่าตกลงมาต่ำกว่าขอบจอหรือยัง ถ้าใช่ให้ลบตัวเอง
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bullet"))
        {
            Destroy(gameObject);
        }
    }
}
