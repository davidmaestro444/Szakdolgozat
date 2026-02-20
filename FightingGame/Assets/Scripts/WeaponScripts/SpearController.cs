using UnityEngine;

public class SpearController : WeaponController
{
    public override void Throw(float direction, string ownerTag)
    {
        transform.parent = null;
        gameObject.tag = ownerTag;
        transform.position += new Vector3(direction * 1.8f, 0.5f, 0);

        if (hitBox != null) hitBox.PrepareForThrow(ownerTag);

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            float angle = (direction > 0) ? -90f : 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            rb.AddForce(new Vector2(direction * 12f, 4f), ForceMode2D.Impulse);
        }

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }
    }
}
