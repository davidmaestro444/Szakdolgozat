using Spine;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private enum PlayerState { Idle, Running, Jumping, Attacking }

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
    private TrackEntry currentActionTrack;

    void Update()
    {
        xInput = Input.GetAxis(horizontalAxis);

        CheckGround();
        UpdateState();
        FlipCharacter();
    }

    private void FixedUpdate()
    {
        body.linearVelocity = new Vector2(xInput * groundspeed, body.linearVelocity.y);
    }

    private void UpdateState()
    {
        if (currentState == PlayerState.Jumping && grounded)
        {
            SetState(PlayerState.Idle);
        }
        else if (currentState == PlayerState.Attacking && currentActionTrack.IsComplete)
        {
            SetState(PlayerState.Idle);
        }

        if (Input.GetKeyDown(attackKey) && currentState != PlayerState.Attacking && grounded)
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
        else if (currentState == PlayerState.Idle || currentState == PlayerState.Running)
        {
            if (Mathf.Abs(xInput) > 0.1f)
            {
                SetState(PlayerState.Running);
            }
            else
            {
                SetState(PlayerState.Idle);
            }
        }
    }

    IEnumerator AttackSequence()
    {
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.SetActive(false);
    }

    private void SetState(PlayerState newState)
    {
        if (newState == currentState) return;
        currentState = newState;
        switch (currentState)
        {
            case PlayerState.Idle: knightControl.idle(); break;
            case PlayerState.Running: knightControl.running(); break;
            case PlayerState.Jumping: currentActionTrack = knightControl.jump(); break;
            case PlayerState.Attacking: currentActionTrack = knightControl.attack_1(); break;
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
