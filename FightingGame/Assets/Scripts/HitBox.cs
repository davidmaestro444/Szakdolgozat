using UnityEngine;

public class HitBox : MonoBehaviour
{
    public string ownerTag;
    private bool isLethal = true;
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void PrepareForThrow(string tagOfThrower)
    {
        ownerTag = tagOfThrower;
        isLethal = true;
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transform.parent != null)
        {
            DamageBox target = other.GetComponentInParent<DamageBox>();
            if (target != null && transform.root != other.transform.root)
            {
                target.GetHit();
                if (col != null) col.enabled = false;
            }
            return;
        }

        if (!isLethal) return;
        if (!string.IsNullOrEmpty(ownerTag) && other.CompareTag(ownerTag)) return;

        DamageBox thrownTarget = other.GetComponentInParent<DamageBox>();

        if (thrownTarget != null)
        {
            thrownTarget.GetHit();
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StopWeaponOnGround();
        }
    }

    private void StopWeaponOnGround()
    {
        isLethal = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.bodyType = RigidbodyType2D.Static;
        }
        if (col != null) col.isTrigger = true;
    }
}
