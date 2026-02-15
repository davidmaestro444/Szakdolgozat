using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform handSocket;
    public GameObject currentWeapon;
    public List<GameObject> weaponPrefabs;
    public GameObject arrowPrefab;

    private int currentWeaponIndex = 0;

    void Start()
    {
        if (currentWeapon == null && weaponPrefabs.Count > 0 && handSocket.childCount == 0)
        {
            EquipWeaponByIndex(0);
        }
    }

    void LateUpdate()
    {
        if (currentWeapon != null && currentWeapon.transform.parent == handSocket)
        {
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            if (!currentWeapon.activeSelf) currentWeapon.SetActive(true);
        }
    }

    public void RefreshWeapon()
    {
        if (currentWeapon == null && handSocket != null && handSocket.childCount > 0)
        {
            currentWeapon = handSocket.GetChild(0).gameObject;
        }

        if (currentWeapon != null && currentWeapon.transform.parent == handSocket)
        {
            currentWeapon.SetActive(true);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;

            Rigidbody2D rb = currentWeapon.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0;
                rb.useFullKinematicContacts = true;
            }

            Collider2D col = currentWeapon.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                col.isTrigger = true;
            }
        }
    }

    public void ThrowCurrentWeapon(float direction)
    {
        if (currentWeapon == null) return;

        if (HasBow())
        {
            Debug.Log("Az íjat nem lehet eldobni!");
            return;
        }
        GameObject thrown = currentWeapon;
        currentWeapon = null;

        thrown.tag = transform.root.tag;
        thrown.transform.SetParent(null);

        thrown.transform.position += new Vector3(direction * 1.2f, 0.5f, 0);

        HitBox hb = thrown.GetComponentInChildren<HitBox>();
        if (hb != null)
        {
            hb.PrepareForThrow(transform.root.tag);
        }

        Rigidbody2D rb = thrown.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;

            rb.AddForce(new Vector2(direction * 10f, 3f), ForceMode2D.Impulse);
            rb.AddTorque(-direction * 15f, ForceMode2D.Impulse);
        }
    }

    public bool TryPickUpWeapon()
    {
        if (currentWeapon != null) return false;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hitColliders)
        {
            if ((hit.CompareTag("Sword") || hit.CompareTag("Spear")) && hit.transform.parent == null)
            {
                currentWeapon = hit.gameObject;
                currentWeapon.transform.SetParent(handSocket);

                if (hit.CompareTag("Spear")) currentWeapon.name = "Spear";
                else if (hit.CompareTag("Sword")) currentWeapon.name = "Sword";

                Rigidbody2D rb = currentWeapon.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.simulated = false;
                }

                RefreshWeapon();
                return true;
            }
        }
        return false;
    }

    public void EquipWeaponByIndex(int index)
    {
        if (weaponPrefabs == null || weaponPrefabs.Count == 0) return;
        if (currentWeapon != null && currentWeapon.scene.name != null) Destroy(currentWeapon);

        currentWeaponIndex = index % weaponPrefabs.Count;
        GameObject prefab = weaponPrefabs[currentWeaponIndex];
        currentWeapon = Instantiate(prefab, handSocket);
        currentWeapon.name = prefab.name;
        RefreshWeapon();
    }

    public void SwitchToNextWeapon()
    {
        int nextIndex = (currentWeaponIndex + 1) % weaponPrefabs.Count;
        EquipWeaponByIndex(nextIndex);
    }

    public void ShootArrow(float facingDirection)
    {
        if (arrowPrefab == null) return;
        Vector3 spawnOffset = new Vector3(facingDirection * 0.8f, 0, 0);
        GameObject arrow = Instantiate(arrowPrefab, handSocket.position + spawnOffset, Quaternion.identity);
        Arrow projectile = arrow.GetComponent<Arrow>();
        if (projectile != null) projectile.Launch(facingDirection, transform.root.tag);
    }

    public bool HasWeapon() => currentWeapon != null;
    public bool HasBow() => currentWeapon != null && (currentWeapon.name.ToLower().Contains("bow"));
}
