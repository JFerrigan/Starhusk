using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildOptionsMenu : MonoBehaviour
{
    private const float HeaderHeight = 48f;
    private const float TabHeight = 42f;
    private const float CardWidth = 148f;
    private const float CardHeight = 172f;
    private const float CardSpacing = 12f;

    private static readonly string[] TabNames = { "All", "Harvest", "Automata", "Fabrication", "Power" };

    private Rect windowRect;
    private bool isOpen;
    private int selectedTab;
    private Vector2 scrollPosition;
    private readonly List<BuildMenuItem> visibleItems = new List<BuildMenuItem>();
    private GUIStyle titleStyle;
    private GUIStyle resourceStyle;
    private GUIStyle tabStyle;
    private GUIStyle itemNameStyle;
    private GUIStyle costStyle;
    private GUIStyle badgeStyle;
    private float styleScale;

    public static Rect CurrentRect { get; private set; }
    public bool IsOpen => isOpen;

    private struct BuildMenuItem
    {
        public string name;
        public Sprite sprite;
        public Color tint;
        public ResourceStack[] cost;
        public BuildingCategory category;
        public bool affordable;
        public bool unlocked;
        public Action clickAction;
    }

    private void EnsureStyles()
    {
        float scale = GameUiScale.Current;
        if (titleStyle != null && Mathf.Approximately(styleScale, scale))
        {
            return;
        }

        styleScale = scale;
        titleStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(18f, scale),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft
        };

        resourceStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(11f, scale),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.82f, 0.98f, 1f, 1f) },
            alignment = TextAnchor.MiddleLeft
        };

        tabStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(12f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        itemNameStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(12f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        costStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(10f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.9f, 0.88f, 1f, 1f) }
        };

        badgeStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(10f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        if (!isOpen)
        {
            CurrentRect = Rect.zero;
            DrawOpenButton();
            return;
        }

        EnsurePosition();
        float scale = GameUiScale.Current;
        CurrentRect = windowRect;
        CollectVisibleItems();

        PixelUiSprites.Draw(windowRect, PixelUiFrame.Panel);
        PixelUiSprites.Draw(new Rect(windowRect.x + GameUiScale.Size(8f, scale), windowRect.y + GameUiScale.Size(8f, scale), windowRect.width - GameUiScale.Size(16f, scale), GameUiScale.Size(HeaderHeight, scale)), PixelUiFrame.Header);

        GUI.Label(new Rect(windowRect.x + GameUiScale.Size(24f, scale), windowRect.y + GameUiScale.Size(18f, scale), GameUiScale.Size(420f, scale), GameUiScale.Size(28f, scale)), "BUILD MENU [I]", titleStyle);
        DrawResources(new Rect(windowRect.x + GameUiScale.Size(24f, scale), windowRect.y + GameUiScale.Size(66f, scale), windowRect.width - GameUiScale.Size(48f, scale), GameUiScale.Size(34f, scale)), scale);
        DrawTabs(new Rect(windowRect.x + GameUiScale.Size(24f, scale), windowRect.y + GameUiScale.Size(112f, scale), windowRect.width - GameUiScale.Size(48f, scale), GameUiScale.Size(TabHeight, scale)), scale);
        DrawItems(new Rect(windowRect.x + GameUiScale.Size(24f, scale), windowRect.y + GameUiScale.Size(166f, scale), windowRect.width - GameUiScale.Size(48f, scale), windowRect.height - GameUiScale.Size(190f, scale)), scale);
    }

    public bool CloseMenu()
    {
        if (!isOpen)
        {
            return false;
        }

        isOpen = false;
        CurrentRect = Rect.zero;
        return true;
    }

    public void OpenMenu()
    {
        isOpen = true;
    }

    private void DrawOpenButton()
    {
        float scale = GameUiScale.Current;
        float size = GameUiScale.Size(46f, scale);
        Rect buttonRect = new Rect(GameUiScale.Size(16f, scale), Screen.height - size - GameUiScale.Size(16f, scale), size, size);
        bool hover = buttonRect.Contains(Event.current.mousePosition);

        DrawLauncherButtonFrame(buttonRect, hover, scale);
        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
        {
            isOpen = true;
        }

        DrawInventoryIcon(buttonRect, scale);
    }

    private static void DrawInventoryIcon(Rect rect, float scale)
    {
        Color color = PixelUiSprites.ConsoleAccent;
        float lineWidth = GameUiScale.Size(2f, scale);
        Rect box = new Rect(
            rect.center.x - GameUiScale.Size(12f, scale),
            rect.center.y - GameUiScale.Size(8f, scale),
            GameUiScale.Size(24f, scale),
            GameUiScale.Size(18f, scale));

        DrawIconOutline(box, color, lineWidth);
        DrawIconLine(new Vector2(box.xMin, box.y + GameUiScale.Size(6f, scale)), new Vector2(box.xMax, box.y + GameUiScale.Size(6f, scale)), color, lineWidth);
        DrawIconLine(new Vector2(box.center.x, box.y + GameUiScale.Size(6f, scale)), new Vector2(box.center.x, box.yMax), color, lineWidth);
        DrawIconLine(new Vector2(box.xMin + GameUiScale.Size(5f, scale), box.yMin), new Vector2(box.center.x, box.yMin - GameUiScale.Size(5f, scale)), color, lineWidth);
        DrawIconLine(new Vector2(box.xMax - GameUiScale.Size(5f, scale), box.yMin), new Vector2(box.center.x, box.yMin - GameUiScale.Size(5f, scale)), color, lineWidth);
    }

    private static void DrawLauncherButtonFrame(Rect rect, bool hover, float scale)
    {
        Color fill = hover ? PixelUiSprites.ConsoleActive : PixelUiSprites.ConsoleSurface;
        Color edge = hover ? PixelUiSprites.Cyan : PixelUiSprites.ConsoleAccent;
        float border = GameUiScale.Size(2f, scale);
        float inset = GameUiScale.Size(5f, scale);

        DrawSolidRect(rect, new Color(0f, 0f, 0f, 0.48f));
        Rect body = new Rect(rect.x + border, rect.y + border, rect.width - (border * 2f), rect.height - (border * 2f));
        DrawSolidRect(body, fill);

        DrawSolidRect(new Rect(body.x, body.y, body.width, border), edge);
        DrawSolidRect(new Rect(body.x, body.yMax - border, body.width, border), PixelUiSprites.DarkPurple);
        DrawSolidRect(new Rect(body.x, body.y, border, body.height), edge);
        DrawSolidRect(new Rect(body.xMax - border, body.y, border, body.height), PixelUiSprites.DarkPurple);

        DrawSolidRect(new Rect(body.x + inset, body.y + GameUiScale.Size(4f, scale), body.width - (inset * 2f), border), new Color(edge.r, edge.g, edge.b, 0.36f));
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static void DrawIconOutline(Rect rect, Color color, float width)
    {
        DrawIconLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), color, width);
        DrawIconLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), color, width);
        DrawIconLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax), color, width);
        DrawIconLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin), color, width);
    }

    private static void DrawIconLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - (width * 0.5f), delta.magnitude, width), Texture2D.whiteTexture);
        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }

    private void EnsurePosition()
    {
        float scale = GameUiScale.Current;
        float margin = GameUiScale.Size(32f, scale);
        windowRect = new Rect(
            margin,
            margin,
            Mathf.Max(GameUiScale.Size(340f, scale), Screen.width - (margin * 2f)),
            Mathf.Max(GameUiScale.Size(360f, scale), Screen.height - (margin * 2f)));
    }

    private void DrawResources(Rect rect, float scale)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.InnerPanel);
        ResourceGui.DrawAvailableResources(
            new Rect(rect.x + GameUiScale.Size(8f, scale), rect.y + GameUiScale.Size(4f, scale), rect.width - GameUiScale.Size(16f, scale), rect.height - GameUiScale.Size(8f, scale)),
            resourceStyle,
            GameUiScale.Size(16f, scale),
            GameUiScale.Size(64f, scale),
            GameUiScale.Size(8f, scale));
    }

    private void DrawTabs(Rect rect, float scale)
    {
        float tabWidth = rect.width / TabNames.Length;
        for (int i = 0; i < TabNames.Length; i++)
        {
            Rect tabRect = new Rect(rect.x + (tabWidth * i), rect.y, tabWidth - GameUiScale.Size(4f, scale), rect.height);
            bool hover = tabRect.Contains(Event.current.mousePosition);
            PixelUiFrame frame = i == selectedTab ? PixelUiFrame.TabActive : hover ? PixelUiFrame.TabHover : PixelUiFrame.Tab;

            PixelUiSprites.Draw(tabRect, frame);
            if (GUI.Button(tabRect, GUIContent.none, GUIStyle.none))
            {
                selectedTab = i;
                scrollPosition = Vector2.zero;
            }

            GUI.Label(tabRect, TabNames[i].ToUpperInvariant(), tabStyle);
        }
    }

    private void DrawItems(Rect rect, float scale)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.InnerPanel);

        Rect padded = new Rect(rect.x + GameUiScale.Size(10f, scale), rect.y + GameUiScale.Size(10f, scale), rect.width - GameUiScale.Size(20f, scale), rect.height - GameUiScale.Size(20f, scale));
        float viewWidth = Mathf.Max(1f, padded.width - GameUiScale.Size(20f, scale));
        float cardWidth = GameUiScale.Size(CardWidth, scale);
        float cardHeight = GameUiScale.Size(CardHeight, scale);
        float cardSpacing = GameUiScale.Size(CardSpacing, scale);
        float contentHeight = PixelUiSprites.GridContentHeight(visibleItems.Count, viewWidth, cardWidth, cardHeight, cardSpacing);
        Rect viewRect = new Rect(0f, 0f, viewWidth, contentHeight);

        scrollPosition = GUI.BeginScrollView(padded, scrollPosition, viewRect);

        int columns = PixelUiSprites.GridColumnCount(viewWidth, cardWidth, cardSpacing);
        float gridWidth = (columns * cardWidth) + ((columns - 1) * cardSpacing);
        float startX = Mathf.Max(0f, (viewWidth - gridWidth) * 0.5f);

        for (int i = 0; i < visibleItems.Count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            Rect cardRect = new Rect(
                startX + (column * (cardWidth + cardSpacing)),
                row * (cardHeight + cardSpacing),
                cardWidth,
                cardHeight);

            DrawCard(cardRect, visibleItems[i], scale);
        }

        GUI.EndScrollView();
    }

    private void DrawCard(Rect rect, BuildMenuItem item, float scale)
    {
        bool enabled = !item.unlocked && item.affordable;
        bool hover = rect.Contains(Event.current.mousePosition);
        PixelUiFrame frame = item.unlocked
            ? PixelUiFrame.CardUnlocked
            : enabled
                ? hover ? PixelUiFrame.CardHover : PixelUiFrame.Card
                : PixelUiFrame.CardDisabled;

        PixelUiSprites.Draw(rect, frame);

        Rect spriteRect = new Rect(rect.x + GameUiScale.Size(22f, scale), rect.y + GameUiScale.Size(16f, scale), rect.width - GameUiScale.Size(44f, scale), GameUiScale.Size(82f, scale));
        Rect nameRect = new Rect(rect.x + GameUiScale.Size(10f, scale), rect.y + GameUiScale.Size(104f, scale), rect.width - GameUiScale.Size(20f, scale), GameUiScale.Size(34f, scale));
        Rect costRect = new Rect(rect.x + GameUiScale.Size(10f, scale), rect.y + GameUiScale.Size(140f, scale), rect.width - GameUiScale.Size(20f, scale), GameUiScale.Size(22f, scale));

        DrawPreview(spriteRect, item.sprite, enabled || item.unlocked ? item.tint : Dim(item.tint));

        GUI.Label(nameRect, item.name.ToUpperInvariant(), itemNameStyle);

        if (item.unlocked)
        {
            Rect badgeRect = new Rect(rect.x + GameUiScale.Size(25f, scale), rect.y + GameUiScale.Size(134f, scale), rect.width - GameUiScale.Size(50f, scale), GameUiScale.Size(24f, scale));
            PixelUiSprites.Draw(badgeRect, PixelUiFrame.Badge);
            GUI.Label(badgeRect, "UNLOCKED", badgeStyle);
        }
        else
        {
            ResourceGui.DrawCostRow(costRect, item.cost, costStyle, true, GameUiScale.Size(16f, scale), GameUiScale.Size(44f, scale), GameUiScale.Size(6f, scale));
            if (!item.affordable)
            {
                Rect badgeRect = new Rect(rect.x + rect.width - GameUiScale.Size(64f, scale), rect.y + GameUiScale.Size(8f, scale), GameUiScale.Size(54f, scale), GameUiScale.Size(22f, scale));
                PixelUiSprites.Draw(badgeRect, PixelUiFrame.WarningBadge);
                GUI.Label(badgeRect, "NO RES", badgeStyle);
            }
        }

        GUI.enabled = enabled;
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            item.clickAction?.Invoke();
            isOpen = false;
        }

        GUI.enabled = true;
    }

    private static void DrawPreview(Rect rect, Sprite sprite, Color tint)
    {
        if (sprite == null || sprite.texture == null)
        {
            return;
        }

        Rect textureRect = sprite.textureRect;
        Rect uv = new Rect(
            textureRect.x / sprite.texture.width,
            textureRect.y / sprite.texture.height,
            textureRect.width / sprite.texture.width,
            textureRect.height / sprite.texture.height);

        Color previous = GUI.color;
        GUI.color = tint;
        GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, true);
        GUI.color = previous;
    }

    private void CollectVisibleItems()
    {
        visibleItems.Clear();

        IReadOnlyList<BuildingType> planetBuildings = BuildingCatalog.AllPlanetBuildings;
        for (int i = 0; i < planetBuildings.Count; i++)
        {
            BuildingType buildingType = planetBuildings[i];
            BuildingDefinition definition = BuildingCatalog.GetDefinition(buildingType);
            if (!IsVisibleInCurrentTab(definition.category))
            {
                continue;
            }

            bool unlocked = definition.upgradeId.HasValue && IsUpgradeUnlocked(definition.upgradeId.Value);
            visibleItems.Add(new BuildMenuItem
            {
                name = definition.displayName,
                sprite = definition.placeholderSprite,
                tint = definition.tint,
                cost = definition.buildCost,
                category = definition.category,
                affordable = BuildResourcePool.CanAfford(definition.buildCost),
                unlocked = unlocked,
                clickAction = () => StartPlanetBuilding(buildingType)
            });
        }

        AutomatonBuildOption[] automata = AutomatonPlacementController.AllBuildOptions;
        for (int i = 0; i < automata.Length; i++)
        {
            AutomatonBuildOption option = automata[i];
            BuildingCategory category = AutomatonPlacementController.CategoryFor(option);
            if (!IsVisibleInCurrentTab(category))
            {
                continue;
            }

            ResourceStack[] cost = AutomatonPlacementController.BuildCostFor(option);
            visibleItems.Add(new BuildMenuItem
            {
                name = AutomatonPlacementController.DisplayNameFor(option),
                sprite = AutomatonPlacementController.SpriteFor(option),
                tint = AutomatonPlacementController.ColorFor(option),
                cost = cost,
                category = category,
                affordable = BuildResourcePool.CanAfford(cost),
                unlocked = false,
                clickAction = () => StartAutomaton(option)
            });
        }
    }

    private bool IsVisibleInCurrentTab(BuildingCategory category)
    {
        if (selectedTab == 0)
        {
            return true;
        }

        return TabNames[selectedTab] == category.ToString();
    }

    private static bool IsUpgradeUnlocked(UpgradeId upgradeId)
    {
        PlayerUpgradeState state = PlayerUpgradeState.Current;
        return state != null && state.IsUnlocked(upgradeId);
    }

    private static void StartPlanetBuilding(BuildingType buildingType)
    {
        BuildingPlacementController controller = BuildingPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginPlacement(buildingType);
        }
    }

    private static void StartAutomaton(AutomatonBuildOption option)
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginPlacement(option);
        }
    }

    public static bool ContainsGuiPoint(Vector2 guiPoint)
    {
        return CurrentRect.Contains(guiPoint);
    }

    private static Color Dim(Color color)
    {
        return new Color(color.r * 0.42f, color.g * 0.42f, color.b * 0.42f, color.a * 0.82f);
    }
}
