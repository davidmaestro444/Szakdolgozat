using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform handSocket;
    public WeaponController currentWeapon;
    public List<GameObject> weaponPrefabs;
    public GameObject arrowPrefab;
    private int currentWeaponIndex = 0;

    void Start()
    {

    }

    void LateUpdate()
    {
        if (currentWeapon != null && currentWeapon.transform.parent == handSocket)
        {
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            currentWeapon.transform.localScale = currentWeapon.equippedScale;

            if (!currentWeapon.gameObject.activeSelf) currentWeapon.gameObject.SetActive(true);
        }
    }

    public void RefreshWeapon()
    {
        if (currentWeapon == null && handSocket.childCount > 0)
        {
            currentWeapon = handSocket.GetChild(0).GetComponent<WeaponController>();
        }

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            currentWeapon.OnEquip();
        }
    }

    public void ThrowCurrentWeapon(float direction)
    {
        if (currentWeapon == null) return;
        currentWeapon.Throw(direction, transform.root.tag);
        currentWeapon = null;
    }

    public bool TryPickUpWeapon()
    {
        if (currentWeapon != null) return false;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2.0f);
        foreach (var hit in hitColliders)
        {
            WeaponController foundWeapon = hit.GetComponent<WeaponController>();

            if (foundWeapon != null && hit.transform.parent == null)
            {
                currentWeapon = foundWeapon;
                currentWeapon.transform.SetParent(handSocket);
                currentWeapon.OnEquip();
                return true;
            }
        }
        return false;
    }

    public void EquipWeaponByIndex(int index)
    {
        if (weaponPrefabs == null || weaponPrefabs.Count == 0) return;
        foreach (Transform child in handSocket)
        {
            Destroy(child.gameObject);
        }
        currentWeapon = null;

        currentWeaponIndex = index % weaponPrefabs.Count;
        GameObject prefab = weaponPrefabs[currentWeaponIndex];
        GameObject spawned = Instantiate(prefab, handSocket);
        currentWeapon = spawned.GetComponent<WeaponController>();

        if (currentWeapon != null)
        {
            currentWeapon.OnEquip();
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    public void SwitchToNextWeapon()
    {
        EquipWeaponByIndex(currentWeaponIndex + 1);
    }

    public void ShootArrow(float facingDirection)
    {
        if (arrowPrefab == null) return;
        Vector3 spawnOffset = new Vector3(facingDirection * 0.8f, 0, 0);
        GameObject arrowObj = Instantiate(arrowPrefab, handSocket.position + spawnOffset, Quaternion.identity);
        Arrow projectile = arrowObj.GetComponent<Arrow>();
        if (projectile != null) projectile.Launch(facingDirection, transform.root.tag);
    }
    public bool HasWeapon() => currentWeapon != null;
    public bool HasBow() => currentWeapon != null && currentWeapon.weaponID == 2;
}
