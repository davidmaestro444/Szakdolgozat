using UnityEngine;

public class DamageBox : MonoBehaviour
{
    public void GetHit()
    {
        Debug.Log(gameObject.name + " meghalt!");
        GetComponent<PlayerMovement>().enabled = false;
        StartCoroutine(RespawnPlayer(3.0f));
    }

    private System.Collections.IEnumerator RespawnPlayer(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log(gameObject.name + " újraéledt!");
        GetComponent<PlayerMovement>().enabled = true;
    }
}
