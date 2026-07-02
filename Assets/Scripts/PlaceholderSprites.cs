using UnityEngine;

public static class PlaceholderSprites
{
    private static Sprite pixel;
    private static Sprite circle;
    private static Sprite planet;
    private static Sprite star;
    private static Sprite asteroid;
    private static Sprite dysonSatellite;
    private static Sprite mine;
    private static Sprite condenser;
    private static Sprite harvester;
    private static Sprite dredger;
    private static Sprite collectorAutomaton;
    private static Sprite collectorHub;

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

    public static Sprite DysonSatellite
    {
        get
        {
            if (dysonSatellite == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Dyson Satellite";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color white = Color.white;
                Color grey = new Color(0.62f, 0.68f, 0.72f, 1f);
                Color dark = new Color(0.22f, 0.26f, 0.3f, 1f);

                FillRect(texture, 40, 40, 16, 16, white);
                FillRect(texture, 44, 34, 8, 28, grey);
                FillRect(texture, 18, 38, 20, 20, grey);
                FillRect(texture, 58, 38, 20, 20, grey);
                FillRect(texture, 12, 42, 6, 12, dark);
                FillRect(texture, 78, 42, 6, 12, dark);
                FillRect(texture, 28, 34, 8, 4, white);
                FillRect(texture, 60, 34, 8, 4, white);

                texture.Apply();

                dysonSatellite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return dysonSatellite;
        }
    }

    public static Sprite Mine
    {
        get
        {
            if (mine == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Mine";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color frame = new Color(0.83f, 0.9f, 0.98f, 1f);
                Color body = new Color(0.36f, 0.47f, 0.58f, 1f);
                Color dark = new Color(0.17f, 0.22f, 0.28f, 1f);
                Color glow = new Color(0.31f, 0.84f, 1f, 1f);

                FillRect(texture, 20, 20, 56, 14, dark);
                FillRect(texture, 24, 34, 48, 30, body);
                FillRect(texture, 28, 38, 40, 22, frame);
                FillRect(texture, 42, 52, 12, 22, dark);
                FillRect(texture, 34, 24, 8, 14, dark);
                FillRect(texture, 54, 24, 8, 14, dark);
                FillRect(texture, 44, 42, 8, 10, glow);

                texture.Apply();

                mine = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.22f),
                    size
                );
            }

            return mine;
        }
    }

    public static Sprite Condenser
    {
        get
        {
            if (condenser == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Condenser";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color frame = new Color(0.88f, 0.96f, 1f, 1f);
                Color body = new Color(0.38f, 0.58f, 0.72f, 1f);
                Color dark = new Color(0.16f, 0.22f, 0.32f, 1f);
                Color glow = new Color(0.44f, 0.92f, 1f, 1f);

                FillRect(texture, 22, 18, 52, 12, dark);
                FillRect(texture, 28, 30, 40, 28, body);
                FillRect(texture, 32, 26, 32, 8, frame);
                FillRect(texture, 38, 48, 20, 22, dark);
                FillRect(texture, 30, 58, 36, 8, frame);
                FillRect(texture, 42, 20, 12, 8, glow);
                FillRect(texture, 18, 40, 10, 18, glow);
                FillRect(texture, 68, 40, 10, 18, glow);

                texture.Apply();

                condenser = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.22f),
                    size
                );
            }

            return condenser;
        }
    }

    public static Sprite Harvester
    {
        get
        {
            if (harvester == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Harvester";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color frame = new Color(0.86f, 1f, 0.88f, 1f);
                Color body = new Color(0.32f, 0.58f, 0.36f, 1f);
                Color dark = new Color(0.17f, 0.24f, 0.2f, 1f);
                Color glow = new Color(0.54f, 1f, 0.56f, 1f);

                FillRect(texture, 24, 22, 48, 12, dark);
                FillRect(texture, 30, 34, 36, 26, body);
                FillRect(texture, 34, 28, 28, 8, frame);
                FillRect(texture, 16, 40, 16, 10, glow);
                FillRect(texture, 64, 40, 16, 10, glow);
                FillRect(texture, 40, 46, 16, 20, dark);
                FillRect(texture, 37, 18, 22, 8, glow);
                FillRect(texture, 34, 58, 28, 6, frame);

                texture.Apply();

                harvester = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.22f),
                    size
                );
            }

            return harvester;
        }
    }

    public static Sprite Dredger
    {
        get
        {
            if (dredger == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Dredger";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color frame = new Color(1f, 0.92f, 0.76f, 1f);
                Color body = new Color(0.62f, 0.48f, 0.26f, 1f);
                Color dark = new Color(0.24f, 0.18f, 0.12f, 1f);
                Color glow = new Color(1f, 0.74f, 0.34f, 1f);

                FillRect(texture, 20, 18, 56, 12, dark);
                FillRect(texture, 26, 30, 44, 24, body);
                FillRect(texture, 30, 24, 36, 8, frame);
                FillRect(texture, 36, 50, 24, 20, dark);
                FillRect(texture, 18, 40, 12, 20, glow);
                FillRect(texture, 66, 40, 12, 20, glow);
                FillRect(texture, 40, 44, 16, 8, frame);
                FillRect(texture, 34, 58, 28, 6, glow);

                texture.Apply();

                dredger = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.22f),
                    size
                );
            }

            return dredger;
        }
    }

    public static Sprite CollectorAutomaton
    {
        get
        {
            if (collectorAutomaton == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Collector Automaton";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color frame = new Color(0.78f, 0.98f, 1f, 1f);
                Color body = new Color(0.18f, 0.48f, 0.62f, 1f);
                Color dark = new Color(0.08f, 0.16f, 0.22f, 1f);
                Color glow = new Color(0.45f, 1f, 0.92f, 1f);

                FillRect(texture, 38, 18, 20, 14, frame);
                FillRect(texture, 28, 32, 40, 30, body);
                FillRect(texture, 34, 38, 28, 16, dark);
                FillRect(texture, 42, 42, 12, 8, glow);
                FillRect(texture, 20, 40, 10, 18, frame);
                FillRect(texture, 66, 40, 10, 18, frame);
                FillRect(texture, 30, 62, 10, 12, dark);
                FillRect(texture, 56, 62, 10, 12, dark);

                texture.Apply();

                collectorAutomaton = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return collectorAutomaton;
        }
    }

    public static Sprite CollectorHub
    {
        get
        {
            if (collectorHub == null)
            {
                const int size = 96;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Runtime Placeholder Collector Hub";
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                Color frame = new Color(1f, 0.92f, 0.64f, 1f);
                Color body = new Color(0.56f, 0.45f, 0.22f, 1f);
                Color dark = new Color(0.2f, 0.16f, 0.1f, 1f);
                Color glow = new Color(1f, 0.82f, 0.26f, 1f);

                FillRect(texture, 20, 26, 56, 44, body);
                FillRect(texture, 26, 20, 44, 8, frame);
                FillRect(texture, 26, 70, 44, 8, frame);
                FillRect(texture, 28, 34, 40, 24, dark);
                FillRect(texture, 36, 40, 24, 12, glow);
                FillRect(texture, 16, 38, 8, 20, frame);
                FillRect(texture, 72, 38, 8, 20, frame);

                texture.Apply();

                collectorHub = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }

            return collectorHub;
        }
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int iy = y; iy < y + height; iy++)
        {
            for (int ix = x; ix < x + width; ix++)
            {
                texture.SetPixel(ix, iy, color);
            }
        }
    }
}
