using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageBox damageBox = other.GetComponent<DamageBox>();

        if (damageBox == null)
        {
            damageBox = other.GetComponentInParent<DamageBox>();
        }

        if (damageBox != null)
        {
            damageBox.GetHit();
        }
    }
}
