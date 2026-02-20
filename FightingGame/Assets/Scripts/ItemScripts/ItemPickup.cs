using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Spear, Shield }
    public ItemType type;

    private List<PlayerMovement> playersInRange = new List<PlayerMovement>();
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        for (int i = playersInRange.Count - 1; i >= 0; i--)
        {
            PlayerMovement pm = playersInRange[i];
            if (pm != null && Input.GetKeyDown(pm.interactKey))
            {
                if (type == ItemType.Shield)
                {
                    DamageBox db = pm.GetComponent<DamageBox>();
                    if (db != null)
                    {
                        db.EnableShield();
                        Destroy(gameObject);
                    }
                }
                else if (type == ItemType.Spear)
                {
                    WeaponManager wm = pm.GetComponentInChildren<WeaponManager>();
                    if (wm != null && wm.TryPickUpWeapon())
                    {
                        playersInRange.Remove(pm);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (transform.parent == null && rb != null && rb.bodyType != RigidbodyType2D.Static)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }

        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm != null && !playersInRange.Contains(pm))
        {
            playersInRange.Add(pm);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm != null && playersInRange.Contains(pm))
        {
            playersInRange.Remove(pm);
        }
    }
}
