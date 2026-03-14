using System.Collections.Generic;
using UnityEngine;

public class WeaponThrow : MonoBehaviour
{
    private static List<GameObject> thrownWeapons = new List<GameObject>();
    public static int maxThrownWeapons = 4;

    public void WeaponThrown()
    {
        for (int i = thrownWeapons.Count - 1; i >= 0; i--)
        {
            if (thrownWeapons[i] == null)
            {
                thrownWeapons.RemoveAt(i);
            }
        }

        thrownWeapons.Add(this.gameObject);

        if (thrownWeapons.Count > maxThrownWeapons)
        {
            GameObject oldestWeapon = thrownWeapons[0];
            thrownWeapons.RemoveAt(0);
            if (oldestWeapon != null && oldestWeapon.transform.parent == null)
            {
                Destroy(oldestWeapon);
            }
        }
    }

    public void WeaponPickUp()
    {
        if (thrownWeapons.Contains(this.gameObject))
        {
            thrownWeapons.Remove(this.gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        if (transform.parent == null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        WeaponPickUp();
    }
}
