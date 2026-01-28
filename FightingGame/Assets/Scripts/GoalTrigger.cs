using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public string targetTag;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(targetTag);
            }
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
