using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private enum PlayerState 
    { 
        Idle, 
        Running, 
        Jumping, 
        Attacking 
    }

    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Vertical";
    public KeyCode attackKey = KeyCode.Space;

    [SerializeField] private float groundspeed = 4f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask groundMask;
    public GameObject attackHitbox;
    public float attackDuration = 0.3f;
    public KnightControl knightControl;
    public Transform knightVisuals;

    public bool grounded;
    private float xInput;
    private bool isFacingRight = true;
    private PlayerState currentState = PlayerState.Idle;

    private WeaponManager weaponManager;
    public float bowAttackDuration = 0.8f;
    public float bowShootDelay = 0.4f;

    public KeyCode throwKey = KeyCode.Q;
    public KeyCode interactKey = KeyCode.F;
    private bool isUncontrollable = false;
    private float defaultGravity;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        defaultGravity = body.gravityScale;
        weaponManager = GetComponentInChildren<WeaponManager>();
        DamageBox db = GetComponent<DamageBox>();
        if (db != null)
        {
            db.OnDeath += HandleDeath;
        }
    }

    void OnDestroy()
    {
        DamageBox db = GetComponent<DamageBox>();
        if (db != null)
        {
            db.OnDeath -= HandleDeath;
        }
    }

    void Update()
    {
        xInput = Input.GetAxis(horizontalAxis);
        CheckGround();

        if (Input.GetKeyDown(throwKey) && weaponManager.HasWeapon() && !weaponManager.HasBow())
        {
            StartCoroutine(ThrowSequence());
        }

        if (Input.GetKeyDown(interactKey))
        {
            if (weaponManager.TryPickUpWeapon())
            {
                knightControl.idle();
            }
        }
        UpdateState();
        FlipCharacter();
    }
    IEnumerator ThrowSequence()
    {
        if (weaponManager.currentWeapon == null) yield break;

        SetState(PlayerState.Attacking);

        string wName = weaponManager.currentWeapon.name.ToLower();
        if (wName.Contains("spear"))
            knightControl.weapon_throw();
        else
            knightControl.weapon_throw();

        yield return new WaitForSeconds(0.15f);

        float direction = isFacingRight ? 1f : -1f;
        weaponManager.ThrowCurrentWeapon(direction);

        yield return new WaitForSeconds(0.2f);

        SetState(PlayerState.Idle);
        knightControl.idle();
    }

    private void FixedUpdate()
    {
        if (isUncontrollable) return;

        if (currentState == PlayerState.Attacking)
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
        }
        else
        {
            body.linearVelocity = new Vector2(xInput * groundspeed, body.linearVelocity.y);
        }
    }

    private void UpdateState()
    {
        if (currentState == PlayerState.Jumping)
        {
            if (grounded && body.linearVelocity.y <= 0.1f)
            {
                SetState(PlayerState.Idle);
            }
            return;
        }

        if (currentState == PlayerState.Attacking)
        {
            if (IsAnimationFinished())
            {
                SetState(PlayerState.Idle);
            }
            return;
        }

        if (Input.GetKeyDown(attackKey) && grounded)
        {
            SetState(PlayerState.Attacking);
            StartCoroutine(AttackSequence());
        }
        else if (Input.GetButtonDown(jumpButton) && grounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0);
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            SetState(PlayerState.Jumping);
        }
        else if (grounded)
        {
            if (Mathf.Abs(xInput) > 0.1f)
                SetState(PlayerState.Running);
            else
                SetState(PlayerState.Idle);
        }
    }

    private bool IsAnimationFinished()
    {
        if (knightControl == null) return true;

        Animator anim = knightControl.GetComponent<Animator>();
        if (anim == null) anim = knightControl.GetComponentInChildren<Animator>();

        if (anim != null)
        {
            return anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !anim.IsInTransition(0);
        }
        return true;
    }

    IEnumerator AttackSequence()
    {
        body.linearVelocity = new Vector2(0, body.linearVelocity.y);

        if (weaponManager != null && weaponManager.HasBow())
        {
            yield return new WaitForSeconds(bowShootDelay);
            float dir = isFacingRight ? 1f : -1f;
            weaponManager.ShootArrow(dir);
            yield return new WaitForSeconds(bowAttackDuration - bowShootDelay);
        }
        else if (weaponManager != null && weaponManager.currentWeapon != null)
        {
            Collider2D weaponCollider = weaponManager.currentWeapon.GetComponent<Collider2D>();
            if (weaponCollider != null)
            {
                weaponCollider.enabled = true;
                yield return new WaitForSeconds(attackDuration);
                weaponCollider.enabled = false;
            }
        }
        else
        {
            if (attackHitbox != null)
            {
                attackHitbox.SetActive(true);
                yield return new WaitForSeconds(attackDuration);
                attackHitbox.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(attackDuration);
            }
        }
        SetState(PlayerState.Idle);
    }

    private void SetState(PlayerState newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        switch (currentState)
        {
            case PlayerState.Idle: knightControl.idle(); break;
            case PlayerState.Running: knightControl.running(); break;
            case PlayerState.Jumping: knightControl.jump(); break;
            case PlayerState.Attacking: knightControl.attack(); break;
        }
    }

    void FlipCharacter()
    {
        if (isFacingRight && xInput < 0f || !isFacingRight && xInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 theScale = knightVisuals.localScale;
            theScale.x *= -1;
            knightVisuals.localScale = theScale;
        }
    }

    void CheckGround()
    {
        grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        StartCoroutine(KnockbackRoutine(force, duration));
    }

    IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isUncontrollable = true;
        float originalGravity = body.gravityScale;
        body.gravityScale = 10f;
        body.linearVelocity = Vector2.zero;
        body.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        body.gravityScale = originalGravity;
        isUncontrollable = false;
    }

    public void ResetState()
    {
        knightControl.idle();
        currentState = PlayerState.Idle;
        body.linearVelocity = Vector2.zero;
        if (attackHitbox != null) attackHitbox.SetActive(false);
        StopAllCoroutines();
        isUncontrollable = false;
        if (body != null)
        {
            body.gravityScale = defaultGravity;
        }
    }

    private void HandleDeath()
    {
        this.enabled = false;
        if (knightControl != null) knightControl.death();
        StopAllCoroutines();
    }
}
