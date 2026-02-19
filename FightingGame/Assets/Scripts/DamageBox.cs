using UnityEngine;

public class DamageBox : MonoBehaviour
{
    private bool isDead = false;
    private Collider2D bodyCollider;
    private Rigidbody2D rb;

    [Header("Pajzs")]
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

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.Die();

        var playerMove = GetComponent<PlayerMovement>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
            GetComponentInChildren<KnightControl>().death();
        }
        GameManager.Instance.OnCharacterDied(gameObject);
    }

    public void ResetCharacter()
    {
        if (isDead)
        {
            isDead = false;
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
        }

        gameObject.SetActive(true);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.Rebind();

        if (bodyCollider != null) bodyCollider.enabled = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
        }

        WeaponManager wm = GetComponentInChildren<WeaponManager>();
        if (wm != null) wm.RefreshWeapon();

        var pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.enabled = true;
            pm.ResetState();
        }

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.ResetState();
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
