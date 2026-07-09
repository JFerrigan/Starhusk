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
    Chip,
    ConsolePanel,
    ConsoleInnerPanel,
    ConsoleHeader,
    ConsoleRow,
    ConsoleRowSelected,
    ConsoleButton,
    ConsoleButtonHover,
    ConsoleButtonActive,
    ConsoleIconButton,
    ConsoleIconButtonHover,
    ConsoleChip
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
    public static readonly Color ConsoleDeep = Deep;
    public static readonly Color ConsoleInset = Inset;
    public static readonly Color ConsoleSurface = Purple;
    public static readonly Color ConsoleAccent = Gold;
    public static readonly Color ConsoleActive = new Color(0.24f, 0.1f, 0.38f, 1f);

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
    private static Texture2D consolePanel;
    private static Texture2D consoleInnerPanel;
    private static Texture2D consoleHeader;
    private static Texture2D consoleRow;
    private static Texture2D consoleRowSelected;
    private static Texture2D consoleButton;
    private static Texture2D consoleButtonHover;
    private static Texture2D consoleButtonActive;
    private static Texture2D consoleIconButton;
    private static Texture2D consoleIconButtonHover;
    private static Texture2D consoleChip;

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
            case PixelUiFrame.ConsolePanel:
                return consolePanel == null ? consolePanel = CreateConsolePanel("Runtime Pixel UI Console Panel", 64, ConsoleDeep, ConsoleSurface, ConsoleAccent, 12) : consolePanel;
            case PixelUiFrame.ConsoleInnerPanel:
                return consoleInnerPanel == null ? consoleInnerPanel = CreateConsolePanel("Runtime Pixel UI Console Inner Panel", 32, ConsoleInset, DarkPurple, Muted, 8) : consoleInnerPanel;
            case PixelUiFrame.ConsoleHeader:
                return consoleHeader == null ? consoleHeader = CreateConsolePanel("Runtime Pixel UI Console Header", 48, ConsoleSurface, DarkPurple, ConsoleAccent, 10) : consoleHeader;
            case PixelUiFrame.ConsoleRow:
                return consoleRow == null ? consoleRow = CreateConsoleRow("Runtime Pixel UI Console Row", ConsoleInset, DarkPurple, Muted) : consoleRow;
            case PixelUiFrame.ConsoleRowSelected:
                return consoleRowSelected == null ? consoleRowSelected = CreateConsoleRow("Runtime Pixel UI Console Row Selected", ConsoleActive, DarkPurple, ConsoleAccent) : consoleRowSelected;
            case PixelUiFrame.ConsoleButton:
                return consoleButton == null ? consoleButton = CreateConsoleButton("Runtime Pixel UI Console Button", ConsoleSurface, Muted, ConsoleAccent) : consoleButton;
            case PixelUiFrame.ConsoleButtonHover:
                return consoleButtonHover == null ? consoleButtonHover = CreateConsoleButton("Runtime Pixel UI Console Button Hover", new Color(0.22f, 0.1f, 0.32f, 1f), ConsoleAccent, Cyan) : consoleButtonHover;
            case PixelUiFrame.ConsoleButtonActive:
                return consoleButtonActive == null ? consoleButtonActive = CreateConsoleButton("Runtime Pixel UI Console Button Active", ConsoleActive, ConsoleAccent, Gold) : consoleButtonActive;
            case PixelUiFrame.ConsoleIconButton:
                return consoleIconButton == null ? consoleIconButton = CreateConsoleIconButton("Runtime Pixel UI Console Icon Button", ConsoleSurface, Muted, ConsoleAccent) : consoleIconButton;
            case PixelUiFrame.ConsoleIconButtonHover:
                return consoleIconButtonHover == null ? consoleIconButtonHover = CreateConsoleIconButton("Runtime Pixel UI Console Icon Button Hover", new Color(0.22f, 0.1f, 0.32f, 1f), ConsoleAccent, Cyan) : consoleIconButtonHover;
            case PixelUiFrame.ConsoleChip:
                return consoleChip == null ? consoleChip = CreateConsoleButton("Runtime Pixel UI Console Chip", ConsoleDeep, Muted, ConsoleAccent) : consoleChip;
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
        RectInt safeRect = ContentSafeRect(texture, stars ? 14 : 10);
        Clear(texture, fill);
        DrawBorder(texture, line, shade, 2);
        DrawCornerBrackets(texture, Cyan);
        DrawRivets(texture, Gold, safeRect);

        for (int y = 5; y < size - 5; y += 7)
        {
            for (int x = 5; x < size - 5; x += 7)
            {
                if (!IsInContentSafeRect(x, y, safeRect) && ((x * 17) + (y * 31)) % 5 == 0)
                {
                    texture.SetPixel(x, y, shade);
                }
            }
        }

        if (stars)
        {
            DrawDecorativeNoiseOutside(texture, safeRect, size);
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateCard(string name, Color fill, Color line, Color accent, bool lit, bool disabled)
    {
        const int width = 48;
        const int height = 56;
        Texture2D texture = NewTexture(name, width, height);
        RectInt safeRect = ContentSafeRect(texture, 6);
        Color contentFill = disabled ? new Color(0.055f, 0.035f, 0.075f, 1f) : Inset;
        Clear(texture, contentFill);
        FillFrame(texture, safeRect, fill);
        DrawBorder(texture, line, disabled ? Muted : DarkPurple, 2);
        DrawCornerBrackets(texture, accent);

        FillRect(texture, safeRect.x, safeRect.y, safeRect.width, safeRect.height, contentFill);
        FillRect(texture, 7, 4, width - 14, 1, disabled ? Muted : accent);
        FillRect(texture, 7, height - 5, width - 14, 1, disabled ? Muted : accent);

        if (lit)
        {
            FillRect(texture, 3, 8, 1, 9, accent);
            FillRect(texture, width - 4, height - 17, 1, 9, accent);
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
        FillRect(texture, 3, 2, width - 6, 1, accent);
        FillRect(texture, 4, height - 3, width - 8, 1, new Color(0f, 0f, 0f, 0.35f));
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
        FillRect(texture, 3, 2, width - 6, 1, new Color(0.24f, 0.12f, 0.34f, 1f));
        FillRect(texture, 4, height - 3, width - 8, 1, new Color(0f, 0f, 0f, 0.35f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateConsolePanel(string name, int size, Color fill, Color shade, Color line, int padding)
    {
        Texture2D texture = NewTexture(name, size, size);
        RectInt safeRect = ContentSafeRect(texture, padding);
        Clear(texture, fill);
        FillFrame(texture, safeRect, shade);
        DrawBorder(texture, line, new Color(0f, 0f, 0f, 0.42f), 1);
        DrawSteppedCorners(texture, fill, Mathf.Max(4, padding / 2));
        DrawQuietCornerTicks(texture, line);

        for (int y = 4; y < safeRect.yMin - 2; y += 5)
        {
            FillRect(texture, 5, y, size - 10, 1, new Color(0f, 0f, 0f, 0.12f));
        }

        for (int y = safeRect.yMax + 2; y < size - 5; y += 5)
        {
            FillRect(texture, 5, y, size - 10, 1, new Color(0f, 0f, 0f, 0.12f));
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateConsoleRow(string name, Color fill, Color shade, Color line)
    {
        const int width = 48;
        const int height = 28;
        Texture2D texture = NewTexture(name, width, height);
        RectInt safeRect = ContentSafeRect(texture, 4);
        Clear(texture, fill);
        FillFrame(texture, safeRect, shade);
        DrawBorder(texture, line, new Color(0f, 0f, 0f, 0.38f), 1);
        DrawSteppedCorners(texture, ConsoleDeep, 4);
        FillRect(texture, safeRect.xMin, safeRect.yMin, safeRect.width, safeRect.height, fill);
        DrawSideNotches(texture, line, safeRect);
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateConsoleButton(string name, Color fill, Color line, Color accent)
    {
        const int width = 48;
        const int height = 16;
        Texture2D texture = NewTexture(name, width, height);
        Clear(texture, fill);
        DrawBorder(texture, line, new Color(0f, 0f, 0f, 0.42f), 1);
        DrawSteppedCorners(texture, ConsoleDeep, 3);
        FillRect(texture, 4, 2, width - 8, 1, new Color(accent.r, accent.g, accent.b, 0.36f));
        FillRect(texture, 4, height - 3, width - 8, 1, new Color(0f, 0f, 0f, 0.28f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateConsoleIconButton(string name, Color fill, Color line, Color accent)
    {
        const int size = 32;
        Texture2D texture = NewTexture(name, size, size);
        Clear(texture, fill);
        DrawBorder(texture, line, new Color(0f, 0f, 0f, 0.42f), 1);
        DrawSteppedCorners(texture, ConsoleDeep, 4);
        FillRect(texture, 5, 3, size - 10, 1, new Color(accent.r, accent.g, accent.b, 0.36f));
        FillRect(texture, 5, size - 4, size - 10, 1, new Color(0f, 0f, 0f, 0.28f));
        FillRect(texture, 3, 5, 1, size - 10, new Color(accent.r, accent.g, accent.b, 0.18f));
        FillRect(texture, size - 4, 5, 1, size - 10, new Color(0f, 0f, 0f, 0.22f));
        texture.Apply();
        return texture;
    }

    private static RectInt ContentSafeRect(Texture2D texture, int padding)
    {
        int clampedPadding = Mathf.Clamp(padding, 0, Mathf.Min(texture.width, texture.height) / 2);
        return new RectInt(
            clampedPadding,
            clampedPadding,
            Mathf.Max(0, texture.width - (clampedPadding * 2)),
            Mathf.Max(0, texture.height - (clampedPadding * 2)));
    }

    private static bool IsInContentSafeRect(int x, int y, RectInt safeRect)
    {
        return x >= safeRect.xMin && x < safeRect.xMax && y >= safeRect.yMin && y < safeRect.yMax;
    }

    private static void DrawDecorativeNoiseOutside(Texture2D texture, RectInt safeRect, int size)
    {
        for (int i = 0; i < 18; i++)
        {
            int x = 5 + ((i * 11) % (size - 10));
            int y = 5 + ((i * 19) % (size - 10));
            if (!IsInContentSafeRect(x, y, safeRect))
            {
                texture.SetPixel(x, y, i % 3 == 0 ? Cyan : new Color(0.55f, 0.46f, 0.68f, 1f));
            }
        }
    }

    private static void FillFrame(Texture2D texture, RectInt safeRect, Color color)
    {
        FillRect(texture, 0, 0, texture.width, safeRect.yMin, color);
        FillRect(texture, 0, safeRect.yMax, texture.width, texture.height - safeRect.yMax, color);
        FillRect(texture, 0, safeRect.yMin, safeRect.xMin, safeRect.height, color);
        FillRect(texture, safeRect.xMax, safeRect.yMin, texture.width - safeRect.xMax, safeRect.height, color);
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

    private static void DrawQuietCornerTicks(Texture2D texture, Color color)
    {
        int xMax = texture.width - 1;
        int yMax = texture.height - 1;
        Color tick = new Color(color.r, color.g, color.b, 0.65f);
        FillRect(texture, 3, 3, 5, 1, tick);
        FillRect(texture, 3, 3, 1, 5, tick);
        FillRect(texture, xMax - 7, 3, 5, 1, tick);
        FillRect(texture, xMax - 3, 3, 1, 5, tick);
        FillRect(texture, 3, yMax - 3, 5, 1, tick);
        FillRect(texture, 3, yMax - 7, 1, 5, tick);
        FillRect(texture, xMax - 7, yMax - 3, 5, 1, tick);
        FillRect(texture, xMax - 3, yMax - 7, 1, 5, tick);
    }

    private static void DrawSteppedCorners(Texture2D texture, Color color, int size)
    {
        int step = Mathf.Max(2, size);
        FillRect(texture, 0, 0, step, 1, color);
        FillRect(texture, 0, 1, step - 1, 1, color);
        FillRect(texture, 0, 2, Mathf.Max(1, step - 2), 1, color);

        FillRect(texture, texture.width - step, 0, step, 1, color);
        FillRect(texture, texture.width - step + 1, 1, step - 1, 1, color);
        FillRect(texture, texture.width - Mathf.Max(1, step - 2), 2, Mathf.Max(1, step - 2), 1, color);

        FillRect(texture, 0, texture.height - 1, step, 1, color);
        FillRect(texture, 0, texture.height - 2, step - 1, 1, color);
        FillRect(texture, 0, texture.height - 3, Mathf.Max(1, step - 2), 1, color);

        FillRect(texture, texture.width - step, texture.height - 1, step, 1, color);
        FillRect(texture, texture.width - step + 1, texture.height - 2, step - 1, 1, color);
        FillRect(texture, texture.width - Mathf.Max(1, step - 2), texture.height - 3, Mathf.Max(1, step - 2), 1, color);
    }

    private static void DrawSideNotches(Texture2D texture, Color color, RectInt safeRect)
    {
        int y = safeRect.yMin + Mathf.Max(1, safeRect.height / 2);
        FillRect(texture, 1, y - 1, 3, 1, color);
        FillRect(texture, 1, y, 3, 1, color);
        FillRect(texture, 1, y + 1, 3, 1, color);
        FillRect(texture, texture.width - 4, y - 1, 3, 1, color);
        FillRect(texture, texture.width - 4, y, 3, 1, color);
        FillRect(texture, texture.width - 4, y + 1, 3, 1, color);
    }

    private static void DrawRivets(Texture2D texture, Color color, RectInt safeRect)
    {
        int inset = Mathf.Max(4, Mathf.Min(11, safeRect.xMin - 3));
        SetSafe(texture, inset, inset, color);
        SetSafe(texture, texture.width - inset - 1, inset, color);
        SetSafe(texture, inset, texture.height - inset - 1, color);
        SetSafe(texture, texture.width - inset - 1, texture.height - inset - 1, color);
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
