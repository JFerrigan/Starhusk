using UnityEngine;

public enum PixelUiFrame
{
    Panel,
    InnerPanel,
    Header,
    Card,
    CardHover,
    CardDisabled,
    CardUnlocked,
    Tab,
    TabHover,
    TabActive,
    Button,
    ButtonHover,
    Badge,
    WarningBadge,
    Chip
}

public static class PixelUiSprites
{
    public static readonly Color Deep = new Color(0.035f, 0.016f, 0.075f, 1f);
    public static readonly Color Inset = new Color(0.02f, 0.012f, 0.045f, 1f);
    public static readonly Color Purple = new Color(0.18f, 0.07f, 0.31f, 1f);
    public static readonly Color DarkPurple = new Color(0.09f, 0.045f, 0.14f, 1f);
    public static readonly Color Magenta = new Color(0.86f, 0.28f, 1f, 1f);
    public static readonly Color Cyan = new Color(0.24f, 0.92f, 1f, 1f);
    public static readonly Color Gold = new Color(1f, 0.78f, 0.26f, 1f);
    public static readonly Color Warning = new Color(1f, 0.32f, 0.18f, 1f);
    public static readonly Color Muted = new Color(0.35f, 0.28f, 0.43f, 1f);

    private static Texture2D panel;
    private static Texture2D innerPanel;
    private static Texture2D header;
    private static Texture2D card;
    private static Texture2D cardHover;
    private static Texture2D cardDisabled;
    private static Texture2D cardUnlocked;
    private static Texture2D tab;
    private static Texture2D tabHover;
    private static Texture2D tabActive;
    private static Texture2D button;
    private static Texture2D buttonHover;
    private static Texture2D badge;
    private static Texture2D warningBadge;
    private static Texture2D chip;

    public static Texture2D TextureFor(PixelUiFrame frame)
    {
        switch (frame)
        {
            case PixelUiFrame.InnerPanel:
                return innerPanel == null ? innerPanel = CreatePanel("Runtime Pixel UI Inner Panel", 32, Inset, DarkPurple, Cyan, false) : innerPanel;
            case PixelUiFrame.Header:
                return header == null ? header = CreatePanel("Runtime Pixel UI Header", 48, Purple, DarkPurple, Magenta, true) : header;
            case PixelUiFrame.Card:
                return card == null ? card = CreateCard("Runtime Pixel UI Card", Purple, Magenta, Cyan, false, false) : card;
            case PixelUiFrame.CardHover:
                return cardHover == null ? cardHover = CreateCard("Runtime Pixel UI Card Hover", new Color(0.24f, 0.1f, 0.38f, 1f), Cyan, Gold, true, false) : cardHover;
            case PixelUiFrame.CardDisabled:
                return cardDisabled == null ? cardDisabled = CreateCard("Runtime Pixel UI Card Disabled", DarkPurple, Muted, Warning, false, true) : cardDisabled;
            case PixelUiFrame.CardUnlocked:
                return cardUnlocked == null ? cardUnlocked = CreateCard("Runtime Pixel UI Card Unlocked", new Color(0.05f, 0.18f, 0.12f, 1f), Gold, Cyan, true, false) : cardUnlocked;
            case PixelUiFrame.Tab:
                return tab == null ? tab = CreateButton("Runtime Pixel UI Tab", 48, DarkPurple, Muted, Magenta) : tab;
            case PixelUiFrame.TabHover:
                return tabHover == null ? tabHover = CreateButton("Runtime Pixel UI Tab Hover", 48, Purple, Cyan, Magenta) : tabHover;
            case PixelUiFrame.TabActive:
                return tabActive == null ? tabActive = CreateButton("Runtime Pixel UI Tab Active", 48, new Color(0.24f, 0.1f, 0.38f, 1f), Gold, Cyan) : tabActive;
            case PixelUiFrame.Button:
                return button == null ? button = CreateButton("Runtime Pixel UI Button", 48, Purple, Magenta, Cyan) : button;
            case PixelUiFrame.ButtonHover:
                return buttonHover == null ? buttonHover = CreateButton("Runtime Pixel UI Button Hover", 48, new Color(0.22f, 0.1f, 0.32f, 1f), Cyan, Gold) : buttonHover;
            case PixelUiFrame.Badge:
                return badge == null ? badge = CreateButton("Runtime Pixel UI Badge", 32, new Color(0.12f, 0.19f, 0.12f, 1f), Gold, Cyan) : badge;
            case PixelUiFrame.WarningBadge:
                return warningBadge == null ? warningBadge = CreateButton("Runtime Pixel UI Warning Badge", 32, new Color(0.24f, 0.07f, 0.06f, 1f), Warning, Gold) : warningBadge;
            case PixelUiFrame.Chip:
                return chip == null ? chip = CreateChip() : chip;
            case PixelUiFrame.Panel:
            default:
                return panel == null ? panel = CreatePanel("Runtime Pixel UI Panel", 64, Deep, Purple, Magenta, true) : panel;
        }
    }

    public static void Draw(Rect rect, PixelUiFrame frame)
    {
        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, TextureFor(frame), ScaleMode.StretchToFill, true);
        GUI.color = previous;
    }

    public static int GridColumnCount(float contentWidth, float cardWidth, float spacing)
    {
        float total = Mathf.Max(1f, contentWidth + spacing);
        return Mathf.Max(1, Mathf.FloorToInt(total / Mathf.Max(1f, cardWidth + spacing)));
    }

    public static float GridContentHeight(int itemCount, float contentWidth, float cardWidth, float cardHeight, float spacing)
    {
        if (itemCount <= 0)
        {
            return cardHeight;
        }

        int columns = GridColumnCount(contentWidth, cardWidth, spacing);
        int rows = Mathf.CeilToInt(itemCount / (float)columns);
        return (rows * cardHeight) + (Mathf.Max(0, rows - 1) * spacing);
    }

    private static Texture2D CreatePanel(string name, int size, Color fill, Color shade, Color line, bool stars)
    {
        Texture2D texture = NewTexture(name, size, size);
        Clear(texture, fill);
        DrawBorder(texture, line, shade, 2);
        DrawCornerBrackets(texture, Cyan);
        DrawRivets(texture, Gold);

        for (int y = 5; y < size - 5; y += 7)
        {
            for (int x = 5; x < size - 5; x += 7)
            {
                if (((x * 17) + (y * 31)) % 5 == 0)
                {
                    texture.SetPixel(x, y, shade);
                }
            }
        }

        if (stars)
        {
            for (int i = 0; i < 18; i++)
            {
                int x = 5 + ((i * 11) % (size - 10));
                int y = 5 + ((i * 19) % (size - 10));
                texture.SetPixel(x, y, i % 3 == 0 ? Cyan : new Color(0.55f, 0.46f, 0.68f, 1f));
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateCard(string name, Color fill, Color line, Color accent, bool lit, bool disabled)
    {
        const int width = 48;
        const int height = 56;
        Texture2D texture = NewTexture(name, width, height);
        Clear(texture, fill);
        DrawBorder(texture, line, disabled ? Muted : DarkPurple, 2);
        DrawCornerBrackets(texture, accent);

        FillRect(texture, 5, 5, width - 10, height - 22, disabled ? new Color(0.055f, 0.035f, 0.075f, 1f) : Inset);
        FillRect(texture, 5, height - 15, width - 10, 10, disabled ? DarkPurple : new Color(0.12f, 0.045f, 0.19f, 1f));
        FillRect(texture, 7, height - 13, width - 14, 1, disabled ? Muted : accent);

        if (lit)
        {
            FillRect(texture, 3, 9, 1, 10, accent);
            FillRect(texture, width - 4, height - 19, 1, 10, accent);
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateButton(string name, int width, Color fill, Color line, Color accent)
    {
        const int height = 16;
        Texture2D texture = NewTexture(name, width, height);
        Clear(texture, fill);
        DrawBorder(texture, line, DarkPurple, 1);
        FillRect(texture, 3, 3, width - 6, 1, accent);
        FillRect(texture, 4, height - 4, width - 8, 1, new Color(0f, 0f, 0f, 0.35f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateChip()
    {
        const int width = 32;
        const int height = 14;
        Texture2D texture = NewTexture("Runtime Pixel UI Resource Chip", width, height);
        Clear(texture, new Color(0.03f, 0.018f, 0.055f, 1f));
        DrawBorder(texture, Cyan, DarkPurple, 1);
        FillRect(texture, 3, 3, width - 6, 1, new Color(0.24f, 0.12f, 0.34f, 1f));
        texture.Apply();
        return texture;
    }

    private static Texture2D NewTexture(string name, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
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

    private static void DrawBorder(Texture2D texture, Color bright, Color shadow, int thickness)
    {
        for (int i = 0; i < thickness; i++)
        {
            FillRect(texture, i, i, texture.width - (i * 2), 1, bright);
            FillRect(texture, i, i, 1, texture.height - (i * 2), bright);
            FillRect(texture, i, texture.height - i - 1, texture.width - (i * 2), 1, shadow);
            FillRect(texture, texture.width - i - 1, i, 1, texture.height - (i * 2), shadow);
        }
    }

    private static void DrawCornerBrackets(Texture2D texture, Color color)
    {
        int xMax = texture.width - 1;
        int yMax = texture.height - 1;
        FillRect(texture, 3, 3, 8, 2, color);
        FillRect(texture, 3, 3, 2, 8, color);
        FillRect(texture, xMax - 10, 3, 8, 2, color);
        FillRect(texture, xMax - 3, 3, 2, 8, color);
        FillRect(texture, 3, yMax - 4, 8, 2, color);
        FillRect(texture, 3, yMax - 10, 2, 8, color);
        FillRect(texture, xMax - 10, yMax - 4, 8, 2, color);
        FillRect(texture, xMax - 3, yMax - 10, 2, 8, color);
    }

    private static void DrawRivets(Texture2D texture, Color color)
    {
        SetSafe(texture, 12, 12, color);
        SetSafe(texture, texture.width - 13, 12, color);
        SetSafe(texture, 12, texture.height - 13, color);
        SetSafe(texture, texture.width - 13, texture.height - 13, color);
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

    private static void SetSafe(Texture2D texture, int x, int y, Color color)
    {
        if (x >= 0 && y >= 0 && x < texture.width && y < texture.height)
        {
            texture.SetPixel(x, y, color);
        }
    }
}
