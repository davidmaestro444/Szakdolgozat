using System.Collections;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 6f;
    [SerializeField] protected float jumpForce = 14f;
    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected Transform visualContainer;
    [SerializeField] protected GameObject attackHitbox;

    protected Rigidbody2D rb;
    protected KnightControl knightControl;
    protected WeaponManager weaponManager;
    protected DamageBox damageBox;
    protected Collider2D mainCollider;
    protected Animator animator;
    protected bool isGrounded;
    protected bool isFacingRight = true;
    protected float currentHorizontalInput;
    protected bool isUncontrollable = false;
    protected float defaultGravity;
    protected bool isAttacking;
    private string currentAnimState = "";

    public bool Grounded
    {
        get
        {
            return isGrounded;
        }
    }

    public bool IsOnHighGround { get; private set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (visualContainer == null)
        {
            Transform v = transform.Find("Visuals");
            visualContainer = (v != null) ? v : transform;
        }

        knightControl = GetComponentInChildren<KnightControl>();
        weaponManager = GetComponentInChildren<WeaponManager>();
        damageBox = GetComponent<DamageBox>();
        mainCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();

        if (damageBox != null) damageBox.OnDeath += HandleDeath;
        if (rb != null) defaultGravity = rb.gravityScale;
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        if (damageBox != null) damageBox.OnDeath -= HandleDeath;
    }

    protected virtual void Update()
    {
        CheckGround();
        UpdateBehavior();
        UpdateAnimationState();
    }

    protected virtual void FixedUpdate()
    {
        if (isUncontrollable) return;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        rb.linearVelocity = new Vector2(currentHorizontalInput * moveSpeed, rb.linearVelocity.y);
        FlipCharacter(currentHorizontalInput);
    }

    protected abstract void UpdateBehavior();

    protected void Move(float direction)
    {
        currentHorizontalInput = direction;
    }

    protected void TryJump()
    {
        if (isGrounded && !isAttacking && !isUncontrollable)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    public void AIMove(float direction)
    {
        Move(direction);
    }

    public void AIJump()
    {
        TryJump();
    }

    protected void CheckGround()
    {
        if (mainCollider == null) return;
        Bounds bounds = mainCollider.bounds;
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - 0.1f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.8f, 0.2f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, groundMask);

        isGrounded = hits.Length > 0;
        IsOnHighGround = false;

        foreach (var hit in hits)
        {
            if (hit == mainCollider) continue;

            if (hit.CompareTag("HighGround"))
            {
                IsOnHighGround = true;
                break;
            }
        }
    }

    protected void FlipCharacter(float input)
    {
        if (isAttacking) return;

        if (Mathf.Abs(input) < 0.1f) return;
        bool shouldFaceRight = input > 0;
        if (isFacingRight != shouldFaceRight)
        {
            isFacingRight = shouldFaceRight;
            if (visualContainer != null)
            {
                Vector3 scale = visualContainer.localScale;
                scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1 : -1);
                visualContainer.localScale = scale;
            }
        }
    }

    public void PerformAttack()
    {
        if (isAttacking || !isGrounded) return;
        StartCoroutine(AttackRoutine());
    }

    public void PerformThrow()
    {
        if (isAttacking || !weaponManager.HasWeapon() || weaponManager.HasBow()) return;
        StartCoroutine(ThrowRoutine());
    }

    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        PlayAnim("Attack");
        knightControl.attack();

        yield return null;

        float waitTime = 0.3f;
        if (animator != null)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName("Idle") && !info.IsName("Run"))
            {
                waitTime = info.length;
            }
        }

        if (weaponManager.HasBow())
        {
            yield return new WaitForSeconds(0.4f);
            weaponManager.ShootArrow(isFacingRight ? 1f : -1f);
            yield return new WaitForSeconds(Mathf.Max(0, waitTime - 0.4f));
        }
        else if (weaponManager.currentWeapon != null)
        {
            Collider2D weaponCol = weaponManager.currentWeapon.GetComponent<Collider2D>();
            if (weaponCol != null)
            {
                weaponCol.enabled = true;
                yield return new WaitForSeconds(waitTime * 0.6f);
                weaponCol.enabled = false;
                yield return new WaitForSeconds(waitTime * 0.4f);
            }
            else
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        else
        {
            if (attackHitbox != null)
            {
                attackHitbox.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                attackHitbox.SetActive(false);

                yield return new WaitForSeconds(Mathf.Max(0, waitTime - 0.3f));
            }
            else
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
        isAttacking = false;
    }

    protected virtual IEnumerator ThrowRoutine()
    {
        isAttacking = true;
        PlayAnim("Throw");
        knightControl.weapon_throw();

        yield return new WaitForSeconds(0.15f);

        float dir = isFacingRight ? 1f : -1f;
        weaponManager.ThrowCurrentWeapon(dir);

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }

    protected void UpdateAnimationState()
    {
        if (isAttacking || isUncontrollable) return;

        if (!isGrounded)
        {
            PlayAnim("Jump");
            knightControl.jump();
            return;
        }

        if (Mathf.Abs(currentHorizontalInput) > 0.1f)
        {
            PlayAnim("Run");
            knightControl.running();
        }
        else
        {
            PlayAnim("Idle");
            knightControl.idle();
        }
    }

    private void PlayAnim(string newState)
    {
        if (currentAnimState == newState) return;
        currentAnimState = newState;
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        StartCoroutine(KnockbackRoutine(force, duration));
    }

    IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isUncontrollable = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 10f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        if (rb != null) rb.gravityScale = originalGravity;
        isUncontrollable = false;
    }

    protected virtual void HandleDeath()
    {
        currentHorizontalInput = 0;
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
        knightControl.death();
        StopAllCoroutines();
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    public virtual void ResetState()
    {
        this.enabled = true;
        isUncontrollable = false;
        isAttacking = false;
        currentAnimState = "";
        currentHorizontalInput = 0;
        knightControl.idle();
        if (rb != null) rb.gravityScale = defaultGravity;
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }
}