using UnityEngine;

public class DamageBox : MonoBehaviour
{
    public void GetHit()
    {
        GameManager.Instance.OnCharacterDied(gameObject);
        gameObject.SetActive(false);
    }
}
