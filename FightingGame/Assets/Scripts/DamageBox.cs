using UnityEngine;

public class DamageBox : MonoBehaviour
{
    private bool isDead = false;
    private Collider2D bodyCollider;
    private Rigidbody2D rb;

    void Awake()
    {
        bodyCollider = GetComponentInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetHit()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " eltalálva!");

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
        if (bodyCollider != null) bodyCollider.enabled = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }

        WeaponManager wm = GetComponentInChildren<WeaponManager>();
        if (wm != null)
        {
            wm.RefreshWeapon();
        }

        gameObject.SetActive(true);

        var pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.enabled = true;
            pm.ResetState();
        }

        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.ResetState();
    }
}
