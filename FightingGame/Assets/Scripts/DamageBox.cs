using System;
using UnityEngine;

public class DamageBox : MonoBehaviour
{
    public event Action OnDeath;
    public event Action OnHit;

    private bool isDead = false;
    private Collider2D bodyCollider;
    private Rigidbody2D rb;

    public bool hasShield = false;
    public GameObject shieldVisual;

    private float hitCooldown = 0.15f;
    private float lastHitTimestamp;

    void Awake()
    {
        bodyCollider = GetComponent<Collider2D>();
        if (bodyCollider == null) bodyCollider = GetComponentInChildren<Collider2D>();

        rb = GetComponent<Rigidbody2D>();
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public void GetHit(bool canBeShielded = true)
    {
        if (Time.time < lastHitTimestamp + hitCooldown) return;
        lastHitTimestamp = Time.time;

        if (canBeShielded && hasShield)
        {
            DisableShield();
            return;
        }

        if (isDead) return;

        OnHit?.Invoke();
        isDead = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        if (bodyCollider != null)
        {
            bodyCollider.enabled = true;
        }
        OnDeath?.Invoke();
    }

    public void ResetCharacter()
    {
        isDead = false;
        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        gameObject.SetActive(true);

        if (bodyCollider != null) bodyCollider.enabled = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void EnableShield()
    {
        hasShield = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    public void DisableShield()
    {
        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }
}
