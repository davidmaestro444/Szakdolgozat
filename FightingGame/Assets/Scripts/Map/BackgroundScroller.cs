using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    //pozitív jobbra görget, negatív balra
    public float scrollSpeed = 0.1f;
    private Renderer quadRenderer;

    void Start()
    {
        quadRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        float offsetX = Time.time * scrollSpeed;
        Vector2 offset = new Vector2(offsetX, 0);
        quadRenderer.material.mainTextureOffset = offset;
    }
}
