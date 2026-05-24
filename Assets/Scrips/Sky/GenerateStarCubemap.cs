using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GenerateStarCubemap : MonoBehaviour
{
    [ContextMenu("Generate Star Cubemap")]
    public void Generate()
    {
#if UNITY_EDITOR
        int size = 2048;
        int starsPerFace = 130;

        Cubemap cubemap = new Cubemap(size, TextureFormat.RGBA32, false);

        for (int face = 0; face < 6; face++)
        {
            Color[] pixels = new Color[size * size];

            Color skyColor = new Color(0.0005f, 0.001f, 0.006f, 1f);

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = skyColor;

            for (int i = 0; i < starsPerFace; i++)
            {
                int x = Random.Range(0, size);
                int y = Random.Range(0, size);

                float brightness = Random.Range(1.1f, 2.2f);
                Color star = new Color(
                    0.75f * brightness,
                    0.85f * brightness,
                    1.25f * brightness,
                    1f
                );

                SetStar(pixels, size, x, y, star);
            }

            cubemap.SetPixels(pixels, (CubemapFace)face);
        }

        cubemap.Apply();

        AssetDatabase.CreateAsset(cubemap, "Assets/Generated_StarCubemap.cubemap");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Generated_StarCubemap created in Assets.");
#endif
    }

    private void SetStar(Color[] pixels, int size, int cx, int cy, Color color)
    {
        float radius = Random.value > 0.88f ? 2.2f : 1.2f;

        for (int y = -3; y <= 3; y++)
        {
            for (int x = -3; x <= 3; x++)
            {
                float distance = Mathf.Sqrt(x * x + y * y);

                if (distance <= radius)
                {
                    float falloff = 1f - (distance / radius);
                    Color softColor = color * Mathf.Lerp(0.25f, 1f, falloff);
                    softColor.a = 1f;

                    SetPixel(pixels, size, cx + x, cy + y, softColor);
                }
            }
        }
    }

    private void SetPixel(Color[] pixels, int size, int x, int y, Color color)
    {
        if (x < 0 || y < 0 || x >= size || y >= size) return;
        pixels[y * size + x] = color;
    }
}