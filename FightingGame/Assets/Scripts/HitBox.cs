using UnityEngine;

public class HitBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageBox damagebox = other.GetComponent<DamageBox>();
        if (damagebox != null)
        {
            damagebox.GetHit();
            gameObject.SetActive(false);
        }
    }
}
