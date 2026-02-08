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
    public float jumpForce = 7f;
    public float attackRange = 1.5f;
    public float jumpCheckDistance = 1f;
    public LayerMask groundLayer;
    public Transform player;
    private Rigidbody2D rb;
    private KnightControl knightControl;
    private BoxCollider2D bodyCollider;
    private bool isAiActive = false;
    private float attackCooldown = 2f;
    private float lastAttackTime;
    private bool isDead = false;
    private EnemyState currentState = EnemyState.Idle;
    private KnightControl.DummyTrack currentActionTrack;
    private bool grounded;
    private float horizontalMovement = 0f;
    public GameObject attackHitbox;
    public float attackDuration = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knightControl = GetComponentInChildren<KnightControl>();
        bodyCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!isAiActive)
        {
            horizontalMovement = 0;
            return;
        }

        CheckGrounded();
        UpdateState();
    }

    private void FixedUpdate()
    {
        if (isAiActive)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * speed, rb.linearVelocity.y);
        }
    }

    private void UpdateState()
    {
        if (currentState == EnemyState.Jumping)
        {
            HandleMovementDirection();
            if (grounded && rb.linearVelocity.y <= 0)
            {
                SetState(isAiActive && player == null ? EnemyState.Advancing : EnemyState.Chasing);
            }
            return;
        }

        if (currentState == EnemyState.Attacking)
        {
            horizontalMovement = 0;
            if (currentActionTrack.IsComplete)
            {
                SetState(EnemyState.Chasing);
            }
            return;
        }

        if (currentState == EnemyState.Advancing)
        {
            horizontalMovement = -1f;
            FlipCharacter(-1f);

            if (ShouldJump() && grounded)
            {
                SetState(EnemyState.Jumping);
            }
            return;
        }

        if (currentState == EnemyState.Chasing || currentState == EnemyState.Idle)
        {
            if (player == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown && grounded)
            {
                SetState(EnemyState.Attacking);
            }
            else if (ShouldJump() && grounded)
            {
                SetState(EnemyState.Jumping);
            }
            else
            {
                SetState(EnemyState.Chasing);
                HandleMovementDirection();
            }
        }
    }

    private void HandleMovementDirection()
    {
        if (currentState == EnemyState.Advancing)
        {
            horizontalMovement = -1f;
            FlipCharacter(-1f);
        }
        else if (player != null)
        {
            float direction = player.position.x > transform.position.x ? 1 : -1;
            horizontalMovement = direction;
            FlipCharacter(direction);
        }
    }

    private void SetState(EnemyState newState)
    {
        if (newState == currentState && newState != EnemyState.Jumping) return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Idle:
                horizontalMovement = 0;
                knightControl.idle();
                break;
            case EnemyState.Chasing:
            case EnemyState.Advancing:
                knightControl.running();
                break;
            case EnemyState.Attacking:
                horizontalMovement = 0;
                lastAttackTime = Time.time;
                currentActionTrack = knightControl.attack_1();
                StartCoroutine(AttackSequence());
                break;
            case EnemyState.Jumping:
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                currentActionTrack = knightControl.jump();
                break;
        }
    }

    public void StartAdvancing()
    {
        isDead = false;
        isAiActive = true;
        player = null;
        SetState(EnemyState.Advancing);
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
        if (isDead) return;

        isAiActive = false;
        horizontalMovement = 0;
        rb.linearVelocity = Vector2.zero;
        SetState(EnemyState.Idle);
    }

    public void Die()
    {
        isDead = true;
        isAiActive = false;
        horizontalMovement = 0;
        rb.linearVelocity = Vector2.zero;
        knightControl.death();
    }

    void FlipCharacter(float moveDirection)
    {
        if (moveDirection > 0 && transform.localScale.x < 0f || moveDirection < 0 && transform.localScale.x > 0f)
        {
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    private bool ShouldJump()
    {
        Vector2 rayOrigin = bodyCollider.bounds.center - new Vector3(0, bodyCollider.bounds.extents.y);
        rayOrigin.x += jumpCheckDistance * transform.localScale.x;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.2f, groundLayer);
        return hit.collider == null;
    }

    private void CheckGrounded()
    {
        Vector2 boxCenter = (Vector2)bodyCollider.bounds.center + Vector2.down * (bodyCollider.bounds.extents.y + 0.1f);
        grounded = Physics2D.OverlapBox(boxCenter, new Vector2(bodyCollider.bounds.size.x * 0.9f, 0.1f), 0f, groundLayer);
    }

    /*IEnumerator AttackSequence()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            yield return new WaitForSeconds(attackDuration);
            attackHitbox.SetActive(false);
        }
    }*/
    IEnumerator AttackSequence()
    {
        if (attackHitbox != null)
        {
            Collider2D weaponCollider = attackHitbox.GetComponent<Collider2D>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = true;
                yield return new WaitForSeconds(attackDuration);
                weaponCollider.enabled = false;
            }
        }
    }

    public void ResetState()
    {
        isDead = false;
        knightControl.idle();
        currentState = EnemyState.Idle;
        rb.linearVelocity = Vector2.zero;
        horizontalMovement = 0;
        if (attackHitbox != null) attackHitbox.SetActive(false);
        StopAllCoroutines();
    }
}
