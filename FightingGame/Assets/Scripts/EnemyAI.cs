using System.Collections;
using UnityEngine;

public class EnemyAI : CharacterBase
{
    private enum EnemyState
    {
        Idle,
        Chasing,
        Advancing,
        Attacking
    }

    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float jumpCheckDistance = 1.2f;
    public Transform player;
    public bool isAiActive = false;
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime;

    protected override void UpdateBehavior()
    {
        if (!isAiActive)
        {
            Move(0);
            return;
        }

        float moveDir = 0;
        if (player == null)
        {
            currentState = EnemyState.Advancing;
        }
        else
        {
            currentState = EnemyState.Chasing;
        }

        if (CanAttackPlayer())
        {
            if (player != null && !isAttacking)
            {
                float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);
                FlipCharacter(dirToPlayer);
            }

            Move(0);

            if (isGrounded && Time.time > lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttack();
            }
            return;
        }

        if (currentState == EnemyState.Advancing)
        {
            moveDir = -1f;
        }
        else if (currentState == EnemyState.Chasing && player != null)
        {
            float distX = player.position.x - transform.position.x;
            if (Mathf.Abs(distX) > 0.6f)
            {
                moveDir = Mathf.Sign(distX);
            }
        }

        if (isGrounded && ShouldJump(moveDir))
        {
            TryJump();
        }
        Move(moveDir);
    }

    private bool CanAttackPlayer()
    {
        if (player == null) return false;

        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);

        float currentRange = (weaponManager != null && weaponManager.HasBow()) ? 7f : attackRange;

        return distX <= currentRange && distY < 1.0f;
    }

    private bool ShouldJump(float moveDir)
    {
        if (moveDir == 0) return false;

        Bounds bounds = mainCollider.bounds;
        Vector2 rayOrigin = new Vector2(transform.position.x + (moveDir * jumpCheckDistance), bounds.min.y + 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 2.0f, groundMask);

        return hit.collider == null;
    }

    public void StartAdvancing()
    {
        player = null;
        isAiActive = true;
    }

    public void StartDueling(Transform target)
    {
        player = target;
        isAiActive = true;
    }

    public void DeactivateAI()
    {
        isAiActive = false;
        Move(0);
    }

    public override void ResetState()
    {
        base.ResetState();
        isAiActive = true;
        currentState = EnemyState.Idle;
    }
}