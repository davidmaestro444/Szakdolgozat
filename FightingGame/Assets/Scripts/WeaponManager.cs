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
        if (currentWeapon == null || HasBow()) return;

        GameObject thrown = currentWeapon;
        currentWeapon = null;
        thrown.tag = transform.root.tag;
        thrown.transform.SetParent(null);

        float offsetDistance = thrown.name.Contains("Spear") ? 1.8f : 1.2f;
        thrown.transform.position += new Vector3(direction * offsetDistance, 0.5f, 0);

        HitBox hb = thrown.GetComponentInChildren<HitBox>();
        if (hb != null) hb.PrepareForThrow(transform.root.tag);

        Rigidbody2D rb = thrown.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.AddForce(new Vector2(direction * 12f, 4f), ForceMode2D.Impulse);

            if (!thrown.name.Contains("Spear"))
            {
                rb.AddTorque(-direction * 15f, ForceMode2D.Impulse);
            }
            else
            {
                float angle = (direction > 0) ? -90f : 90f;
                thrown.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    public bool TryPickUpWeapon()
    {
        if (currentWeapon != null) return false;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.0f);
        foreach (var hit in hitColliders)
        {
            bool isSword = hit.name.Contains("Sword") || hit.CompareTag("Sword");
            bool isSpear = hit.name.Contains("Spear") || hit.CompareTag("Spear");

            if ((isSword || isSpear) && hit.transform.parent == null)
            {
                currentWeapon = hit.gameObject;
                currentWeapon.transform.SetParent(handSocket);

                if (isSpear) currentWeapon.name = "Spear";
                else currentWeapon.name = "Sword";

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
