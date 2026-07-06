using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildOptionsMenu : MonoBehaviour
{
    private const float WindowWidth = 620f;
    private const float WindowHeight = 520f;
    private const float HeaderHeight = 32f;
    private const float TabHeight = 28f;

    private static readonly string[] TabNames = { "All", "Harvest", "Automata", "Fabrication", "Power" };

    private Rect windowRect;
    private bool initializedPosition;
    private bool isOpen;
    private int selectedTab;
    private Vector2 scrollPosition;
    private Texture2D pixel;
    private GUIStyle titleStyle;
    private GUIStyle resourceStyle;
    private GUIStyle tabStyle;
    private GUIStyle itemStyle;
    private GUIStyle costStyle;

    public static Rect CurrentRect { get; private set; }

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        titleStyle = new GUIStyle
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        resourceStyle = new GUIStyle
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.78f, 0.96f, 1f) }
        };
        tabStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };
        itemStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 8, 4, 4)
        };
        costStyle = new GUIStyle
        {
            fontSize = 11,
            normal = { textColor = new Color(0.86f, 0.82f, 1f) },
            alignment = TextAnchor.MiddleRight
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
        if (!isOpen)
        {
            CurrentRect = Rect.zero;
            return;
        }

        EnsurePosition();
        CurrentRect = windowRect;

        DrawRect(windowRect, new Color(0.09f, 0.03f, 0.16f, 0.94f));
        DrawRectOutline(windowRect, new Color(0.78f, 0.38f, 1f, 0.95f), 3f);
        DrawRect(new Rect(windowRect.x, windowRect.y, windowRect.width, HeaderHeight), new Color(0.16f, 0.05f, 0.28f, 0.98f));

        GUI.Label(new Rect(windowRect.x + 14f, windowRect.y + 6f, 320f, 22f), "BUILD MENU [I]", titleStyle);
        DrawResources(new Rect(windowRect.x + 14f, windowRect.y + 42f, windowRect.width - 28f, 22f));
        DrawTabs(new Rect(windowRect.x + 14f, windowRect.y + 74f, windowRect.width - 28f, TabHeight));
        DrawItems(new Rect(windowRect.x + 14f, windowRect.y + 112f, windowRect.width - 28f, windowRect.height - 126f));
    }

    private void EnsurePosition()
    {
        if (initializedPosition)
        {
            return;
        }

        windowRect = new Rect(
            Mathf.Max(12f, (Screen.width - WindowWidth) * 0.5f),
            Mathf.Max(12f, (Screen.height - WindowHeight) * 0.5f),
            Mathf.Min(WindowWidth, Screen.width - 24f),
            Mathf.Min(WindowHeight, Screen.height - 24f));
        initializedPosition = true;
    }

    private void DrawResources(Rect rect)
    {
        string text =
            "ORE " + BuildResourcePool.GetAvailable(ResourceType.Ore) +
            "   ICE " + BuildResourcePool.GetAvailable(ResourceType.Ice) +
            "   SILICATE " + BuildResourcePool.GetAvailable(ResourceType.Silicate) +
            "   COPPER " + BuildResourcePool.GetAvailable(ResourceType.Copper) +
            "   BIOMASS " + BuildResourcePool.GetAvailable(ResourceType.Biomass);

        DrawRect(rect, new Color(0.02f, 0.01f, 0.04f, 0.72f));
        GUI.Label(new Rect(rect.x + 8f, rect.y + 3f, rect.width - 16f, rect.height), text, resourceStyle);
    }

    private void DrawTabs(Rect rect)
    {
        float tabWidth = rect.width / TabNames.Length;
        for (int i = 0; i < TabNames.Length; i++)
        {
            Rect tabRect = new Rect(rect.x + (tabWidth * i), rect.y, tabWidth - 4f, rect.height);
            Color color = i == selectedTab ? new Color(0.4f, 0.14f, 0.62f, 1f) : new Color(0.12f, 0.04f, 0.22f, 1f);
            DrawRect(tabRect, color);
            DrawRectOutline(tabRect, new Color(0.72f, 0.34f, 1f, 0.85f), 2f);
            if (GUI.Button(tabRect, TabNames[i], tabStyle))
            {
                selectedTab = i;
                scrollPosition = Vector2.zero;
            }
        }
    }

    private void DrawItems(Rect rect)
    {
        DrawRect(rect, new Color(0.03f, 0.01f, 0.06f, 0.66f));

        Rect viewRect = new Rect(0f, 0f, rect.width - 20f, ContentHeight());
        scrollPosition = GUI.BeginScrollView(rect, scrollPosition, viewRect);

        float y = 0f;
        IReadOnlyList<BuildingType> planetBuildings = BuildingCatalog.AllPlanetBuildings;
        for (int i = 0; i < planetBuildings.Count; i++)
        {
            BuildingType buildingType = planetBuildings[i];
            BuildingDefinition definition = BuildingCatalog.GetDefinition(buildingType);
            if (!IsVisibleInCurrentTab(definition.category))
            {
                continue;
            }

            DrawPlanetBuildingItem(new Rect(0f, y, viewRect.width, 42f), buildingType, definition);
            y += 48f;
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

            DrawAutomatonItem(new Rect(0f, y, viewRect.width, 42f), option);
            y += 48f;
        }

        GUI.EndScrollView();
    }

    private void DrawPlanetBuildingItem(Rect rect, BuildingType buildingType, BuildingDefinition definition)
    {
        bool unlocked = definition.upgradeId.HasValue && IsUpgradeUnlocked(definition.upgradeId.Value);
        bool affordable = BuildResourcePool.CanAfford(definition.buildCost);
        bool enabled = !unlocked && affordable;
        string status = unlocked ? "Unlocked" : CostText(definition.buildCost);

        DrawItemFrame(rect, enabled, unlocked);
        GUI.enabled = enabled;
        if (GUI.Button(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), definition.displayName, itemStyle))
        {
            StartPlanetBuilding(buildingType);
            isOpen = false;
        }
        GUI.enabled = true;
        GUI.Label(new Rect(rect.x + rect.width - 260f, rect.y + 9f, 246f, 24f), status, costStyle);
    }

    private void DrawAutomatonItem(Rect rect, AutomatonBuildOption option)
    {
        ResourceStack[] cost = AutomatonPlacementController.BuildCostFor(option);
        bool affordable = BuildResourcePool.CanAfford(cost);

        DrawItemFrame(rect, affordable, false);
        GUI.enabled = affordable;
        if (GUI.Button(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), AutomatonPlacementController.DisplayNameFor(option), itemStyle))
        {
            AutomatonPlacementController controller = AutomatonPlacementController.Instance;
            if (controller != null)
            {
                controller.BeginPlacement(option);
                isOpen = false;
            }
        }
        GUI.enabled = true;
        GUI.Label(new Rect(rect.x + rect.width - 260f, rect.y + 9f, 246f, 24f), CostText(cost), costStyle);
    }

    private void DrawItemFrame(Rect rect, bool enabled, bool unlocked)
    {
        Color fill = unlocked
            ? new Color(0.06f, 0.18f, 0.12f, 0.82f)
            : enabled
                ? new Color(0.11f, 0.05f, 0.18f, 0.9f)
                : new Color(0.08f, 0.05f, 0.08f, 0.72f);
        Color line = enabled
            ? new Color(0.75f, 0.4f, 1f, 0.9f)
            : new Color(0.45f, 0.34f, 0.52f, 0.74f);
        DrawRect(rect, fill);
        DrawRectOutline(rect, line, 2f);
    }

    private float ContentHeight()
    {
        int count = 0;
        IReadOnlyList<BuildingType> planetBuildings = BuildingCatalog.AllPlanetBuildings;
        for (int i = 0; i < planetBuildings.Count; i++)
        {
            if (IsVisibleInCurrentTab(BuildingCatalog.GetDefinition(planetBuildings[i]).category))
            {
                count++;
            }
        }

        AutomatonBuildOption[] automata = AutomatonPlacementController.AllBuildOptions;
        for (int i = 0; i < automata.Length; i++)
        {
            if (IsVisibleInCurrentTab(AutomatonPlacementController.CategoryFor(automata[i])))
            {
                count++;
            }
        }

        return Mathf.Max(1, count) * 48f;
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

    private static string CostText(ResourceStack[] cost)
    {
        if (cost == null || cost.Length <= 0)
        {
            return "Free";
        }

        string text = string.Empty;
        for (int i = 0; i < cost.Length; i++)
        {
            if (i > 0)
            {
                text += " + ";
            }

            text += cost[i].amount + " " + cost[i].type;
        }

        return text;
    }

    private static void StartPlanetBuilding(BuildingType buildingType)
    {
        BuildingPlacementController controller = BuildingPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginPlacement(buildingType);
        }
    }

    public static bool ContainsGuiPoint(Vector2 guiPoint)
    {
        return CurrentRect.Contains(guiPoint);
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = previous;
    }

    private void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }
}
