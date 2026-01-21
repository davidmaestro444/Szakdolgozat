using UnityEngine;

public class ProcBackgroundGen : MonoBehaviour
{
    [Header("Dimensions")]
    [Tooltip("A generált textúra szélessége pixelben.")]
    public int textureWidth = 512;
    [Tooltip("A generált textúra magassága pixelben.")]
    public int textureHeight = 256;

    [Header("Noise Settings")]
    [Tooltip("A zaj 'nagyítása'. Kisebb érték = simább, elnyújtottabb táj. Nagyobb érték = sûrûbb, hegyesebb táj.")]
    public float noiseScale = 0.1f;
    [Tooltip("A generálás kiindulópontja. Ugyanazzal a seed-del mindig ugyanazt a tájat kapod.")]
    public float seed;

    [Header("Color Settings")]
    [Tooltip("A 'föld' színe.")]
    public Color groundColor = Color.black;
    [Tooltip("Az 'ég' színe.")]
    public Color skyColor = Color.blue;

    [Tooltip("A Quad objektum MeshRenderer komponense, amire a textúrát tesszük.")]
    [SerializeField] private MeshRenderer backgroundQuadRenderer;

    void Start()
    {
        if (seed == 0)
        {
            seed = Random.Range(0f, 100f);
        }

        GenerateAndApplyTexture();
    }

    public void GenerateAndApplyTexture()
    {
        Texture2D texture = GenerateTexture();
        texture.wrapMode = TextureWrapMode.Repeat;

        if (backgroundQuadRenderer != null)
        {
            backgroundQuadRenderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("Nincs beállítva a Background Quad Renderer a szkriptben!");
        }
    }

    private Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        for (int x = 0; x < textureWidth; x++)
        {
            float perlinX = (float)x * noiseScale + seed;
            float perlinValue = Mathf.PerlinNoise(perlinX, seed);
            int height = Mathf.RoundToInt(perlinValue * textureHeight);

            for (int y = 0; y < textureHeight; y++)
            {
                if (y < height)
                {
                    texture.SetPixel(x, y, groundColor);
                }
                else
                {
                    texture.SetPixel(x, y, skyColor);
                }
            }
        }

        texture.Apply();
        return texture;
    }
}
