using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 15f;
    private Rigidbody2D rb;
    private bool hasHit = false;
    private string shooterTag;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void Launch(float direction, string tagOfShooter)
    {
        shooterTag = tagOfShooter;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(direction * speed, 0);
        float targetAngle = (direction > 0) ? -90f : 90f;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        DamageBox targetDamageBox = other.GetComponentInParent<DamageBox>();

        if (targetDamageBox != null)
        {
            if (other.transform.root.CompareTag(shooterTag))
            {
                return;
            }
            targetDamageBox.GetHit();
            hasHit = true;
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
