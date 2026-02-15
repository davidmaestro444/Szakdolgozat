using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;
    public Transform[] spawnPoints;
    public float interval = 15f;
    private GameObject currentItem;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (currentItem != null && currentItem.transform.parent == null)
            {
                Destroy(currentItem);
            }

            int randItem = Random.Range(0, itemPrefabs.Length);
            int randPoint = Random.Range(0, spawnPoints.Length);
            currentItem = Instantiate(itemPrefabs[randItem], spawnPoints[randPoint].position, Quaternion.identity);
        }
    }
}
