using System;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceGui
{
    private static readonly ResourceType[] Types =
    {
        ResourceType.Ore,
        ResourceType.Ice,
        ResourceType.Silicate,
        ResourceType.Copper,
        ResourceType.Biomass
    };

    private static Texture2D oreIcon;
    private static Texture2D iceIcon;
    private static Texture2D silicateIcon;
    private static Texture2D copperIcon;
    private static Texture2D biomassIcon;

    public static IReadOnlyList<ResourceType> AllTypes => Types;

    public static Texture2D IconFor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Ice:
                return iceIcon == null ? iceIcon = CreateIce() : iceIcon;
            case ResourceType.Silicate:
                return silicateIcon == null ? silicateIcon = CreateSilicate() : silicateIcon;
            case ResourceType.Copper:
                return copperIcon == null ? copperIcon = CreateCopper() : copperIcon;
            case ResourceType.Biomass:
                return biomassIcon == null ? biomassIcon = CreateBiomass() : biomassIcon;
            case ResourceType.Ore:
            default:
                return oreIcon == null ? oreIcon = CreateOre() : oreIcon;
        }
    }

    public static void DrawIconAmount(Rect rect, ResourceType type, int amount, GUIStyle style)
    {
        DrawIconAmount(rect, type, amount, style, Color.white, null);
    }

    public static void DrawIconAmount(Rect rect, ResourceType type, int amount, GUIStyle style, Color color, string suffix)
    {
        DrawIconAmount(rect, type, amount, style, color, suffix, 16f);
    }

    public static void DrawIconAmount(Rect rect, ResourceType type, int amount, GUIStyle style, Color color, string suffix, float iconSize)
    {
        iconSize = Mathf.Max(1f, iconSize);
        float gap = Mathf.Max(3f, iconSize * 0.25f);
        Rect iconRect = new Rect(rect.x, rect.y + Mathf.Max(0f, (rect.height - iconSize) * 0.5f), iconSize, iconSize);
        Rect labelRect = new Rect(rect.x + iconSize + gap, rect.y, rect.width - iconSize - gap, rect.height);

        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(iconRect, IconFor(type), ScaleMode.ScaleToFit, true);
        GUI.color = previousColor;

        Color previousText = style.normal.textColor;
        style.normal.textColor = color;
        GUI.Label(labelRect, amount + (string.IsNullOrEmpty(suffix) ? string.Empty : suffix), style);
        style.normal.textColor = previousText;
    }

    public static void DrawIconLabel(Rect rect, ResourceType type, GUIStyle style, Color color)
    {
        DrawIconLabel(rect, type, style, color, 16f);
    }

    public static void DrawIconLabel(Rect rect, ResourceType type, GUIStyle style, Color color, float iconSize)
    {
        iconSize = Mathf.Max(1f, iconSize);
        float gap = Mathf.Max(3f, iconSize * 0.25f);
        Rect iconRect = new Rect(rect.x, rect.y + Mathf.Max(0f, (rect.height - iconSize) * 0.5f), iconSize, iconSize);
        Rect labelRect = new Rect(rect.x + iconSize + gap, rect.y, rect.width - iconSize - gap, rect.height);

        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(iconRect, IconFor(type), ScaleMode.ScaleToFit, true);
        GUI.color = previousColor;

        Color previousText = style.normal.textColor;
        style.normal.textColor = color;
        GUI.Label(labelRect, type.ToString().ToUpperInvariant(), style);
        style.normal.textColor = previousText;
    }

    public static float DrawCostRow(Rect rect, ResourceStack[] cost, GUIStyle style)
    {
        return DrawCostRow(rect, cost, style, true);
    }

    public static float DrawCostRow(Rect rect, ResourceStack[] cost, GUIStyle style, bool showMissing)
    {
        return DrawCostRow(rect, cost, style, showMissing, 16f, 44f, 6f);
    }

    public static float DrawCostRow(Rect rect, ResourceStack[] cost, GUIStyle style, bool showMissing, float iconSize, float baseChipWidth, float chipSpacing)
    {
        if (cost == null || cost.Length <= 0)
        {
            GUI.Label(rect, "FREE", style);
            return Mathf.Max(baseChipWidth, rect.height);
        }

        float x = rect.x;
        for (int i = 0; i < cost.Length; i++)
        {
            ResourceStack stack = cost[i];
            int available = BuildResourcePool.GetAvailable(stack.type);
            bool missing = showMissing && available < stack.amount;
            float digitWidth = Mathf.Max(5f, style.fontSize * 0.52f);
            float width = Mathf.Clamp(baseChipWidth + (stack.amount.ToString().Length * digitWidth), baseChipWidth + iconSize * 0.5f, baseChipWidth + iconSize * 2.4f);
            Rect chipRect = new Rect(x, rect.y, width, rect.height);

            PixelUiSprites.Draw(chipRect, PixelUiFrame.Chip);
            DrawIconAmount(
                new Rect(chipRect.x + Mathf.Max(4f, iconSize * 0.25f), chipRect.y + 2f, chipRect.width - Mathf.Max(8f, iconSize * 0.5f), chipRect.height - 4f),
                stack.type,
                stack.amount,
                style,
                missing ? PixelUiSprites.Warning : Color.white,
                null,
                iconSize);

            x += width + chipSpacing;
        }

        return x - rect.x;
    }

    public static float DrawAvailableResources(Rect rect, GUIStyle style)
    {
        return DrawAvailableResources(rect, style, 16f, 64f, 8f);
    }

    public static float DrawAvailableResources(Rect rect, GUIStyle style, float iconSize, float baseChipWidth, float chipSpacing)
    {
        float x = rect.x;
        for (int i = 0; i < Types.Length; i++)
        {
            ResourceType type = Types[i];
            int amount = BuildResourcePool.GetAvailable(type);
            float digitWidth = Mathf.Max(5f, style.fontSize * 0.52f);
            float width = Mathf.Clamp(baseChipWidth + (amount.ToString().Length * digitWidth), baseChipWidth + iconSize * 0.6f, baseChipWidth + iconSize * 3.0f);
            Rect chipRect = new Rect(x, rect.y, width, rect.height);

            PixelUiSprites.Draw(chipRect, PixelUiFrame.Chip);
            DrawIconAmount(
                new Rect(chipRect.x + Mathf.Max(5f, iconSize * 0.3f), chipRect.y + 3f, chipRect.width - Mathf.Max(10f, iconSize * 0.6f), chipRect.height - 6f),
                type,
                amount,
                style,
                Color.white,
                null,
                iconSize);

            x += width + chipSpacing;
        }

        return x - rect.x;
    }

    public static void DrawResourceStackList(Rect rect, IReadOnlyList<ResourceStack> stacks, GUIStyle style, float rowHeight)
    {
        DrawResourceStackList(rect, stacks, style, rowHeight, 16f);
    }

    public static void DrawResourceStackList(Rect rect, IReadOnlyList<ResourceStack> stacks, GUIStyle style, float rowHeight, float iconSize)
    {
        if (stacks == null || stacks.Count <= 0)
        {
            GUI.Label(rect, "EMPTY", style);
            return;
        }

        for (int i = 0; i < stacks.Count; i++)
        {
            ResourceStack stack = stacks[i];
            DrawIconAmount(
                new Rect(rect.x, rect.y + (i * rowHeight), rect.width, rowHeight),
                stack.type,
                stack.amount,
                style,
                Color.white,
                null,
                iconSize);
        }
    }

    private static Texture2D CreateOre()
    {
        Texture2D texture = NewIcon("Runtime Resource Icon Ore");
        Color dark = new Color(0.2f, 0.21f, 0.25f, 1f);
        Color mid = new Color(0.52f, 0.55f, 0.62f, 1f);
        Color light = new Color(0.82f, 0.86f, 0.92f, 1f);
        FillRect(texture, 4, 5, 8, 7, dark);
        FillRect(texture, 5, 4, 6, 9, mid);
        FillRect(texture, 7, 3, 4, 3, light);
        FillRect(texture, 3, 8, 3, 3, new Color(0.32f, 0.34f, 0.4f, 1f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateIce()
    {
        Texture2D texture = NewIcon("Runtime Resource Icon Ice");
        Color dark = new Color(0.08f, 0.34f, 0.5f, 1f);
        Color mid = new Color(0.18f, 0.78f, 0.95f, 1f);
        Color light = new Color(0.72f, 1f, 1f, 1f);
        FillRect(texture, 7, 2, 3, 12, dark);
        FillRect(texture, 5, 5, 7, 7, mid);
        FillRect(texture, 7, 3, 2, 8, light);
        FillRect(texture, 3, 8, 3, 4, new Color(0.12f, 0.52f, 0.72f, 1f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateSilicate()
    {
        Texture2D texture = NewIcon("Runtime Resource Icon Silicate");
        Color dark = new Color(0.18f, 0.12f, 0.32f, 1f);
        Color mid = new Color(0.62f, 0.5f, 0.9f, 1f);
        Color light = new Color(0.88f, 0.82f, 1f, 1f);
        FillRect(texture, 4, 7, 4, 6, dark);
        FillRect(texture, 8, 4, 4, 9, mid);
        FillRect(texture, 5, 5, 3, 4, light);
        FillRect(texture, 11, 8, 2, 4, new Color(0.42f, 0.3f, 0.7f, 1f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateCopper()
    {
        Texture2D texture = NewIcon("Runtime Resource Icon Copper");
        Color dark = new Color(0.36f, 0.13f, 0.04f, 1f);
        Color mid = new Color(0.86f, 0.34f, 0.09f, 1f);
        Color light = new Color(1f, 0.68f, 0.24f, 1f);
        FillRect(texture, 4, 5, 8, 7, dark);
        FillRect(texture, 5, 4, 7, 6, mid);
        FillRect(texture, 6, 5, 5, 2, light);
        FillRect(texture, 3, 10, 9, 2, new Color(0.58f, 0.2f, 0.06f, 1f));
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateBiomass()
    {
        Texture2D texture = NewIcon("Runtime Resource Icon Biomass");
        Color dark = new Color(0.07f, 0.27f, 0.13f, 1f);
        Color mid = new Color(0.28f, 0.78f, 0.28f, 1f);
        Color light = new Color(0.72f, 1f, 0.42f, 1f);
        FillRect(texture, 5, 5, 6, 7, dark);
        FillRect(texture, 4, 6, 8, 5, mid);
        FillRect(texture, 7, 3, 3, 4, light);
        FillRect(texture, 6, 8, 2, 2, new Color(0.1f, 0.46f, 0.16f, 1f));
        texture.Apply();
        return texture;
    }

    private static Texture2D NewIcon(string name)
    {
        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texture.name = name;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }

        return texture;
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int iy = Math.Max(0, y); iy < Math.Min(texture.height, y + height); iy++)
        {
            for (int ix = Math.Max(0, x); ix < Math.Min(texture.width, x + width); ix++)
            {
                texture.SetPixel(ix, iy, color);
            }
        }
    }
}
