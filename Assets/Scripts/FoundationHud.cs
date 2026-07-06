using UnityEngine;

public class FoundationHud : MonoBehaviour
{
    private ResourceInventory inventory;
    private PlayerMovement playerMovement;
    private PlayerScanner scanner;
    private PlayerRadarPing radarPing;
    private StarSystemGenerator generator;
    private Texture2D pixel;
    private GUIStyle labelStyle;
    private GUIStyle smallLabelStyle;

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        labelStyle = new GUIStyle
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };
        smallLabelStyle = new GUIStyle
        {
            fontSize = 12,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void Update()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<ResourceInventory>();
        }

        if (scanner == null && inventory != null)
        {
            scanner = inventory.GetComponent<PlayerScanner>();
        }

        if (playerMovement == null && inventory != null)
        {
            playerMovement = inventory.GetComponent<PlayerMovement>();
        }

        if (radarPing == null && inventory != null)
        {
            radarPing = inventory.GetComponent<PlayerRadarPing>();
        }

        if (generator == null)
        {
            generator = FindFirstObjectByType<StarSystemGenerator>();
        }
    }

    private void OnGUI()
    {
        if (inventory == null)
        {
            return;
        }

        string scannerState = scanner == null || scanner.IsReady ? "Ready" : "Charging";
        int seed = generator == null ? 0 : generator.seed;
        float speed = playerMovement == null ? 0f : playerMovement.Speed;
        int ore = BuildResourcePool.GetAvailable(ResourceType.Ore, inventory, BuildResourcePool.BuildRange);
        int ice = BuildResourcePool.GetAvailable(ResourceType.Ice, inventory, BuildResourcePool.BuildRange);
        int silicate = BuildResourcePool.GetAvailable(ResourceType.Silicate, inventory, BuildResourcePool.BuildRange);
        int copper = BuildResourcePool.GetAvailable(ResourceType.Copper, inventory, BuildResourcePool.BuildRange);
        int biomass = BuildResourcePool.GetAvailable(ResourceType.Biomass, inventory, BuildResourcePool.BuildRange);

        GUI.Label(
            new Rect(12f, 12f, 560f, 100f),
            "Seed " + seed +
            "\nOre " + ore +
            "  Ice " + ice +
            "  Silicate " + silicate +
            "  Copper " + copper +
            "  Biomass " + biomass +
            "\nScanner " + scannerState +
            "  Speed " + speed.ToString("0.0"),
            labelStyle
        );

        DrawRadarSlot(new Rect(12f, 104f, 64f, 64f));
        DrawRadarPointers();
    }

    private void DrawRadarSlot(Rect rect)
    {
        bool ready = radarPing == null || radarPing.IsReady;
        Color frameColor = ready ? new Color(0.2f, 0.95f, 1f, 0.95f) : new Color(0.18f, 0.36f, 0.42f, 0.95f);
        Color fillColor = ready ? new Color(0.04f, 0.24f, 0.3f, 0.92f) : new Color(0.03f, 0.08f, 0.1f, 0.92f);

        DrawRect(rect, new Color(0f, 0f, 0f, 0.76f));
        DrawRectOutline(rect, frameColor, 3f);
        DrawRect(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f), fillColor);

        if (radarPing != null && !ready)
        {
            float cooldownProgress = 1f - Mathf.Clamp01(radarPing.CooldownRemaining / Mathf.Max(0.01f, radarPing.cooldownSeconds));
            Rect fillRect = new Rect(rect.x + 6f, rect.yMax - 6f - ((rect.height - 12f) * cooldownProgress), rect.width - 12f, (rect.height - 12f) * cooldownProgress);
            DrawRect(fillRect, new Color(0.12f, 0.7f, 0.85f, 0.55f));
        }

        GUI.Label(new Rect(rect.x + 6f, rect.y + 4f, 18f, 18f), "1", smallLabelStyle);
        GUI.Label(new Rect(rect.x + 6f, rect.y + 23f, rect.width - 12f, 18f), "RAD", smallLabelStyle);
        GUI.Label(new Rect(rect.x + 6f, rect.y + 41f, rect.width - 12f, 18f), ready ? "RDY" : radarPing.CooldownRemaining.ToString("0.0"), smallLabelStyle);
    }

    private void DrawRadarPointers()
{
    if (radarPing == null || inventory == null)
    {
        return;
    }

    Vector2 planetPosition;
    Vector2 starPosition;

    System.Collections.Generic.IReadOnlyList<Vector2> asteroidPositions = radarPing.GetPointers(MapMarkerType.Asteroid);
    bool hasAsteroid = asteroidPositions.Count > 0;
    bool hasPlanet = radarPing.TryGetPointer(MapMarkerType.Planet, out planetPosition);
    bool hasStar = radarPing.TryGetPointer(MapMarkerType.Star, out starPosition);

    if (!hasAsteroid && !hasPlanet && !hasStar)
    {
        return;
    }

    float separation = 0f;

    if (hasAsteroid)
    {
        for (int i = 0; i < asteroidPositions.Count; i++)
        {
            DrawEdgePointer("ROCK", asteroidPositions[i], new Color(0.95f, 0.78f, 0.42f, 0.95f), separation);
            separation += 24f;
        }
    }

    if (hasPlanet)
    {
        DrawEdgePointer("PLANET", planetPosition, new Color(0.28f, 0.95f, 1f, 0.95f), separation);
        separation += 24f;
    }

    if (hasStar)
    {
        DrawEdgePointer("SUN", starPosition, new Color(1f, 0.76f, 0.22f, 0.95f), separation);
    }
}

   private void DrawEdgePointer(string label, Vector2 targetPosition, Color color, float separation)
{
    Vector2 targetGuiPosition;
    Vector2 direction;

    if (TryWorldToGuiPosition(targetPosition, out targetGuiPosition))
    {
        if (IsGuiPointOnScreen(targetGuiPosition, 18f))
        {
            DrawOnScreenPointer(label, targetGuiPosition, color);
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        direction = targetGuiPosition - screenCenter;
    }
    else
    {
        Vector2 worldDelta = targetPosition - (Vector2)inventory.transform.position;
        direction = new Vector2(worldDelta.x, -worldDelta.y);
    }

    if (direction.sqrMagnitude < 0.001f)
    {
        return;
    }

    direction.Normalize();

    Vector2 center = EdgePointerPosition(direction, Screen.width, Screen.height, 34f + separation);
    Vector2 tip = center + (direction * 20f);
    Vector2 left = tip - (direction * 8f) + new Vector2(-direction.y, direction.x) * 5f;
    Vector2 right = tip - (direction * 8f) - new Vector2(-direction.y, direction.x) * 5f;
    Rect labelRect = new Rect(center.x - 34f, center.y + 16f, 68f, 18f);

    DrawRect(new Rect(center.x - 28f, center.y - 18f, 56f, 52f), new Color(0f, 0f, 0f, 0.52f));
    DrawRectOutline(new Rect(center.x - 28f, center.y - 18f, 56f, 52f), color, 2f);
    DrawLine(center - (direction * 10f), tip, color, 4f);
    DrawLine(tip, left, color, 4f);
    DrawLine(tip, right, color, 4f);
    GUI.Label(labelRect, label, smallLabelStyle);
}

private bool TryWorldToGuiPosition(Vector2 worldPosition, out Vector2 guiPosition)
{
    guiPosition = Vector2.zero;

    Camera camera = Camera.main;
    if (camera == null)
    {
        return false;
    }

    Vector3 screenPosition = camera.WorldToScreenPoint(new Vector3(worldPosition.x, worldPosition.y, 0f));
    if (screenPosition.z < 0f)
    {
        return false;
    }

    guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
    return true;
}

private static bool IsGuiPointOnScreen(Vector2 guiPosition, float margin)
{
    return guiPosition.x >= margin &&
           guiPosition.x <= Screen.width - margin &&
           guiPosition.y >= margin &&
           guiPosition.y <= Screen.height - margin;
}

private void DrawOnScreenPointer(string label, Vector2 center, Color color)
{
    float size = 28f;
    Rect frameRect = new Rect(center.x - (size * 0.5f), center.y - (size * 0.5f), size, size);
    Rect labelRect = new Rect(center.x - 34f, center.y - 34f, 68f, 18f);

    DrawRect(new Rect(center.x - 20f, center.y - 20f, 40f, 40f), new Color(0f, 0f, 0f, 0.38f));
    DrawRectOutline(frameRect, color, 2f);

    DrawLine(new Vector2(center.x - 16f, center.y), new Vector2(center.x - 6f, center.y), color, 3f);
    DrawLine(new Vector2(center.x + 6f, center.y), new Vector2(center.x + 16f, center.y), color, 3f);
    DrawLine(new Vector2(center.x, center.y - 16f), new Vector2(center.x, center.y - 6f), color, 3f);
    DrawLine(new Vector2(center.x, center.y + 6f), new Vector2(center.x, center.y + 16f), color, 3f);

    GUI.Label(labelRect, label, smallLabelStyle);
}

    public static Vector2 EdgePointerPosition(Vector2 direction, float screenWidth, float screenHeight, float margin)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return new Vector2(screenWidth * 0.5f, margin);
        }

        Vector2 normalized = direction.normalized;
        Vector2 center = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
        float halfWidth = Mathf.Max(1f, (screenWidth * 0.5f) - margin);
        float halfHeight = Mathf.Max(1f, (screenHeight * 0.5f) - margin);
        float scaleX = Mathf.Abs(normalized.x) > 0.001f ? halfWidth / Mathf.Abs(normalized.x) : float.MaxValue;
        float scaleY = Mathf.Abs(normalized.y) > 0.001f ? halfHeight / Mathf.Abs(normalized.y) : float.MaxValue;
        float scale = Mathf.Min(scaleX, scaleY);

        return center + (normalized * scale);
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

    private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;

        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - (width * 0.5f), delta.magnitude, width), pixel);

        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }
}
