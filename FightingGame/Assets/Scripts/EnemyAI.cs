using Spine;
using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Chasing,
        Attacking,
        Jumping
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

    private EnemyState currentState = EnemyState.Chasing;
    private TrackEntry currentActionTrack;
    private bool grounded;

    private float horizontalMovement = 0f;

    public GameObject attackHitbox;
    public float attackDuration = 0.3f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        knightControl = GetComponentInChildren<KnightControl>();
        bodyCollider = GetComponent<BoxCollider2D>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    void Update()
    {
        if (!isAiActive || player == null)
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
        if (currentState == EnemyState.Jumping && grounded)
        {
            SetState(EnemyState.Chasing);
        }
        else if (currentState == EnemyState.Attacking && currentActionTrack.IsComplete)
        {
            SetState(EnemyState.Chasing);
        }

        if (currentState != EnemyState.Attacking)
        {
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
            }
        }

        switch (currentState)
        {
            case EnemyState.Chasing:
            case EnemyState.Jumping:
                float direction = player.position.x > transform.position.x ? 1 : -1;
                horizontalMovement = direction;
                FlipCharacter(direction);
                break;
            case EnemyState.Attacking:
                horizontalMovement = 0;
                break;
        }
    }

    private void SetState(EnemyState newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Chasing:
                knightControl.running();
                break;
            case EnemyState.Attacking:
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

    public void SetAIActive(bool isActive)
    {
        isAiActive = isActive;
        if (!isActive)
        {
            rb.linearVelocity = Vector2.zero;
            horizontalMovement = 0;
            knightControl.idle();
        }
        else
        {
            SetState(EnemyState.Chasing);
        }
    }
    IEnumerator AttackSequence()
    {
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.SetActive(false);
    }

    public void Die()
    {
        isAiActive = false;
        rb.linearVelocity = Vector2.zero;
        knightControl.death();
    }
}
