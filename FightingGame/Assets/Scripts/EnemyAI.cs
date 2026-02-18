using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState 
    { 
        Idle, 
        Chasing, 
        Attacking, 
        Jumping, 
        Advancing 
    }

    public float speed = 4f;
    public float jumpForce = 16f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float jumpCheckDistance = 1.2f;
    public LayerMask groundLayer;

    public Transform player;
    public Transform visuals;
    public GameObject punchHitbox;

    private Rigidbody2D rb;
    private KnightControl knightControl;
    private WeaponManager weaponManager;
    private CapsuleCollider2D bodyCollider;

    private bool isAiActive = false;
    private bool isDead = false;
    private EnemyState currentState = EnemyState.Idle;

    public bool isGrounded;

    private float horizontalMovement = 0f;
    private float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        knightControl = GetComponentInChildren<KnightControl>();
        weaponManager = GetComponentInChildren<WeaponManager>();
        if (visuals == null) visuals = transform.Find("Visuals");
    }

    void Update()
    {
        if (!isAiActive || isDead) { horizontalMovement = 0; return; }

        CheckGrounded();

        if (currentState != EnemyState.Attacking)
        {
            UpdateAILogic();
        }
    }

    private void FixedUpdate()
    {
        if (!isAiActive || isDead) return;

        if (currentState == EnemyState.Attacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocity.y);
        }
    }

    private void UpdateAILogic()
    {
        if (currentState == EnemyState.Jumping)
        {
            if (currentState == EnemyState.Advancing)
            {
                horizontalMovement = -1f;
            }
            else if (player != null)
            {
                horizontalMovement = Mathf.Sign(player.position.x - transform.position.x);
            }

            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                SetState(player == null ? EnemyState.Advancing : EnemyState.Chasing);
            }
            return;
        }

        if (currentState == EnemyState.Advancing)
        {
            horizontalMovement = -1f;
            FlipCharacter(-1f);

            if (isGrounded && ShouldJump())
            {
                SetState(EnemyState.Jumping);
            }
            return;
        }

        if (player == null)
        {
            horizontalMovement = 0;
            return;
        }

        float distX = player.position.x - transform.position.x;
        float distY = player.position.y - transform.position.y;
        float absDistX = Mathf.Abs(distX);
        float currentRange = (weaponManager != null && weaponManager.HasBow()) ? 7f : attackRange;

        if (absDistX <= currentRange && Mathf.Abs(distY) < 1.0f && Time.time > lastAttackTime + attackCooldown && isGrounded)
        {
            SetState(EnemyState.Attacking);
            return;
        }

        if (isGrounded && ShouldJump())
        {
            SetState(EnemyState.Jumping);
            return;
        }
        SetState(EnemyState.Chasing);

        if (absDistX > 0.6f)
        {
            horizontalMovement = Mathf.Sign(distX);
            FlipCharacter(horizontalMovement);
        }
        else
        {
            horizontalMovement = 0;
            FlipCharacter(Mathf.Sign(distX));
        }
    }

    private void SetState(EnemyState newState)
    {
        if (newState == currentState && newState != EnemyState.Jumping) return;
        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Idle: knightControl.idle(); break;
            case EnemyState.Chasing: knightControl.running(); break;
            case EnemyState.Advancing: knightControl.running(); break;
            case EnemyState.Attacking: StartCoroutine(AttackSequence()); break;
            case EnemyState.Jumping:
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                knightControl.jump();
                break;
        }
    }

    IEnumerator AttackSequence()
    {
        lastAttackTime = Time.time;
        horizontalMovement = 0;
        knightControl.attack_1();

        if (weaponManager != null && weaponManager.HasBow())
        {
            yield return new WaitForSeconds(0.4f);
            float dir = visuals.localScale.x;
            weaponManager.ShootArrow(Mathf.Sign(dir));
            yield return new WaitForSeconds(1.1f);
        }
        else if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            Collider2D weaponCol = weaponManager.currentWeapon.GetComponent<Collider2D>();
            if (weaponCol != null)
            {
                weaponCol.enabled = true;
                yield return new WaitForSeconds(0.3f);
                weaponCol.enabled = false;
            }
        }
        else if (punchHitbox != null)
        {
            punchHitbox.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            punchHitbox.SetActive(false);
        }

        if (isDead)
        {
            yield break;
        }

        if (player == null)
        {
            SetState(EnemyState.Advancing);
        }
        else
        {
            SetState(EnemyState.Chasing);
        }
    }

    void FlipCharacter(float moveDirection)
    {
        if (visuals == null) return;
        Vector3 scale = visuals.localScale;
        float baseScale = 0.2185436f;
        scale.x = moveDirection > 0 ? baseScale : -baseScale;
        visuals.localScale = scale;
    }

    private bool ShouldJump()
    {
        float lookDir = Mathf.Sign(visuals.localScale.x);
        Bounds bounds = bodyCollider.bounds;

        Vector2 rayOrigin = new Vector2(transform.position.x + (lookDir * jumpCheckDistance), bounds.min.y + 0.1f);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 2.0f, groundLayer);

        Debug.DrawRay(rayOrigin, Vector2.down * 2.0f, hit.collider == null ? Color.magenta : Color.white);

        return hit.collider == null;
    }

    private void CheckGrounded()
    {
        Bounds bounds = bodyCollider.bounds;

        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - 0.1f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.8f, 0.2f);

        isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);

        Color debugColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(new Vector3(bounds.min.x, bounds.min.y, 0), Vector3.down * 0.2f, debugColor);
        Debug.DrawRay(new Vector3(bounds.max.x, bounds.min.y, 0), Vector3.down * 0.2f, debugColor);
    }

    public void StartAdvancing()
    {
        player = null;

        if (currentState != EnemyState.Attacking)
        {
            StopAllCoroutines();
            if (punchHitbox != null) punchHitbox.SetActive(false);
            if (weaponManager != null && weaponManager.currentWeapon != null)
            {
                var col = weaponManager.currentWeapon.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }

            isDead = false;
            isAiActive = true;
            SetState(EnemyState.Advancing);
        }
    }
    public void StartDueling(Transform playerTarget) 
    { 
        isDead = false; 
        isAiActive = true; 
        player = playerTarget; 
        SetState(EnemyState.Chasing); 
    }
    public void DeactivateAI() 
    { 
        isAiActive = false; 
        SetState(EnemyState.Idle); 
    }
    public void Die()
    {
        StopAllCoroutines();

        if (punchHitbox != null) punchHitbox.SetActive(false);
        if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            var col = weaponManager.currentWeapon.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
        isDead = true;
        isAiActive = false;
        rb.linearVelocity = Vector2.zero;
        knightControl.death();
    }
    public void ResetState() 
    { 
        isDead = false; 
        isAiActive = true; 
        SetState(EnemyState.Idle); 
        StopAllCoroutines(); 
    }
}