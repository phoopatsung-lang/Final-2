using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float fallSpeed = 3f;

    void Update()
    {
        transform.Translate(
            Vector2.down * fallSpeed * Time.deltaTime
        );

        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health =
                other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(1);
            }

            Destroy(gameObject);
        }
    }
}
