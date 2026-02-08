using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform handSocket;
    public GameObject currentWeapon;

    void Awake()
    {
        RefreshWeapon();
    }

    public void RefreshWeapon()
    {
        if (handSocket != null && handSocket.childCount > 0)
        {
            currentWeapon = handSocket.GetChild(0).gameObject;

            if (currentWeapon.transform.parent == handSocket)
            {
                currentWeapon.SetActive(true);
            }
        }
    }

    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon != null) return;

        currentWeapon = Instantiate(weaponPrefab, handSocket);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }

    public void ThrowCurrentWeapon(float direction)
    {
        if (currentWeapon == null) return;

        GameObject thrown = currentWeapon;
        currentWeapon = null;

        thrown.transform.SetParent(null);
        Rigidbody2D rb = thrown.GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(new Vector2(direction * 15f, 5f), ForceMode2D.Impulse);
        rb.AddTorque(-direction * 20f, ForceMode2D.Impulse);
    }
}
