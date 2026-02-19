using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
    public string weaponName;
    public int weaponID;
    public Vector3 equippedScale = Vector3.one;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected HitBox hitBox;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        hitBox = GetComponentInChildren<HitBox>();
    }

    public virtual void OnEquip()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = equippedScale;

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }

        if (col != null)
        {
            col.enabled = false;
            col.isTrigger = true;
        }
    }

    public abstract void Throw(float direction, string ownerTag);
}
