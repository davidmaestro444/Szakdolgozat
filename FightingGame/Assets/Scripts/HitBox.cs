using UnityEngine;

public class HitBox : MonoBehaviour
{
    /*private void OnTriggerEnter2D(Collider2D other)
    {
        DamageBox damagebox = other.GetComponent<DamageBox>();
        if (damagebox != null)
        {
            damagebox.GetHit();
            gameObject.SetActive(false);
        }
    }*/
    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageBox damagebox = other.GetComponentInParent<DamageBox>();

        if (damagebox != null)
        {
            if (transform.root == other.transform.root)
            {
                return;
            }

            damagebox.GetHit();
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
