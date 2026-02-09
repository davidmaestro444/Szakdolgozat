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

    private bool grounded;
    private float xInput;
    private bool isFacingRight = true;
    private PlayerState currentState = PlayerState.Idle;

    private WeaponManager weaponManager;
    public float bowAttackDuration = 0.8f;
    public float bowShootDelay = 0.4f;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        weaponManager = GetComponentInChildren<WeaponManager>();
    }

    void Update()
    {
        xInput = Input.GetAxis(horizontalAxis);

        CheckGround();
        UpdateState();
        FlipCharacter();
    }

    private void FixedUpdate()
    {
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
            yield return new WaitForSeconds(attackDuration);
        }
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
            case PlayerState.Attacking: knightControl.attack_1(); break;
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

    public void ResetState()
    {
        knightControl.idle();
        currentState = PlayerState.Idle;
        body.linearVelocity = Vector2.zero;
        if (attackHitbox != null) attackHitbox.SetActive(false);
        StopAllCoroutines();
    }
}
