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

        bodyCollider.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        var playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            GetComponentInChildren<KnightControl>().death();
        }

        var enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.Die();
        }
        GameManager.Instance.OnCharacterDied(gameObject);
    }

    public void ResetCharacter()
    {
        isDead = false;
        bodyCollider.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ResetState();
        }

        var enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.ResetState();
        }
    }
}
