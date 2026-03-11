using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public string targetTag;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Fighter agent = other.transform.root.GetComponent<Fighter>();
        CharacterBase character = other.transform.root.GetComponent<CharacterBase>();

        if (agent != null || character != null)
        {
            if (other.transform.root.CompareTag(targetTag))
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
