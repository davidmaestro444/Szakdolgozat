using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public GameObject[] rockPrefabs;
    public float minSpawnTime = 3f;
    public float maxSpawnTime = 8f;
    public float spawnXOffset = 25f;
    public float destroyXOffset = -25f;
    public float baseTrainSpeed = 5f;
    [Range(0, 1)]
    public float parallaxFactor = 0.2f;
    public Transform cameraTransform;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            SpawnRock();
        }
    }

    void SpawnRock()
    {
        if (rockPrefabs.Length == 0) return;

        GameObject prefabToSpawn = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
        Vector3 spawnPos = new Vector3(cameraTransform.position.x + spawnXOffset, cameraTransform.position.y, transform.position.z);
        GameObject newRock = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        newRock.transform.SetParent(this.transform);
        RockMover mover = newRock.AddComponent<RockMover>();
        mover.moveSpeed = baseTrainSpeed * (1 - parallaxFactor);
        mover.destroyX = destroyXOffset;
        mover.cameraTransform = cameraTransform;
    }
}

public class RockMover : MonoBehaviour
{
    public float moveSpeed;
    public float destroyX;
    public Transform cameraTransform;

    void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        if (transform.position.x < cameraTransform.position.x + destroyX)
        {
            Destroy(gameObject);
        }
    }
}
