using UnityEngine;

public static class GameUiScale
{
    // Tune all in-game IMGUI scale from here.
    public const float SizeMultiplier = 1.1f;
    public const float MinScale = 0.8f;
    public const float MaxScale = 1.5f;
    public const float ReferenceWidth = 1280f;
    public const float ReferenceHeight = 720f;

    public static float Current => ForScreen(Screen.width, Screen.height);

    public static float ForScreen(float screenWidth, float screenHeight)
    {
        return ForScreen(screenWidth, screenHeight, SizeMultiplier, MinScale, MaxScale);
    }

    public static float ForScreen(float screenWidth, float screenHeight, float sizeMultiplier, float minScale, float maxScale)
    {
        float min = Mathf.Min(minScale, maxScale);
        float max = Mathf.Max(minScale, maxScale);
        float fit = Mathf.Min(Mathf.Max(1f, screenWidth) / ReferenceWidth, Mathf.Max(1f, screenHeight) / ReferenceHeight);
        return Mathf.Clamp(fit * Mathf.Max(0.01f, sizeMultiplier), min, max);
    }

    public static int Font(float baseSize)
    {
        return Mathf.Max(1, Mathf.RoundToInt(baseSize * Current));
    }

    public static int Font(float baseSize, float scale)
    {
        return Mathf.Max(1, Mathf.RoundToInt(baseSize * Mathf.Max(0.01f, scale)));
    }

    public static float Size(float baseSize)
    {
        return Size(baseSize, Current);
    }

    public static float Size(float baseSize, float scale)
    {
        return Mathf.Max(1f, baseSize * Mathf.Max(0.01f, scale));
    }

    public static Rect Rect(float x, float y, float width, float height)
    {
        float scale = Current;
        return new Rect(Size(x, scale), Size(y, scale), Size(width, scale), Size(height, scale));
    }

    public static Rect Inset(Rect rect, float horizontal, float vertical)
    {
        float xInset = Size(horizontal);
        float yInset = Size(vertical);
        return new Rect(rect.x + xInset, rect.y + yInset, Mathf.Max(1f, rect.width - (xInset * 2f)), Mathf.Max(1f, rect.height - (yInset * 2f)));
    }

    public static Rect ClampRectToScreen(Vector2 position, Vector2 size, float margin = 4f)
    {
        float scaledMargin = Size(margin);
        return new Rect(
            Mathf.Clamp(position.x, scaledMargin, Mathf.Max(scaledMargin, Screen.width - size.x - scaledMargin)),
            Mathf.Clamp(position.y, scaledMargin, Mathf.Max(scaledMargin, Screen.height - size.y - scaledMargin)),
            size.x,
            size.y);
    }

    public static Rect FullMapUiRect(Rect mapRect)
    {
        float scale = Current;
        return FullMapUiRect(mapRect, scale);
    }

    public static Rect FullMapUiRect(Rect mapRect, float scale)
    {
        return new Rect(mapRect.x + Size(12f, scale), mapRect.y + Size(10f, scale), Size(380f, scale), Size(64f, scale));
    }

    public static int GridColumnCount(float contentWidth, float baseCardWidth, float baseSpacing, float scale)
    {
        return PixelUiSprites.GridColumnCount(contentWidth, Size(baseCardWidth, scale), Size(baseSpacing, scale));
    }

    public static float GridContentHeight(int itemCount, float contentWidth, float baseCardWidth, float baseCardHeight, float baseSpacing, float scale)
    {
        return PixelUiSprites.GridContentHeight(itemCount, contentWidth, Size(baseCardWidth, scale), Size(baseCardHeight, scale), Size(baseSpacing, scale));
    }
}
