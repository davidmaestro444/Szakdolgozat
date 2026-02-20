using UnityEngine;

public class WeaponThrow : MonoBehaviour
{
    private void OnBecameInvisible()
    {
        if (transform.parent == null)
        {
            Destroy(gameObject);
        }
    }
}
