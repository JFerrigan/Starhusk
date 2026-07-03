using UnityEngine;

public class BuildOptionsMenu : MonoBehaviour
{
    private const float WindowWidth = 260f;
    private const float WindowHeight = 374f;
    private const float HeaderHeight = 26f;

    private Rect windowRect;
    private bool initializedPosition;
    private bool dragging;
    private Vector2 dragOffset;
    private Texture2D pixel;
    private GUIStyle titleStyle;
    private GUIStyle sectionStyle;

    public static Rect CurrentRect { get; private set; }

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        titleStyle = new GUIStyle
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        sectionStyle = new GUIStyle
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.74f, 0.92f, 1f) }
        };
    }

    private void OnGUI()
    {
        EnsurePosition();
        CurrentRect = windowRect;
        HandleDrag();

        DrawRect(windowRect, new Color(0f, 0f, 0f, 0.82f));
        DrawRectOutline(windowRect, new Color(0.24f, 0.84f, 0.95f, 0.95f), 2f);

        GUI.Label(new Rect(windowRect.x + 12f, windowRect.y + 6f, windowRect.width - 24f, 20f), "Architect's Repository", titleStyle);

        float y = windowRect.y + 38f;
        GUI.Label(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 18f), "Planet Structures", sectionStyle);
        y += 22f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Mine T1", () => StartPlanetBuilding(BuildingType.Mine));
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Condenser T1", () => StartPlanetBuilding(BuildingType.Condenser));
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Harvester T1", () => StartPlanetBuilding(BuildingType.Harvester));
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Dredger T1", () => StartPlanetBuilding(BuildingType.Dredger));

        y += 40f;
        GUI.Label(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 18f), "Space Logistics", sectionStyle);
        y += 22f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Collector Automaton", StartCollector);
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Collector Hub", StartHub);
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Freighter", StartFreighter);
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Freighter Cargo Storage", StartFreighterCargoStorage);
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Satellite Factory", StartSatelliteFactory);
        y += 30f;
        DrawBuildButton(new Rect(windowRect.x + 12f, y, windowRect.width - 24f, 26f), "Stationary Satellite", StartStationarySatellite);
    }

    private void EnsurePosition()
    {
        if (initializedPosition)
        {
            return;
        }

        windowRect = new Rect(
            Mathf.Max(12f, Screen.width - WindowWidth - 24f),
            Mathf.Max(12f, (Screen.height - WindowHeight) * 0.5f),
            WindowWidth,
            WindowHeight);
        initializedPosition = true;
    }

    private void HandleDrag()
    {
        Event current = Event.current;
        if (current == null)
        {
            return;
        }

        Rect dragRect = new Rect(windowRect.x, windowRect.y, windowRect.width, HeaderHeight);
        if (current.type == EventType.MouseDown && current.button == 0 && dragRect.Contains(current.mousePosition))
        {
            dragging = true;
            dragOffset = current.mousePosition - new Vector2(windowRect.x, windowRect.y);
            current.Use();
        }
        else if (current.type == EventType.MouseDrag && dragging)
        {
            windowRect.position = ClampToScreen(current.mousePosition - dragOffset, windowRect.size);
            current.Use();
        }
        else if (current.type == EventType.MouseUp && current.button == 0)
        {
            dragging = false;
        }
    }

    private void DrawBuildButton(Rect rect, string label, System.Action action)
    {
        if (GUI.Button(rect, label) && action != null)
        {
            action();
        }
    }

    private static void StartPlanetBuilding(BuildingType buildingType)
    {
        BuildingPlacementController controller = BuildingPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginPlacement(buildingType);
        }
    }

    private static void StartCollector()
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginCollectorPlacement();
        }
    }

    private static void StartHub()
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginHubPlacement();
        }
    }

    private static void StartFreighter()
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginFreighterPlacement();
        }
    }

    private static void StartFreighterCargoStorage()
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginFreighterCargoStoragePlacement();
        }
    }

    private static void StartSatelliteFactory()
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginSatelliteFactoryPlacement();
        }
    }

    private static void StartStationarySatellite()
    {
        AutomatonPlacementController controller = AutomatonPlacementController.Instance;
        if (controller != null)
        {
            controller.BeginStationarySatellitePlacement();
        }
    }

    private static Vector2 ClampToScreen(Vector2 position, Vector2 size)
    {
        return new Vector2(
            Mathf.Clamp(position.x, 4f, Mathf.Max(4f, Screen.width - size.x - 4f)),
            Mathf.Clamp(position.y, 4f, Mathf.Max(4f, Screen.height - size.y - 4f)));
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
