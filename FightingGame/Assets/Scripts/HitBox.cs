using UnityEngine;

public class HitBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageBox targetDamageBox = other.GetComponentInParent<DamageBox>();

        if (targetDamageBox != null)
        {
            if (transform.root == other.transform.root)
            {
                return;
            }
            targetDamageBox.GetHit();
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}
