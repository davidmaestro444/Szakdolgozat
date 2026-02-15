using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Spear, Shield }
    public ItemType type;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();

        if (pm == null) return;

        if (Input.GetKeyDown(pm.interactKey))
        {
            if (type == ItemType.Shield)
            {
                DamageBox db = pm.GetComponent<DamageBox>();
                if (db != null)
                {
                    db.hasShield = true;
                    if (db.shieldVisual != null) db.shieldVisual.SetActive(true);
                    Destroy(gameObject);
                    Debug.Log("Pajzs felvéve!");
                }
            }
            else if (type == ItemType.Spear)
            {
                WeaponManager wm = pm.GetComponentInChildren<WeaponManager>();

                if (wm != null)
                {
                    if (wm.TryPickUpWeapon())
                    {
                        Debug.Log("Lándzsa felvéve!");
                    }
                }
            }
        }
    }
}
