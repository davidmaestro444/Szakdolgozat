using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public string targetTag;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root.CompareTag(targetTag))
        {
            bool isRealCharacter = other.transform.root.GetComponent<PlayerMovement>() != null ||other.transform.root.GetComponent<EnemyAI>() != null;

            if (isRealCharacter)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndGame(targetTag);
                }
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}
