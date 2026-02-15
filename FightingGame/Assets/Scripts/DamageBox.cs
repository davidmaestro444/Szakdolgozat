using UnityEngine;

public class DamageBox : MonoBehaviour
{
    private bool isDead = false;
    private Collider2D bodyCollider;
    private Rigidbody2D rb;

    [Header("Pajzs")]
    public bool hasShield = false;
    public GameObject shieldVisual;

    void Awake()
    {
        bodyCollider = GetComponentInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public void GetHit()
    {
        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            Debug.Log(gameObject.name + " pajzsa összetört!");
            return;
        }

        if (isDead) return;
        isDead = true;

        if (bodyCollider != null) bodyCollider.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        var knightControl = GetComponentInChildren<KnightControl>();
        if (knightControl != null) knightControl.death();

        GameManager.Instance.OnCharacterDied(gameObject);
    }

    public void ResetCharacter()
    {
        isDead = false;
        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);

        gameObject.SetActive(true);
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.Rebind();

        if (bodyCollider != null) bodyCollider.enabled = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }

        WeaponManager wm = GetComponentInChildren<WeaponManager>();
        if (wm != null) wm.RefreshWeapon();

        var pm = GetComponent<PlayerMovement>();
        if (pm != null) { pm.enabled = true; pm.ResetState(); }
    }
}
