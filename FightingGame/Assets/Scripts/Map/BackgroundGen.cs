using UnityEngine;

public class BackgroundGen : MonoBehaviour
{
    public int textureWidth = 1024;
    public int textureHeight = 256;
    public float noiseScale = 0.005f;
    public float seed;
    [Range(1, 8)]
    public int octaves = 4;
    [Range(0f, 1f)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Color groundColor = new Color(0.76f, 0.68f, 0.51f, 1f);
    public float baseTrainSpeed = 5f;
    [Range(0, 1)]
    public float parallaxFactor = 0.5f;

    private Texture2D texture;
    private Color[] pixelData;
    private MeshRenderer quadRenderer;
    private int nextColumnToGenerate = 0;
    private float totalOffset = 0;

    void Start()
    {
        quadRenderer = GetComponent<MeshRenderer>();
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        pixelData = new Color[textureWidth * textureHeight];
        if (seed == 0) { seed = Random.Range(0f, 100f); }

        for (int x = 0; x < textureWidth; x++)
        {
            GenerateColumn(x, x);
        }
        ApplyPixelDataToTexture();
        quadRenderer.material.mainTexture = texture;
    }

    void Update()
    {
        float scrollThisFrame = (baseTrainSpeed * (1 - parallaxFactor)) * Time.deltaTime;
        totalOffset += scrollThisFrame;
        quadRenderer.material.mainTextureOffset = new Vector2(totalOffset, 0);

        int currentGlobalPixel = Mathf.FloorToInt(totalOffset * textureWidth);
        int pixelsToGenerate = currentGlobalPixel - nextColumnToGenerate;

        if (pixelsToGenerate > 0)
        {
            for (int i = 0; i < pixelsToGenerate; i++)
            {
                int globalColumnIndex = nextColumnToGenerate + i;
                int localColumnIndex = globalColumnIndex % textureWidth;
                GenerateColumn(localColumnIndex, globalColumnIndex);
            }

            ApplyPixelDataToTexture();
            nextColumnToGenerate += pixelsToGenerate;
        }
    }

    void GenerateColumn(int localX, int globalX)
    {
        float totalNoiseValue = 0;
        float frequency = noiseScale;
        float amplitude = 1f;
        float maxAmplitude = 0;

        for (int i = 0; i < octaves; i++)
        {
            float noiseX = ((float)globalX * frequency) + seed;
            float perlinValue = Mathf.PerlinNoise(noiseX, seed);

            totalNoiseValue += perlinValue * amplitude;
            maxAmplitude += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float normalizedNoise = totalNoiseValue / maxAmplitude;
        int height = Mathf.RoundToInt(normalizedNoise * textureHeight);

        for (int y = 0; y < textureHeight; y++)
        {
            int pixelIndex = y * textureWidth + localX;
            if (y < height)
            {
                pixelData[pixelIndex] = groundColor;
            }
            else
            {
                pixelData[pixelIndex] = Color.clear;
            }
        }
    }

    void ApplyPixelDataToTexture()
    {
        texture.SetPixels(pixelData);
        texture.Apply();
    }
}
