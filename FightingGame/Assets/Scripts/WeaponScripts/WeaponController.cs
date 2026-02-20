using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
    public string weaponName;
    public int weaponID;
    public Vector3 equippedScale = Vector3.one;
    public string idleAnim = "char_idle";
    public string attackAnim = "char_punch";
    public string throwAnim = "";
    public string runAnim = "char_run";
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected HitBox hitBox;

    protected string originalTag;

    protected virtual void Awake()
    {
        EnsureReferences();
    }


    protected void EnsureReferences()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<Collider2D>();
        if (hitBox == null) hitBox = GetComponentInChildren<HitBox>();

        if (string.IsNullOrEmpty(originalTag))
        {
            originalTag = gameObject.tag;
        }
    }

    public virtual void OnEquip()
    {
        EnsureReferences();
        gameObject.tag = originalTag;
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
