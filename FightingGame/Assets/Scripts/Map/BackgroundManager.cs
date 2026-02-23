using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public GameObject[] chunkPrefabs;
    public float baseTrainSpeed = 5f;
    [Range(0, 1)]
    public float parallaxFactor = 0.8f;
    public int chunksOnScreen = 4;
    public float chunkWidth = 19.2f;
    public Transform cameraTransform;
    private List<GameObject> activeChunks = new List<GameObject>();
    private float autoScrollOffset = 0f;
    public float yOffset = 0f;
    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        float cameraHeight = Camera.main.orthographicSize * 2f;

        for (int i = 0; i < chunksOnScreen; i++)
        {
            GameObject newChunk = Instantiate(chunkPrefabs[Random.Range(0, chunkPrefabs.Length)]);
            newChunk.transform.SetParent(this.transform);
            SpriteRenderer sr = newChunk.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float spriteHeight = sr.sprite.bounds.size.y;
                newChunk.transform.localScale = new Vector3(1, cameraHeight / spriteHeight, 1);
            }
            activeChunks.Add(newChunk);
        }
    }

    void LateUpdate()
    {
        autoScrollOffset += baseTrainSpeed * Time.deltaTime;
        float cameraMoveOffset = cameraTransform.position.x * (1 - parallaxFactor);
        float totalOffset = autoScrollOffset + cameraMoveOffset;

        if (totalOffset >= chunkWidth)
        {
            autoScrollOffset -= chunkWidth;
            GameObject first = activeChunks[0];
            activeChunks.RemoveAt(0);
            activeChunks.Add(first);
        }

        for (int i = 0; i < activeChunks.Count; i++)
        {
            float xPos = cameraTransform.position.x + (i * chunkWidth) - (totalOffset % chunkWidth) - (chunkWidth / 2);
            activeChunks[i].transform.position = new Vector3(xPos, cameraTransform.position.y + yOffset, transform.position.z);
        }
    }
}
