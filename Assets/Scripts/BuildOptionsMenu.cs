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

    public static Rect CurrentRect { get; private set; }

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
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft
        };

        resourceStyle = new GUIStyle
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.82f, 0.98f, 1f, 1f) },
            alignment = TextAnchor.MiddleLeft
        };

        tabStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        itemNameStyle = new GUIStyle
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        costStyle = new GUIStyle
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.9f, 0.88f, 1f, 1f) }
        };

        badgeStyle = new GUIStyle
        {
            fontSize = 10,
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
        CurrentRect = windowRect;
        CollectVisibleItems();

        PixelUiSprites.Draw(windowRect, PixelUiFrame.Panel);
        PixelUiSprites.Draw(new Rect(windowRect.x + 8f, windowRect.y + 8f, windowRect.width - 16f, HeaderHeight), PixelUiFrame.Header);

        GUI.Label(new Rect(windowRect.x + 24f, windowRect.y + 18f, 420f, 28f), "BUILD MENU [I]", titleStyle);
        DrawResources(new Rect(windowRect.x + 24f, windowRect.y + 66f, windowRect.width - 48f, 30f));
        DrawTabs(new Rect(windowRect.x + 24f, windowRect.y + 110f, windowRect.width - 48f, TabHeight));
        DrawItems(new Rect(windowRect.x + 24f, windowRect.y + 164f, windowRect.width - 48f, windowRect.height - 188f));
    }

    private void DrawOpenButton()
    {
        Rect buttonRect = new Rect(16f, Screen.height - 58f, 184f, 42f);
        bool hover = buttonRect.Contains(Event.current.mousePosition);

        PixelUiSprites.Draw(buttonRect, hover ? PixelUiFrame.ButtonHover : PixelUiFrame.Button);
        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
        {
            isOpen = true;
        }

        GUI.Label(buttonRect, "[I] BUILD", tabStyle);
    }

    private void EnsurePosition()
    {
        float margin = 48f;
        windowRect = new Rect(
            margin,
            margin,
            Mathf.Max(340f, Screen.width - (margin * 2f)),
            Mathf.Max(360f, Screen.height - (margin * 2f)));
    }

    private void DrawResources(Rect rect)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.InnerPanel);
        ResourceGui.DrawAvailableResources(new Rect(rect.x + 8f, rect.y + 4f, rect.width - 16f, rect.height - 8f), resourceStyle);
    }

    private void DrawTabs(Rect rect)
    {
        float tabWidth = rect.width / TabNames.Length;
        for (int i = 0; i < TabNames.Length; i++)
        {
            Rect tabRect = new Rect(rect.x + (tabWidth * i), rect.y, tabWidth - 4f, rect.height);
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

    private void DrawItems(Rect rect)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.InnerPanel);

        Rect padded = new Rect(rect.x + 10f, rect.y + 10f, rect.width - 20f, rect.height - 20f);
        float viewWidth = Mathf.Max(1f, padded.width - 20f);
        float contentHeight = PixelUiSprites.GridContentHeight(visibleItems.Count, viewWidth, CardWidth, CardHeight, CardSpacing);
        Rect viewRect = new Rect(0f, 0f, viewWidth, contentHeight);

        scrollPosition = GUI.BeginScrollView(padded, scrollPosition, viewRect);

        int columns = PixelUiSprites.GridColumnCount(viewWidth, CardWidth, CardSpacing);
        float gridWidth = (columns * CardWidth) + ((columns - 1) * CardSpacing);
        float startX = Mathf.Max(0f, (viewWidth - gridWidth) * 0.5f);

        for (int i = 0; i < visibleItems.Count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            Rect cardRect = new Rect(
                startX + (column * (CardWidth + CardSpacing)),
                row * (CardHeight + CardSpacing),
                CardWidth,
                CardHeight);

            DrawCard(cardRect, visibleItems[i]);
        }

        GUI.EndScrollView();
    }

    private void DrawCard(Rect rect, BuildMenuItem item)
    {
        bool enabled = !item.unlocked && item.affordable;
        bool hover = rect.Contains(Event.current.mousePosition);
        PixelUiFrame frame = item.unlocked
            ? PixelUiFrame.CardUnlocked
            : enabled
                ? hover ? PixelUiFrame.CardHover : PixelUiFrame.Card
                : PixelUiFrame.CardDisabled;

        PixelUiSprites.Draw(rect, frame);

        Rect spriteRect = new Rect(rect.x + 22f, rect.y + 16f, rect.width - 44f, 82f);
        Rect nameRect = new Rect(rect.x + 10f, rect.y + 104f, rect.width - 20f, 34f);
        Rect costRect = new Rect(rect.x + 10f, rect.y + 140f, rect.width - 20f, 22f);

        DrawPreview(spriteRect, item.sprite, enabled || item.unlocked ? item.tint : Dim(item.tint));

        GUI.Label(nameRect, item.name.ToUpperInvariant(), itemNameStyle);

        if (item.unlocked)
        {
            Rect badgeRect = new Rect(rect.x + 25f, rect.y + 134f, rect.width - 50f, 24f);
            PixelUiSprites.Draw(badgeRect, PixelUiFrame.Badge);
            GUI.Label(badgeRect, "UNLOCKED", badgeStyle);
        }
        else
        {
            ResourceGui.DrawCostRow(costRect, item.cost, costStyle);
            if (!item.affordable)
            {
                Rect badgeRect = new Rect(rect.x + rect.width - 64f, rect.y + 8f, 54f, 22f);
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
