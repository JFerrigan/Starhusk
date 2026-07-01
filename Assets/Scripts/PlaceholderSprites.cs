using UnityEngine;

public static class PlaceholderSprites
{
    private static Sprite pixel;
    private static Sprite circle;
    private static Sprite planet;
    private static Sprite star;
    private static Sprite asteroid;

    public static Sprite Pixel
    {
        get
        {
            if (pixel == null)
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.name = "Runtime Placeholder Pixel";
                texture.filterMode = FilterMode.Point;
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                pixel = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f
                );
            }

            return pixel;
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (circle == null)
            {
                const int size = 64;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Circle";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                float radius = (size - 1) * 0.5f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                        float alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(0.92f, 1f, distance));
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }

                texture.Apply();

                circle = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return circle;
        }
    }

    public static Sprite Planet
    {
        get
        {
            if (planet == null)
            {
                const int size = 256;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Planet";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                float radius = (size - 1) * 0.48f;
                Vector2 lightDirection = new Vector2(-0.45f, 0.9f).normalized;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 pixelPosition = new Vector2(x, y);
                        Vector2 fromCenter = pixelPosition - center;
                        float normalizedDistance = fromCenter.magnitude / radius;

                        if (normalizedDistance > 1f)
                        {
                            texture.SetPixel(x, y, Color.clear);
                            continue;
                        }

                        Vector2 normal = fromCenter.normalized;
                        float light = Mathf.InverseLerp(-0.7f, 1f, Vector2.Dot(normal, lightDirection));
                        float rim = Mathf.SmoothStep(0.86f, 1f, normalizedDistance);
                        float banding = Mathf.PerlinNoise((x * 0.035f) + 7.2f, (y * 0.02f) + 3.1f);
                        float shade = Mathf.Lerp(0.74f, 1.08f, light) + ((banding - 0.5f) * 0.08f);
                        shade = Mathf.Lerp(shade, shade * 0.62f, rim);

                        texture.SetPixel(x, y, new Color(shade, shade, shade, 1f));
                    }
                }

                texture.Apply();

                planet = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return planet;
        }
    }

    public static Sprite Star
    {
        get
        {
            if (star == null)
            {
                const int size = 256;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Star";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                float radius = (size - 1) * 0.47f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 fromCenter = new Vector2(x, y) - center;
                        float normalizedDistance = fromCenter.magnitude / radius;

                        if (normalizedDistance > 1f)
                        {
                            texture.SetPixel(x, y, Color.clear);
                            continue;
                        }

                        float core = 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);
                        float surfaceNoise = Mathf.PerlinNoise((x * 0.045f) + 18.4f, (y * 0.045f) + 11.7f);
                        float hotSpot = Mathf.SmoothStep(0.42f, 0.9f, surfaceNoise) * 0.2f;
                        float rim = Mathf.SmoothStep(0.82f, 1f, normalizedDistance);
                        float brightness = Mathf.Clamp01(0.82f + (core * 0.32f) + hotSpot - (rim * 0.12f));

                        texture.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                    }
                }

                texture.Apply();

                star = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return star;
        }
    }

    public static Sprite Asteroid
    {
        get
        {
            if (asteroid == null)
            {
                const int size = 128;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Asteroid";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                Vector2 lightDirection = new Vector2(-0.55f, 0.75f).normalized;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 fromCenter = new Vector2(x, y) - center;
                        float angle = Mathf.Atan2(fromCenter.y, fromCenter.x);
                        float distance = fromCenter.magnitude / ((size - 1) * 0.42f);
                        float edgeNoise = Mathf.PerlinNoise(
                            (Mathf.Cos(angle) * 2.8f) + 12.1f,
                            (Mathf.Sin(angle) * 2.8f) + 6.4f
                        );
                        float radius = Mathf.Lerp(0.78f, 1.16f, edgeNoise);

                        if (distance > radius)
                        {
                            texture.SetPixel(x, y, Color.clear);
                            continue;
                        }

                        Vector2 normal = fromCenter.sqrMagnitude > 0.001f ? fromCenter.normalized : Vector2.up;
                        float light = Mathf.InverseLerp(-0.85f, 1f, Vector2.Dot(normal, lightDirection));
                        float surface = Mathf.PerlinNoise((x * 0.09f) + 22.7f, (y * 0.09f) + 9.3f);
                        float cracks = Mathf.PerlinNoise((x * 0.19f) + 4.6f, (y * 0.19f) + 31.2f);
                        float rim = Mathf.SmoothStep(0.72f, 1f, distance / radius);
                        float shade = Mathf.Lerp(0.62f, 1.04f, light);
                        shade += (surface - 0.5f) * 0.18f;
                        shade -= Mathf.SmoothStep(0.62f, 0.78f, cracks) * 0.16f;
                        shade = Mathf.Lerp(shade, shade * 0.56f, rim);
                        shade = Mathf.Clamp01(shade);

                        texture.SetPixel(x, y, new Color(shade, shade, shade, 1f));
                    }
                }

                texture.Apply();

                asteroid = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return asteroid;
        }
    }
}
