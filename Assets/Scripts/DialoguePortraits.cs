using UnityEngine;

public static class DialoguePortraits
{
    private static Texture2D council;
    private static Texture2D unknown;

    public static Texture2D GetPortrait(string portraitId)
    {
        if (portraitId == "council")
        {
            return council == null ? council = CreateCouncilPortrait() : council;
        }

        return unknown == null ? unknown = CreateUnknownPortrait() : unknown;
    }

    private static Texture2D CreateCouncilPortrait()
    {
        Texture2D texture = NewPortrait("Runtime Dialogue Portrait Council");
        Clear(texture, new Color(0.025f, 0.018f, 0.055f, 1f));
        DrawFrame(texture, PixelUiSprites.Cyan, PixelUiSprites.Gold);
        FillRect(texture, 14, 36, 36, 10, new Color(0.24f, 0.1f, 0.38f, 1f));
        FillRect(texture, 18, 22, 28, 18, new Color(0.12f, 0.2f, 0.26f, 1f));
        FillRect(texture, 22, 18, 20, 6, PixelUiSprites.Gold);
        FillRect(texture, 20, 28, 6, 4, PixelUiSprites.Cyan);
        FillRect(texture, 38, 28, 6, 4, PixelUiSprites.Cyan);
        FillRect(texture, 29, 35, 6, 2, new Color(0.95f, 0.78f, 1f, 1f));

        for (int i = 0; i < 9; i++)
        {
            int x = 10 + (i * 5);
            int y = 9 + ((i * 7) % 12);
            texture.SetPixel(x, y, i % 2 == 0 ? PixelUiSprites.Cyan : PixelUiSprites.Magenta);
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateUnknownPortrait()
    {
        Texture2D texture = NewPortrait("Runtime Dialogue Portrait Unknown");
        Clear(texture, new Color(0.03f, 0.024f, 0.045f, 1f));
        DrawFrame(texture, PixelUiSprites.Muted, PixelUiSprites.Cyan);
        FillRect(texture, 20, 19, 24, 24, new Color(0.08f, 0.07f, 0.11f, 1f));
        FillRect(texture, 28, 25, 8, 4, PixelUiSprites.Muted);
        FillRect(texture, 31, 29, 4, 8, PixelUiSprites.Muted);
        FillRect(texture, 31, 40, 4, 4, PixelUiSprites.Muted);
        texture.Apply();
        return texture;
    }

    private static Texture2D NewPortrait(string name)
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.name = name;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static void Clear(Texture2D texture, Color color)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }

    private static void DrawFrame(Texture2D texture, Color bright, Color accent)
    {
        FillRect(texture, 0, 0, texture.width, 2, bright);
        FillRect(texture, 0, 0, 2, texture.height, bright);
        FillRect(texture, 0, texture.height - 2, texture.width, 2, PixelUiSprites.DarkPurple);
        FillRect(texture, texture.width - 2, 0, 2, texture.height, PixelUiSprites.DarkPurple);
        FillRect(texture, 5, 5, 12, 2, accent);
        FillRect(texture, 5, 5, 2, 12, accent);
        FillRect(texture, texture.width - 17, 5, 12, 2, accent);
        FillRect(texture, texture.width - 7, 5, 2, 12, accent);
        FillRect(texture, 5, texture.height - 7, 12, 2, accent);
        FillRect(texture, 5, texture.height - 17, 2, 12, accent);
        FillRect(texture, texture.width - 17, texture.height - 7, 12, 2, accent);
        FillRect(texture, texture.width - 7, texture.height - 17, 2, 12, accent);
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        int xMin = Mathf.Max(0, x);
        int yMin = Mathf.Max(0, y);
        int xMax = Mathf.Min(texture.width, x + width);
        int yMax = Mathf.Min(texture.height, y + height);

        for (int iy = yMin; iy < yMax; iy++)
        {
            for (int ix = xMin; ix < xMax; ix++)
            {
                texture.SetPixel(ix, iy, color);
            }
        }
    }
}
