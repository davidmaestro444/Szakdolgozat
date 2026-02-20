using UnityEngine;

public class SwordController : WeaponController
{
    public override void Throw(float direction, string ownerTag)
    {
        transform.parent = null;
        gameObject.tag = ownerTag;
        transform.position += new Vector3(direction * 1.2f, 0.5f, 0);

        if (hitBox != null) hitBox.PrepareForThrow(ownerTag);

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;

            rb.AddForce(new Vector2(direction * 10f, 3f), ForceMode2D.Impulse);
            rb.AddTorque(-direction * 15f, ForceMode2D.Impulse);
        }

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }
    }
}
