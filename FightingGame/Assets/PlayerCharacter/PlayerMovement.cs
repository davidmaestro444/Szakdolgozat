using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float groundspeed = 4f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask groundMask;

    private bool grounded;
    private float xInput;

    void Update()
    {
        xInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Vertical") && grounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0);
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        body.linearVelocity = new Vector2(xInput * groundspeed, body.linearVelocity.y);
    }

    void CheckGround()
    {
        grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }
}
