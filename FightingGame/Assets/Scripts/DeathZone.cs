using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageBox db = other.GetComponentInParent<DamageBox>();
        if (db != null)
        {
            db.GetHit(false);
        }
        else if (other.CompareTag("Sword") || other.CompareTag("Spear"))
        {
            Destroy(other.gameObject);
        }
    }
}
