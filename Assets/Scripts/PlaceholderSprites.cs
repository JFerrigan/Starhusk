using UnityEngine;

public static class PlaceholderSprites
{
    private static Sprite pixel;
    private static Sprite circle;

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
}
